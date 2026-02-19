using UnityEngine;
using Mirror;
using resource.LobbyScene;
using resource.script;

namespace resource.MainMenuScene
{
    public class CustomNetworkManager : NetworkManager
    {
        [Header("Scene Management")]
        [Scene] public string mainMenuScene = "MainMenuScene";
        [Scene] public string lobbyScene = "LobbyScene";
        [Scene] public string gameScene = "MainGameScene";
        private string selectedGameScene = "MainGameScene";

        [Header("Lobby Settings")]
        public GameObject lobbyPlayerPrefab;

        private LobbyManager lobbyManager;
        private bool playerAddRequested = false;

        void Start()
        {
            // DISABLE auto create player - we handle it manually to prevent duplicates
            if (autoCreatePlayer)
            {
                Debug.Log("[CustomNetworkManager] Disabling autoCreatePlayer to prevent duplicate player creation");
                autoCreatePlayer = false;
            }
            
            // CRITICAL: Check if player prefab is registered
            if (playerPrefab == null && lobbyPlayerPrefab != null)
            {
                Debug.Log("[CustomNetworkManager] Setting playerPrefab to lobbyPlayerPrefab");
                playerPrefab = lobbyPlayerPrefab;
            }
            else if (lobbyPlayerPrefab == null && playerPrefab != null)
            {
                Debug.Log("[CustomNetworkManager] Setting lobbyPlayerPrefab to playerPrefab");
                lobbyPlayerPrefab = playerPrefab;
            }
            
            if (playerPrefab == null)
            {
                Debug.LogError("[CustomNetworkManager] CRITICAL: playerPrefab is not assigned! Players will not spawn!");
            }
            else
            {
                // Check if prefab has NetworkIdentity
                var netId = playerPrefab.GetComponent<NetworkIdentity>();
                if (netId == null)
                {
                    Debug.LogError("[CustomNetworkManager] CRITICAL: playerPrefab is missing NetworkIdentity component!");
                }
                else
                {
                    Debug.Log($"[CustomNetworkManager] playerPrefab configured: {playerPrefab.name}");
                }
            }
        }

        public override void OnStartHost()
        {
            Debug.Log("HOST button pressed – starting host and loading LobbyScene");
            base.OnStartHost();
            
            // Ensure PlayerDataContainer exists
            EnsurePlayerDataContainer();
            
            // Automatically transition to LobbyScene after starting host
            ServerChangeScene(lobbyScene);
        }
        
        void EnsurePlayerDataContainer()
        {
            // Check if PlayerDataContainer already exists
            var existingContainer = FindObjectOfType<PlayerDataContainer>();
            if (existingContainer == null)
            {
                // Create a new GameObject with PlayerDataContainer
                GameObject containerObj = new GameObject("PlayerDataContainer");
                containerObj.AddComponent<PlayerDataContainer>();
                Debug.Log("[CustomNetworkManager] Created PlayerDataContainer");
            }
        }

        public override void OnClientConnect()
        {
            Debug.Log("[CustomNetworkManager] Client connected successfully");
            
            // Call base but don't add player yet - wait for scene to be ready
            base.OnClientConnect();
            
            Debug.Log($"[CustomNetworkManager] Local player: {(NetworkClient.localPlayer != null ? "EXISTS" : "NULL")}");
        }
        
        public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
        {
            Debug.Log($"[CustomNetworkManager] Client changing scene to: {newSceneName}");
            base.OnClientChangeScene(newSceneName, sceneOperation, customHandling);
        }
        
        public override void OnClientSceneChanged()
        {
            base.OnClientSceneChanged();
            Debug.Log($"[CustomNetworkManager] Client scene changed. Local player: {(NetworkClient.localPlayer != null ? "EXISTS" : "NULL")}");
            
            // Manually add player if autoCreatePlayer is disabled and we haven't requested yet
            if (NetworkClient.localPlayer == null && NetworkClient.ready && !playerAddRequested)
            {
                playerAddRequested = true;
                Debug.Log("[CustomNetworkManager] Requesting player spawn...");
                NetworkClient.AddPlayer(); // Use modern Mirror API
            }
            else if (NetworkClient.localPlayer == null)
            {
                Debug.LogWarning("[CustomNetworkManager] WARNING: Local player is null after scene change!");
                
                // Try to find any existing player object
                var existingPlayer = FindObjectOfType<LobbyPlayer>();
                if (existingPlayer != null && existingPlayer.isLocalPlayer)
                {
                    Debug.Log("[CustomNetworkManager] Found existing local player object!");
                    // This shouldn't happen, but if it does, something is wrong with Mirror's player assignment
                }
            }
            else
            {
                var lobbyPlayer = NetworkClient.localPlayer.GetComponent<LobbyPlayer>();
                if (lobbyPlayer == null)
                {
                    Debug.LogError("[CustomNetworkManager] CRITICAL: Local player has no LobbyPlayer component!");
                    Debug.Log($"[CustomNetworkManager] Player object name: {NetworkClient.localPlayer.name}");
                    
                    // Build component list
                    var components = NetworkClient.localPlayer.GetComponents<Component>();
                    string componentList = "";
                    foreach (var comp in components)
                    {
                        componentList += comp.GetType().Name + ", ";
                    }
                    Debug.Log($"[CustomNetworkManager] Components on player: {componentList}");
                }
                else
                {
                    Debug.Log($"[CustomNetworkManager] ✓ Local player ready: {lobbyPlayer.playerName}");
                }
            }
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            Debug.Log($"CustomNetworkManager: Adding player for connection {conn.connectionId}");

            // Check if player already exists for this connection
            if (conn.identity != null)
            {
                Debug.LogWarning($"Player already exists for connection {conn.connectionId}, skipping.");
                return;
            }

            // Spawn LobbyPlayer prefab for this connection
            if (lobbyPlayerPrefab != null)
            {
                GameObject player = Instantiate(lobbyPlayerPrefab);
                NetworkServer.AddPlayerForConnection(conn, player);
                
                // Let LobbyManager handle the rest
                // Note: lobbyManager might be null if scene hasn't loaded yet
                if (lobbyManager != null)
                {
                    lobbyManager.OnPlayerAdded(conn);
                }
                else
                {
                    // Queue the player for later assignment when lobby manager is ready
                    Debug.Log($"LobbyManager not ready yet, queuing player {conn.connectionId} for assignment");
                    StartCoroutine(WaitForLobbyManagerAndAssignPlayer(conn));
                }
            }
            else
            {
                Debug.LogError("LobbyPlayer prefab not assigned! Cannot create player.");
                // Don't call base.OnServerAddPlayer here - it would create a duplicate
                // The connection will remain without a player object
                conn.Disconnect();
            }
        }

