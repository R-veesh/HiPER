using UnityEngine;
using Mirror;
using System.Collections.Generic;
using resource.MainMenuScene;

namespace resource.script
{
    public class GameSpawnManager : NetworkBehaviour
    {
        public static GameSpawnManager Instance;
        
        [Header("Car Prefabs")]
        public GameObject[] carPrefabs;
        
        [Header("Spawn Points")]
        public Transform[] spawnPoints;
        
        [Header("Player Data")]
        public bool usePlayerDataContainer = true;
        
        private List<GameObject> spawnedCars = new List<GameObject>();
        private int nextSpawnIndex;
        
        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }
        
        public override void OnStartServer()
        {
            base.OnStartServer();
            Debug.Log("[GameSpawnManager] Started on server");
            
            // Spawn cars for all connected players using saved data
            if (usePlayerDataContainer)
            {
                SpawnCarsFromSavedData();
            }
            else
            {
                SpawnCarsForConnectedPlayers();
            }
        }
        
        /// <summary>
        /// Spawn cars using data saved in PlayerDataContainer (recommended)
        /// </summary>
        [Server]
        void SpawnCarsFromSavedData()
        {
            var playerDataContainer = FindObjectOfType<PlayerDataContainer>();
            
            if (playerDataContainer == null)
            {
                Debug.LogError("[GameSpawnManager] PlayerDataContainer not found! Falling back to connected players.");
                SpawnCarsForConnectedPlayers();
                return;
            }
            
            var playerDataList = playerDataContainer.GetAllPlayerData();
            Debug.Log($"[GameSpawnManager] Spawning cars for {playerDataList.Count} players from saved data");
            
            foreach (var playerData in playerDataList)
            {
                SpawnCarForPlayerData(playerData);
            }
        }
        
        /// <summary>
        /// Spawn cars for currently connected players (fallback method)
        /// </summary>
        [Server]
        void SpawnCarsForConnectedPlayers()
        {
            var connections = NetworkServer.connections;
            Debug.Log($"[GameSpawnManager] Spawning cars for {connections.Count} connected players");
            
            foreach (var conn in connections.Values)
            {
                if (conn != null && conn.isReady)
                {
                    SpawnCarForConnection(conn);
                }
            }
        }
        
        [Server]
        void SpawnCarForPlayerData(PlayerDataContainer.PlayerGameData playerData)
        {
            if (carPrefabs == null || carPrefabs.Length == 0)
            {
                Debug.LogError("[GameSpawnManager] No car prefabs assigned!");
                return;
            }
            
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogError("[GameSpawnManager] No spawn points assigned!");
                return;
            }
            
            // Get selected car index (ensure it's valid)
            int carIndex = Mathf.Clamp(playerData.selectedCarIndex, 0, carPrefabs.Length - 1);
            
            // Get spawn position
            int spawnIndex = nextSpawnIndex % spawnPoints.Length;
            Transform spawnPoint = spawnPoints[spawnIndex];
            nextSpawnIndex++;
            
            // Spawn the car
            GameObject car = Instantiate(carPrefabs[carIndex], spawnPoint.position, spawnPoint.rotation);
            NetworkServer.Spawn(car);
            
            // Find the connection for this player
            NetworkConnectionToClient conn = null;
            if (NetworkServer.connections.TryGetValue(playerData.connectionId, out conn))
            {
                // Assign car to player
                AssignCarToPlayer(car, conn);
                Debug.Log($"[GameSpawnManager] Spawned car {carPrefabs[carIndex].name} for player {playerData.playerName} at spawn point {spawnIndex}");
            }
            else
            {
                Debug.LogWarning($"[GameSpawnManager] Connection {playerData.connectionId} not found for player {playerData.playerName}. Car spawned without ownership.");
            }
            
            spawnedCars.Add(car);
        }
        
        [Server]
        void SpawnCarForConnection(NetworkConnectionToClient conn)
        {
            if (carPrefabs == null || carPrefabs.Length == 0)
            {
                Debug.LogError("[GameSpawnManager] No car prefabs assigned!");
                return;
            }
            
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogError("[GameSpawnManager] No spawn points assigned!");
                return;
            }
            
            // Try to get saved data for this connection
            var playerDataContainer = FindObjectOfType<PlayerDataContainer>();
            int carIndex = 0;
            string playerName = $"Player {conn.connectionId}";
            
            if (playerDataContainer != null)
            {
                var data = playerDataContainer.GetPlayerData(conn.connectionId);
                if (data != null)
                {
                    carIndex = data.selectedCarIndex;
                    playerName = data.playerName;
                }
            }
            
            carIndex = Mathf.Clamp(carIndex, 0, carPrefabs.Length - 1);
            
            // Get spawn position
            int spawnIndex = nextSpawnIndex % spawnPoints.Length;
            Transform spawnPoint = spawnPoints[spawnIndex];
            nextSpawnIndex++;
            
            // Spawn the car
            GameObject car = Instantiate(carPrefabs[carIndex], spawnPoint.position, spawnPoint.rotation);
            NetworkServer.Spawn(car);
            
            // Assign car to player
            AssignCarToPlayer(car, conn);
            
            spawnedCars.Add(car);
            
            Debug.Log($"[GameSpawnManager] Spawned car {carPrefabs[carIndex].name} for {playerName} at spawn point {spawnIndex}");
        }
        
        [Server]
        void AssignCarToPlayer(GameObject car, NetworkConnectionToClient conn)
        {
            // Add car control component if needed
            var carController = car.GetComponent<CarPlayer>();
            if (carController == null)
            {
                carController = car.AddComponent<CarPlayer>();
            }
            
            // Set ownership
            var networkIdentity = car.GetComponent<NetworkIdentity>();
            if (networkIdentity != null)
            {
                networkIdentity.AssignClientAuthority(conn);
            }
            else
            {
                Debug.LogError($"[GameSpawnManager] Car prefab {car.name} is missing NetworkIdentity component!");
            }
            
            Debug.Log($"[GameSpawnManager] Assigned car authority to connection {conn.connectionId}");
        }
        
        public override void OnStopServer()
        {
            base.OnStopServer();
            
            // Clean up spawned cars
            foreach (var car in spawnedCars)
            {
                if (car != null)
                {
                    NetworkServer.Destroy(car);
                }
            }
            spawnedCars.Clear();
        }
        
        // Called when scene loads to spawn cars for existing players
        [Server]
        public void OnSceneLoaded()
        {
            Debug.Log("[GameSpawnManager] MainGameScene loaded - spawning cars for players");
            
            // Wait a frame for all players to be ready
            Invoke(nameof(SpawnCarsFromSavedData), 0.1f);
        }
    }
}
