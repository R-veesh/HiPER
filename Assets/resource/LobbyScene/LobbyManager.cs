using Mirror;
using UnityEngine;
using System.Collections.Generic;
using resource.MainMenuScene;

namespace resource.LobbyScene
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class LobbyManager : NetworkBehaviour
    {
        public static LobbyManager Instance;

        [Header("Spawn Points")]
        public Transform[] spawnPoints;

        [Header("Player Prefab")]
        public GameObject lobbyPlayerPrefab;

        [Header("Map Settings")]
        public MapData[] availableMaps;
        public int selectedMapIndex = 0;
        [SyncVar] public int currentMapVotes = 0;

        [Header("Player Management")]
        [SyncVar] public int connectedPlayerCount = 0;
        [SyncVar] public int readyPlayerCount = 0;

        public List<LobbyPlayer> lobbyPlayers = new List<LobbyPlayer>();
        
        // SyncList to track which players are ready across network
        private readonly SyncList<bool> playerReadyStates = new SyncList<bool>();

        private bool[] usedSpawnPoints;
        private Dictionary<int, int> mapVoteCounts = new Dictionary<int, int>();

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                usedSpawnPoints = new bool[spawnPoints.Length];
                Debug.Log($"[LobbyManager] Initialized with {spawnPoints.Length} spawn points");
            }
            else
            {
                usedSpawnPoints = new bool[0];
                Debug.LogError("[LobbyManager] CRITICAL: No spawn points assigned! Players cannot spawn.");
            }
            
            if (lobbyPlayerPrefab == null)
            {
                Debug.LogError("[LobbyManager] CRITICAL: LobbyPlayer prefab not assigned!");
            }
            else
            {
                // Check if car prefabs are assigned in the prefab
                var lobbyPlayer = lobbyPlayerPrefab.GetComponent<LobbyPlayer>();
                if (lobbyPlayer != null)
                {
                    if (lobbyPlayer.carPrefabs == null || lobbyPlayer.carPrefabs.Length == 0)
                    {
                        Debug.LogError("[LobbyManager] CRITICAL: LobbyPlayer prefab has no car prefabs assigned!");
                    }
                    else
                    {
                        Debug.Log($"[LobbyManager] LobbyPlayer prefab has {lobbyPlayer.carPrefabs.Length} car prefabs");
                    }
                }
                else
                {
                    Debug.LogError("[LobbyManager] CRITICAL: LobbyPlayer prefab missing LobbyPlayer component!");
                }
            }
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            Debug.Log("LobbyManager started on server");
            InitializeMapVoting();
            
            // Subscribe to SyncList changes
            playerReadyStates.Callback += OnReadyStatesChanged;
        }
        
        void OnReadyStatesChanged(SyncList<bool>.Operation op, int itemIndex, bool oldItem, bool newItem)
        {
            Debug.Log($"[LobbyManager] Ready states changed: {op} at index {itemIndex}, value: {newItem}");
            UpdateReadyCount();
        }
        
        [Server]
        void UpdateReadyCount()
        {
            int readyCount = 0;
            foreach (var ready in playerReadyStates)
            {
                if (ready) readyCount++;
            }
            readyPlayerCount = readyCount;
            Debug.Log($"[LobbyManager] Updated ready count: {readyPlayerCount}/{connectedPlayerCount}");
        }

        void Update()
        {
            // Periodic check to sync players that might have been missed
            if (isServer && lobbyPlayers.Count == 0)
            {
                SyncMissingPlayers();
            }
            
            // Server periodically validates player counts
            if (isServer && Time.frameCount % 60 == 0) // Every 60 frames
            {
                ValidatePlayerCounts();
            }
        }
        
        [Server]
        void ValidatePlayerCounts()
        {
            // Ensure player counts match actual connections
            int actualConnections = NetworkServer.connections.Count;
            if (actualConnections != connectedPlayerCount)
            {
                Debug.LogWarning($"[LobbyManager] Player count mismatch! SyncVar: {connectedPlayerCount}, Actual: {actualConnections}. Resyncing...");
                ResyncPlayerList();
            }
        }
        
        [Server]
        void ResyncPlayerList()
        {
            lobbyPlayers.Clear();
            playerReadyStates.Clear();
            
            foreach (var conn in NetworkServer.connections.Values)
            {
                if (conn?.identity != null)
                {
                    var lobbyPlayer = conn.identity.GetComponent<LobbyPlayer>();
                    if (lobbyPlayer != null)
                    {
                        lobbyPlayers.Add(lobbyPlayer);
                        playerReadyStates.Add(lobbyPlayer.isReady);
                    }
                }
            }
            
            connectedPlayerCount = lobbyPlayers.Count;
            UpdateReadyCount();
            
            Debug.Log($"[LobbyManager] Resynced {connectedPlayerCount} players");
        }

        void SyncMissingPlayers()
        {
            Debug.Log("[LobbyManager] Checking for missing players...");
            
            // Find all LobbyPlayers in the scene
            var allPlayers = FindObjectsOfType<LobbyPlayer>();
            Debug.Log($"[LobbyManager] Found {allPlayers.Length} LobbyPlayers in scene");
            
            foreach (var player in allPlayers)
            {
                if (!lobbyPlayers.Contains(player))
                {
                    Debug.Log($"[LobbyManager] Adding missing player: {player.playerName}");
                    
                    // Find available spawn point
                    int spawnIndex = GetAvailableSpawnPoint();
                    if (spawnIndex == -1)
                    {
                        Debug.LogError("[LobbyManager] No available spawn points for missing player!");
                        continue;
                    }
                    
                    // Assign spawn point
                    player.SetPlatePosition(spawnPoints[spawnIndex], spawnIndex);
                    player.transform.position = spawnPoints[spawnIndex].position;
                    player.transform.rotation = spawnPoints[spawnIndex].rotation;
                    
                    // Mark spawn point as used
                    usedSpawnPoints[spawnIndex] = true;
                    
                    // Add to list
                    lobbyPlayers.Add(player);
                    connectedPlayerCount = lobbyPlayers.Count;
                    
                    Debug.Log($"[LobbyManager] ✓ Player {player.playerName} synced. Total: {connectedPlayerCount}");
                }
            }
        }

        void InitializeMapVoting()
        {
            mapVoteCounts.Clear();
            for (int i = 0; i < availableMaps.Length; i++)
            {
                mapVoteCounts[i] = 0;
            }
        }

        public void OnPlayerAdded(NetworkConnectionToClient conn)
        {
            Debug.Log($"[LobbyManager] OnPlayerAdded called for connection {conn.connectionId}");
            Debug.Log($"[LobbyManager] Current player count before add: {lobbyPlayers.Count}");

            if (conn.identity == null)
            {
                Debug.LogError($"[LobbyManager] Connection {conn.connectionId} has no identity!");
                return;
            }
            
            Debug.Log($"[LobbyManager] Connection identity: {conn.identity.gameObject.name}");

            // Find available spawn point
            int spawnIndex = GetAvailableSpawnPoint();
            if (spawnIndex == -1)
            {
                Debug.LogError("[LobbyManager] No available spawn points!");
                return;
            }
            
            Debug.Log($"[LobbyManager] Assigning to spawn point {spawnIndex}");

            // Get the already spawned player (spawned by CustomNetworkManager)
            GameObject player = conn.identity.gameObject;
            var lobbyPlayer = player.GetComponent<LobbyPlayer>();
            
            if (lobbyPlayer == null)
            {
                Debug.LogError($"[LobbyManager] Player object missing LobbyPlayer component!");
                Debug.Log($"[LobbyManager] Player object components: ");
                var components = player.GetComponents<Component>();
                foreach (var comp in components)
                {
                    Debug.Log($"  - {comp.GetType().Name}");
                }
                return;
            }
            
            Debug.Log($"[LobbyManager] Found LobbyPlayer: {lobbyPlayer.playerName}");

            // Assign spawn point to player
            lobbyPlayer.SetPlatePosition(spawnPoints[spawnIndex], spawnIndex);
            
            // Move player to spawn position
            player.transform.position = spawnPoints[spawnIndex].position;
            player.transform.rotation = spawnPoints[spawnIndex].rotation;

            // Mark spawn point as used
            usedSpawnPoints[spawnIndex] = true;
            
            // Add to list and update count
            Debug.Log($"[LobbyManager] Adding player to lobbyPlayers list. Current count: {lobbyPlayers.Count}");
            lobbyPlayers.Add(lobbyPlayer);
            
            // Add to ready states list
            playerReadyStates.Add(lobbyPlayer.isReady);
            
            connectedPlayerCount = lobbyPlayers.Count;
            UpdateReadyCount();
            
            Debug.Log($"[LobbyManager] Player {conn.connectionId} assigned to plate {spawnIndex + 1}. Total players: {connectedPlayerCount}");
            
            // Verify the player was added
            if (lobbyPlayers.Contains(lobbyPlayer))
            {
                Debug.Log($"[LobbyManager] ✓ Player successfully added to list");
            }
            else
            {
                Debug.LogError($"[LobbyManager] ✗ Player was NOT added to list!");
            }
        }


        public void OnPlayerRemoved(NetworkConnectionToClient conn)
        {
            if (conn.identity != null)
            {
                var lobbyPlayer = conn.identity.GetComponent<LobbyPlayer>();
                if (lobbyPlayer != null)
                {
                    // Free up spawn point
                    if (lobbyPlayer.plateIndex >= 0 && lobbyPlayer.plateIndex < spawnPoints.Length)
                    {
                        usedSpawnPoints[lobbyPlayer.plateIndex] = false;
                        
                        // Remove map vote
                        if (mapVoteCounts.ContainsKey(lobbyPlayer.selectedMapIndex))
                        {
                            mapVoteCounts[lobbyPlayer.selectedMapIndex]--;
                        }
                    }

                    lobbyPlayers.Remove(lobbyPlayer);
                    connectedPlayerCount = lobbyPlayers.Count;
                    
                    // Check countdown status
                    if (LobbyCountdown.Instance != null)
                    {
                        LobbyCountdown.Instance.OnPlayerReadinessChanged(this);
                    }
                }
            }
        }

        [Server]
        public void OnPlayerVotedForMap()
        {
            // Recalculate map votes
            InitializeMapVoting();
            int maxVotes = 0;
            int winningMapIndex = 0;

            foreach (var player in lobbyPlayers)
            {
                if (mapVoteCounts.ContainsKey(player.selectedMapIndex))
                {
                    mapVoteCounts[player.selectedMapIndex]++;
                    
                    // Track winning map
                    if (mapVoteCounts[player.selectedMapIndex] > maxVotes)
                    {
                        maxVotes = mapVoteCounts[player.selectedMapIndex];
                        winningMapIndex = player.selectedMapIndex;
                    }
                }
            }

            // Update selected map
            selectedMapIndex = winningMapIndex;
            currentMapVotes = maxVotes;
            
            string mapName = (availableMaps != null && availableMaps.Length > selectedMapIndex && availableMaps[selectedMapIndex] != null) 
                ? availableMaps[selectedMapIndex].mapName 
                : "Unknown";
            Debug.Log($"Map voting updated. Winning map: {mapName} with {maxVotes} votes");
            
            // Notify all clients
            RpcUpdateMapSelection(selectedMapIndex, maxVotes);
        }

        [ClientRpc]
        void RpcUpdateMapSelection(int mapIndex, int votes)
        {
            selectedMapIndex = mapIndex;
            currentMapVotes = votes;
        }

        [Server]
        public void ForceMapSelection(int mapIndex)
        {
            if (mapIndex >= 0 && mapIndex < availableMaps.Length)
            {
                selectedMapIndex = mapIndex;
                RpcUpdateMapSelection(mapIndex, lobbyPlayers.Count);
            }
        }

        int GetAvailableSpawnPoint()
        {
            for (int i = 0; i < usedSpawnPoints.Length; i++)
            {
                if (!usedSpawnPoints[i])
                    return i;
            }
            return -1;
        }

        [Header("Player Requirements")]
        [Tooltip("Minimum players required to start (1-4)")]
        public int minPlayers = 1;
        [Tooltip("Maximum players allowed (1-4)")]
        public int maxPlayers = 4;

        public bool AllPlayersReady()
        {
            // Use actual connection count for more accuracy
            int actualPlayerCount = isServer ? NetworkServer.connections.Count : connectedPlayerCount;
            
            Debug.Log($"[LobbyManager] Checking AllPlayersReady: actualCount={actualPlayerCount}, min={minPlayers}, max={maxPlayers}, readyCount={readyPlayerCount}");
            
            // Must have at least minPlayers to start
            if (actualPlayerCount < minPlayers)
            {
                Debug.Log($"[LobbyManager] Not enough players: {actualPlayerCount}/{minPlayers}");
                return false;
            }
            
            // Must have at most maxPlayers
            if (actualPlayerCount > maxPlayers)
            {
                Debug.Log($"[LobbyManager] Too many players: {actualPlayerCount}/{maxPlayers}");
                return false;
            }
            
            // All connected players must be ready
            // Check both local list and SyncList for redundancy
            bool allReady = true;
            
            // Check local list
            foreach (var player in lobbyPlayers)
            {
                if (!player.isReady)
                {
                    Debug.Log($"[LobbyManager] Player {player.playerName} is NOT ready");
                    allReady = false;
                    break;
                }
            }
            
            // Also verify ready count matches player count
            if (readyPlayerCount != actualPlayerCount)
            {
                Debug.Log($"[LobbyManager] Ready count mismatch: {readyPlayerCount}/{actualPlayerCount}");
                allReady = false;
            }
            
            Debug.Log($"[LobbyManager] AllPlayersReady result: {allReady}");
            return allReady;
        }

        public string GetReadyStatusMessage()
        {
            int playerCount = isServer ? NetworkServer.connections.Count : connectedPlayerCount;
            int readyCount = readyPlayerCount;
            
            Debug.Log($"[LobbyManager] GetReadyStatusMessage: players={playerCount}, ready={readyCount}, min={minPlayers}, max={maxPlayers}");
            
            if (playerCount < minPlayers)
            {
                return $"Need {minPlayers - playerCount} more player(s)";
            }
            else if (playerCount > maxPlayers)
            {
                return $"Too many players (max {maxPlayers})";
            }
            else if (readyCount < playerCount)
            {
                return $"Waiting for {playerCount - readyCount} player(s) to ready";
            }
            else
            {
                return "All players ready!";
            }
        }

        public int GetReadyPlayerCount()
        {
            int count = 0;
            foreach (var player in lobbyPlayers)
            {
                if (player.isReady)
                    count++;
            }
            return count;
        }

        public void OnStartClicked()
        {
            if (!isServer) return; // HOST ONLY

            if (!AllPlayersReady())
            {
                string statusMessage = GetReadyStatusMessage();
                Debug.Log($"[LobbyManager] Cannot start: {statusMessage}");
                return;
            }

            Debug.Log($"[LobbyManager] Host clicked start! {lobbyPlayers.Count} players ready. Starting countdown...");
            
            // Start countdown instead of immediate start
            if (LobbyCountdown.Instance != null)
            {
                LobbyCountdown.Instance.StartCountdown();
            }
            else
            {
                // Fallback: start immediately if countdown system not available
                StartGame();
            }
        }

        [Server]
        public void StartGame()
        {
            Debug.Log("[LobbyManager] Starting game!");
            
            // Save all player data before scene change
            var playerDataContainer = FindObjectOfType<resource.MainMenuScene.PlayerDataContainer>();
            if (playerDataContainer != null)
            {
                playerDataContainer.SaveAllPlayerData();
            }
            else
            {
                Debug.LogWarning("[LobbyManager] PlayerDataContainer not found! Player selections won't persist to game.");
            }
            
            var networkManager = NetworkManager.singleton as resource.MainMenuScene.CustomNetworkManager;
            if (networkManager != null)
            {
                // Set the selected map scene
                if (availableMaps != null && availableMaps.Length > selectedMapIndex && availableMaps[selectedMapIndex] != null)
                {
                    networkManager.SetGameScene(availableMaps[selectedMapIndex].sceneName);
                }
                
                networkManager.LoadGameScene();
            }
            else
            {
                Debug.LogError("[LobbyManager] Cannot start game - CustomNetworkManager not found!");
            }
        }

        public void OnReadyClicked()
        {
            if (NetworkClient.localPlayer != null)
            {
                var localLobbyPlayer = NetworkClient.localPlayer.GetComponent<LobbyPlayer>();
                if (localLobbyPlayer != null)
                {
                    Debug.Log($"[LobbyManager] Local player clicking ready. Current state: {localLobbyPlayer.isReady}");
                    localLobbyPlayer.CmdSetReady();
                    
                    // Notify server to update ready states
                    if (isServer)
                    {
                        UpdatePlayerReadyState(localLobbyPlayer);
                    }
                }
                
                // Check if we should start countdown
                if (isServer && LobbyCountdown.Instance != null)
                {
                    LobbyCountdown.Instance.OnPlayerReadinessChanged(this);
                }
            }
        }
        
        [Server]
        public void UpdatePlayerReadyState(LobbyPlayer player)
        {
            // Find player index in list
            int index = lobbyPlayers.IndexOf(player);
            if (index >= 0 && index < playerReadyStates.Count)
            {
                playerReadyStates[index] = player.isReady;
                UpdateReadyCount();
                Debug.Log($"[LobbyManager] Updated ready state for {player.playerName}: {player.isReady}");
            }
            else if (index >= 0)
            {
                // Index out of range, resync
                Debug.LogWarning($"[LobbyManager] Ready state index out of range, resyncing...");
                ResyncPlayerList();
            }
        }


        public MapData GetSelectedMap()
        {
            if (availableMaps != null && availableMaps.Length > selectedMapIndex)
            {
                return availableMaps[selectedMapIndex];
            }
            return null;
        }

        public List<LobbyPlayer> GetLobbyPlayers()
        {
            return lobbyPlayers;
        }
    }
}