        private System.Collections.IEnumerator WaitForLobbyManagerAndAssignPlayer(NetworkConnectionToClient conn)
        {
            // Wait up to 5 seconds for LobbyManager to be ready
            float timeout = 5f;
            float elapsed = 0f;
            
            while (lobbyManager == null && elapsed < timeout)
            {
                // Try to find lobby manager
                lobbyManager = FindObjectOfType<LobbyManager>();
                if (lobbyManager == null)
                {
                    elapsed += 0.1f;
                    yield return new WaitForSeconds(0.1f);
                }
            }
            
            if (lobbyManager != null)
            {
                Debug.Log($"LobbyManager found after {elapsed:F1}s, assigning player {conn.connectionId}");
                lobbyManager.OnPlayerAdded(conn);
            }
            else
            {
                Debug.LogError($"Failed to find LobbyManager after {timeout}s! Player {conn.connectionId} won't be assigned to a plate.");
            }
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            Debug.Log($"Client disconnected: {conn.connectionId}");
            
            if (lobbyManager != null)
            {
                lobbyManager.OnPlayerRemoved(conn);
            }

            base.OnServerDisconnect(conn);
        }

        public override void OnServerSceneChanged(string sceneName)
        {
            base.OnServerSceneChanged(sceneName);
            
            Debug.Log($"[CustomNetworkManager] Server scene changed to: {sceneName}");
            
            // Find lobby manager when in lobby scene
            if (sceneName == lobbyScene)
            {
                lobbyManager = FindObjectOfType<LobbyManager>();
                if (lobbyManager != null)
                {
                    Debug.Log("[CustomNetworkManager] LobbyManager found and connected");
                }
                else
                {
                    Debug.LogWarning("[CustomNetworkManager] LobbyManager not found in LobbyScene!");
                }
            }
            // Handle game scene loading - check if it's ANY game scene
            else if (sceneName != mainMenuScene && sceneName != lobbyScene)
            {
                Debug.Log($"[CustomNetworkManager] Game scene {sceneName} loaded - initializing game");
                // Wait a frame for all clients to be ready
                StartCoroutine(SpawnCarsInGameScene());
            }
        }
        
        System.Collections.IEnumerator SpawnCarsInGameScene()
        {
            // Wait for all players to finish loading
            yield return new WaitForSeconds(0.5f);
            
            var gameSpawnManager = FindObjectOfType<GameSpawnManager>();
            if (gameSpawnManager != null)
            {
                gameSpawnManager.OnSceneLoaded();
            }
            else
            {
                Debug.LogWarning("[CustomNetworkManager] GameSpawnManager not found in game scene!");
            }
        }

        public void SetGameScene(string sceneName)
        {
            selectedGameScene = sceneName;
            Debug.Log($"Selected game scene set to: {sceneName}");
        }

        public void LoadGameScene()
        {
            if (NetworkServer.active)
            {
                Debug.Log($"Loading Game Scene: {selectedGameScene}");
                ServerChangeScene(selectedGameScene);
            }
        }

        public override void OnClientDisconnect()
        {
            // Reset flag so we can add player again on next connection
            playerAddRequested = false;
            Debug.Log("[CustomNetworkManager] Client disconnected, reset playerAddRequested flag");
            
            base.OnClientDisconnect();
        }

        public void ReturnToMainMenu()
        {
            Debug.Log("Returning to MainMenuScene");
            
            // Reset flag
            playerAddRequested = false;
            
            // Stop all network activity
            if (NetworkServer.active)
            {
                NetworkManager.singleton.StopHost();
            }
            else if (NetworkClient.active)
            {
                NetworkManager.singleton.StopClient();
            }
            
            // Load main menu scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuScene);
        }
    }
}
