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
            
            // Find the connection for this player first
            NetworkConnectionToClient conn = null;
            NetworkServer.connections.TryGetValue(playerData.connectionId, out conn);
            
            // Spawn the car with authority if connection exists
            GameObject car = Instantiate(carPrefabs[carIndex], spawnPoint.position, spawnPoint.rotation);
            if (conn != null)
            {
                // Spawn with client authority
                NetworkServer.Spawn(car, conn);
                Debug.Log($"[GameSpawnManager] Spawned car {carPrefabs[carIndex].name} for player {playerData.playerName} at spawn point {spawnIndex} with authority");
            }
            else
            {
                // Spawn without authority
                NetworkServer.Spawn(car);
                Debug.LogWarning($"[GameSpawnManager] Connection {playerData.connectionId} not found for player {playerData.playerName}. Car spawned without ownership.");
            }
            
            // Add car control component
            AssignCarControl(car, conn);
            
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
            
            // Spawn the car with client authority
            GameObject car = Instantiate(carPrefabs[carIndex], spawnPoint.position, spawnPoint.rotation);
            NetworkServer.Spawn(car, conn);
            
            // Add car control component
            AssignCarControl(car, conn);
            
            spawnedCars.Add(car);
            
            Debug.Log($"[GameSpawnManager] Spawned car {carPrefabs[carIndex].name} for {playerName} at spawn point {spawnIndex} with authority");
        }
        
        [Server]
        void AssignCarControl(GameObject car, NetworkConnectionToClient conn)
        {
            // Add car control component if needed
            var carController = car.GetComponent<CarPlayer>();
            if (carController == null)
            {
                carController = car.AddComponent<CarPlayer>();
            }
            
            // Verify NetworkIdentity exists
            var networkIdentity = car.GetComponent<NetworkIdentity>();
            if (networkIdentity == null)
            {
                Debug.LogError($"[GameSpawnManager] Car prefab {car.name} is missing NetworkIdentity component!");
                return;
            }
            
            Debug.Log($"[GameSpawnManager] Car control assigned to connection {(conn != null ? conn.connectionId.ToString() : "null")}");
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
            Debug.Log("[GameSpawnManager] Game scene loaded - spawning cars for players");
            
            // Wait for clients to finish loading the scene
            StartCoroutine(WaitAndSpawnCars());
        }
        
        System.Collections.IEnumerator WaitAndSpawnCars()
        {
            // Wait for all connected clients to be ready
            float timeout = 10f;
            float elapsed = 0f;
            bool allClientsReady = false;
            
            while (!allClientsReady && elapsed < timeout)
            {
                allClientsReady = true;
                
                // Check if all connections are ready
                foreach (var conn in NetworkServer.connections.Values)
                {
                    if (conn != null && !conn.isReady)
                    {
                        allClientsReady = false;
                        break;
                    }
                }
                
                if (!allClientsReady)
                {
                    elapsed += 0.1f;
                    yield return new WaitForSeconds(0.1f);
                }
            }
            
            if (allClientsReady)
            {
                Debug.Log($"[GameSpawnManager] All clients ready after {elapsed:F1}s, spawning cars...");
                SpawnCarsFromSavedData();
            }
            else
            {
                Debug.LogWarning($"[GameSpawnManager] Timeout waiting for clients! Spawning cars anyway...");
                SpawnCarsFromSavedData();
            }
        }
    }
}
