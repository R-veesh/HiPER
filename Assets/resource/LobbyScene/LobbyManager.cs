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

        private List<LobbyPlayer> lobbyPlayers = new List<LobbyPlayer>();
        private bool[] usedSpawnPoints;
        private Dictionary<int, int> mapVoteCounts = new Dictionary<int, int>();

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            if (spawnPoints != null)
                usedSpawnPoints = new bool[spawnPoints.Length];
            else
                usedSpawnPoints = new bool[0];
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            Debug.Log("LobbyManager started on server");
            InitializeMapVoting();
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
            Debug.Log($"Adding player for connection {conn.connectionId}");

            if (conn.identity == null)
            {
                Debug.LogError($"Connection {conn.connectionId} has no identity!");
                return;
            }

            // Find available spawn point
            int spawnIndex = GetAvailableSpawnPoint();
            if (spawnIndex == -1)
            {
                Debug.LogError("No available spawn points!");
                return;
            }

            // Get the already spawned player (spawned by CustomNetworkManager)
            GameObject player = conn.identity.gameObject;
            var lobbyPlayer = player.GetComponent<LobbyPlayer>();
            
            if (lobbyPlayer == null)
            {
                Debug.LogError($"Player object missing LobbyPlayer component!");
                return;
            }

            // Assign spawn point to player
            lobbyPlayer.SetPlatePosition(spawnPoints[spawnIndex], spawnIndex);
            
            // Move player to spawn position
            player.transform.position = spawnPoints[spawnIndex].position;
            player.transform.rotation = spawnPoints[spawnIndex].rotation;

            // Mark spawn point as used
            usedSpawnPoints[spawnIndex] = true;
            lobbyPlayers.Add(lobbyPlayer);
            connectedPlayerCount = lobbyPlayers.Count;

            Debug.Log($"Player {conn.connectionId} assigned to plate {spawnIndex + 1}");
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

        public bool AllPlayersReady()
        {
            if (lobbyPlayers.Count == 0) return false;
            
            foreach (var player in lobbyPlayers)
            {
                if (!player.isReady)
                    return false;
            }
            return true;
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
                Debug.Log("Not all players are ready!");
                return;
            }

            Debug.Log("Host clicked start! Starting countdown...");
            
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
            Debug.Log("Starting game!");
            var networkManager = NetworkManager.singleton as CustomNetworkManager;
            if (networkManager != null)
            {
                // Set the selected map scene
                if (availableMaps != null && availableMaps.Length > selectedMapIndex && availableMaps[selectedMapIndex] != null)
                {
                    networkManager.SetGameScene(availableMaps[selectedMapIndex].sceneName);
                }
                
                networkManager.LoadGameScene();
            }
        }

        public void OnReadyClicked()
        {
            if (NetworkClient.localPlayer != null)
            {
                NetworkClient.localPlayer.GetComponent<LobbyPlayer>()?.CmdSetReady();
                
                // Check if we should start countdown
                if (isServer && LobbyCountdown.Instance != null)
                {
                    LobbyCountdown.Instance.OnPlayerReadinessChanged(this);
                }
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
