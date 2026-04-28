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

        [Header("Offline Mode")]
        public MapData[] offlineChallengeMaps;
        private bool offlineModeActive;

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
            Debug.Log("[CustomNetworkManager] HOST button pressed – starting host");
            Debug.Log($"[CustomNetworkManager] Server will listen on port: {GetComponent<Mirror.TelepathyTransport>()?.port ?? 7777}");
            base.OnStartHost();
            
            // Verify server actually started
            if (NetworkServer.active)
            {
                Debug.Log("[CustomNetworkManager] ✓ Server is now ACTIVE and listening for connections");
            }
            else
            {
                Debug.LogError("[CustomNetworkManager] ✗ Server failed to start!");
            }
            
            // Ensure PlayerDataContainer exists (now a regular MonoBehaviour, not NetworkBehaviour)
            EnsurePlayerDataContainer();

            if (offlineModeActive && OfflineRaceConfig.Instance != null && OfflineRaceConfig.Instance.IsOfflineMode)
            {
                selectedGameScene = string.IsNullOrEmpty(OfflineRaceConfig.Instance.SelectedSceneName)
                    ? gameScene
                    : OfflineRaceConfig.Instance.SelectedSceneName;
                ServerChangeScene(selectedGameScene);
            }
            else
            {
                // Automatically transition to LobbyScene after starting host
                ServerChangeScene(lobbyScene);
            }
        }
        
        void EnsurePlayerDataContainer()
        {
            // Check if PlayerDataContainer already exists
            var existingContainer = FindObjectOfType<PlayerDataContainer>();
            if (existingContainer == null)
            {
                // Create a new GameObject with PlayerDataContainer
                // Note: PlayerDataContainer is now a MonoBehaviour (not NetworkBehaviour)
                // so it doesn't need NetworkServer.Spawn - it just persists via DontDestroyOnLoad
                GameObject containerObj = new GameObject("PlayerDataContainer");
                containerObj.AddComponent<PlayerDataContainer>();
                Debug.Log("[CustomNetworkManager] Created PlayerDataContainer");
            }
        }

        public override void OnClientConnect()
        {
            Debug.Log("[CustomNetworkManager] ✓ Client connected successfully to server!");
            Debug.Log($"[CustomNetworkManager] Connected to: {NetworkManager.singleton.networkAddress}");
            
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
                if (offlineModeActive)
                {
                    SeedOfflinePlayerData(conn);
                }
                else if (lobbyManager != null)
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
            
            // Clean up disconnected player data to prevent orphaned cars
            if (PlayerDataContainer.Instance != null)
            {
                PlayerDataContainer.Instance.RemovePlayerData(conn.connectionId);
            }

            base.OnServerDisconnect(conn);
        }

        public override void OnServerSceneChanged(string sceneName)
        {
            base.OnServerSceneChanged(sceneName);
            
            Debug.Log($"[CustomNetworkManager] Server scene changed to: {sceneName}");
            
            // Find lobby manager when in lobby scene
            // Use Contains instead of == because scene paths differ between Editor and Build
            // Editor: "Assets/Scenes/LobbyScene.unity"  Build: may be just "LobbyScene" or different path
            if (IsSceneMatch(sceneName, "LobbyScene"))
            {
                lobbyManager = FindObjectOfType<LobbyManager>();
                if (lobbyManager != null)
                {
                    Debug.Log("[CustomNetworkManager] LobbyManager found and connected");
                }
                else
                {
                    Debug.LogWarning("[CustomNetworkManager] LobbyManager not found in LobbyScene!");
                    StartCoroutine(RetryFindLobbyManager());
                }
            }
            // Handle game scene loading - check if it's ANY game scene
            else if (!IsSceneMatch(sceneName, "MainMenu") && !IsSceneMatch(sceneName, "LobbyScene"))
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

        public void StartOfflineGame(MapData mapData, int selectedCarIndex)
        {
            if (mapData == null)
            {
                Debug.LogError("[CustomNetworkManager] Cannot start offline game - mapData is null");
                return;
            }

            OfflineRaceConfig offlineConfig = OfflineRaceConfig.EnsureExists();
            offlineConfig.Configure(selectedCarIndex, GetOfflineMapIndex(mapData), mapData.mapName, mapData.sceneName);

            ChallengeProgressService.EnsureExists().SetSelectedOfflineCarIndex(selectedCarIndex);
            offlineModeActive = true;
            selectedGameScene = mapData.sceneName;

            if (!NetworkServer.active && !NetworkClient.active)
            {
                StartHost();
            }
            else if (NetworkServer.active)
            {
                LoadOfflineChallenge(offlineConfig.SelectedMapIndex);
            }
        }

        public bool TryLoadNextOfflineChallenge()
        {
            if (!offlineModeActive || OfflineRaceConfig.Instance == null)
                return false;

            int currentMapIndex = OfflineRaceConfig.Instance.SelectedMapIndex;
            var progressService = ChallengeProgressService.EnsureExists();

            if (!progressService.TryGetNextChallengeIndex(currentMapIndex, offlineChallengeMaps != null ? offlineChallengeMaps.Length : 0, out int nextMapIndex))
                return false;

            LoadOfflineChallenge(nextMapIndex);
            return true;
        }

        public void LoadOfflineChallenge(int mapIndex)
        {
            if (!NetworkServer.active || offlineChallengeMaps == null || mapIndex < 0 || mapIndex >= offlineChallengeMaps.Length)
                return;

            MapData nextMap = offlineChallengeMaps[mapIndex];
            if (nextMap == null)
                return;

            OfflineRaceConfig offlineConfig = OfflineRaceConfig.EnsureExists();
            offlineConfig.Configure(offlineConfig.SelectedCarIndex, mapIndex, nextMap.mapName, nextMap.sceneName);
            selectedGameScene = nextMap.sceneName;

            if (PlayerDataContainer.Instance != null)
            {
                foreach (var conn in NetworkServer.connections.Values)
                {
                    if (conn != null)
                    {
                        SeedOfflinePlayerData(conn);
                        break;
                    }
                }
            }

            ServerChangeScene(selectedGameScene);
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
            
            // Log additional info for debugging connection issues
            if (!NetworkClient.isConnected)
            {
                Debug.Log("[CustomNetworkManager] Tip: Make sure the Host is running before clicking Join");
                Debug.Log("[CustomNetworkManager] Check that no firewall is blocking port 7777");
            }
            
            base.OnClientDisconnect();
        }

        /// <summary>
        /// Compare scene name robustly — works in both Editor (full path) and Build (short name).
        /// </summary>
        bool IsSceneMatch(string scenePath, string sceneKeyword)
        {
            if (string.IsNullOrEmpty(scenePath)) return false;
            return scenePath.IndexOf(sceneKeyword, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        System.Collections.IEnumerator RetryFindLobbyManager()
        {
            for (int i = 0; i < 20; i++)
            {
                yield return new WaitForSeconds(0.25f);
                lobbyManager = FindObjectOfType<LobbyManager>();
                if (lobbyManager != null)
                {
                    Debug.Log($"[CustomNetworkManager] LobbyManager found on retry #{i + 1}");
                    yield break;
                }
            }
            Debug.LogError("[CustomNetworkManager] LobbyManager not found after 5s of retries!");
        }

        public void ReturnToMainMenu()
        {
            Debug.Log("Returning to MainMenuScene");
            
            // Reset flag
            playerAddRequested = false;
            offlineModeActive = false;

            if (OfflineRaceConfig.Instance != null)
                OfflineRaceConfig.Instance.Clear();
            
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

        int GetOfflineMapIndex(MapData mapData)
        {
            if (offlineChallengeMaps == null || mapData == null)
                return 0;

            for (int i = 0; i < offlineChallengeMaps.Length; i++)
            {
                if (offlineChallengeMaps[i] == mapData)
                    return i;
            }

            return 0;
        }

        void SeedOfflinePlayerData(NetworkConnectionToClient conn)
        {
            if (conn == null)
                return;

            OfflineRaceConfig offlineConfig = OfflineRaceConfig.Instance;
            PlayerDataContainer container = PlayerDataContainer.Instance ?? FindObjectOfType<PlayerDataContainer>();
            if (offlineConfig == null || container == null)
                return;

            string playerName = UserSession.Instance != null && !string.IsNullOrEmpty(UserSession.Instance.DisplayName)
                ? UserSession.Instance.DisplayName
                : "Offline Player";

            container.UpsertPlayerData(new PlayerDataContainer.PlayerGameData(
                conn.connectionId,
                playerName,
                offlineConfig.SelectedCarIndex,
                offlineConfig.SelectedMapIndex,
                true
            ));
        }
    }
}
