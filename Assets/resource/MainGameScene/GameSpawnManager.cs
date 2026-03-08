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
            Debug.Log("[GameSpawnManager] Server started - waiting for scene load to spawn cars");
            // DON'T spawn here - wait for OnSceneLoaded when all clients are ready
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
            
            int carIndex = Mathf.Clamp(playerData.selectedCarIndex, 0, carPrefabs.Length - 1);
            Transform spawnPoint = GetNextSpawnPoint();
            
            NetworkConnectionToClient conn = null;
            if (NetworkServer.connections.TryGetValue(playerData.connectionId, out var foundConn))
            {
                conn = foundConn;
            }
            
            // CRITICAL: Validate prefab BEFORE instantiation
            GameObject prefab = carPrefabs[carIndex];
            if (!ValidateCarPrefab(prefab, playerData.playerName))
            {
                Debug.LogError($"[GameSpawnManager] CANNOT spawn car for {playerData.playerName} - prefab validation failed! Fix carPrefabs[{carIndex}] in Inspector.");
                return;
            }
            
            GameObject car = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            
            // Get required components (already validated above)
            CarPlayer carPlayer = car.GetComponent<CarPlayer>();
            NetworkIdentity netId = car.GetComponent<NetworkIdentity>();
            
            // Name the car for easier debugging
            car.name = $"Car_{playerData.playerName}_{playerData.connectionId}";
            
            if (conn != null)
            {
                NetworkServer.Spawn(car, conn);
                Debug.Log($"[GameSpawnManager] Spawned {carPrefabs[carIndex].name} for {playerData.playerName} (ID: {playerData.connectionId}) with AUTHORITY");
            }
            else
            {
                // DO NOT spawn - no owner means no one can control it
                Destroy(car);
                Debug.LogWarning($"[GameSpawnManager] SKIPPED spawn for {playerData.playerName} — connection {playerData.connectionId} not found (disconnected). Car destroyed.");
                return;
            }
            
            spawnedCars.Add(car);

            // Register with race system
            if (RaceManager.Instance != null)
                RaceManager.Instance.RegisterPlayer(car.GetComponent<NetworkIdentity>(), playerData.playerName);
            
            // Start the game for this car after a frame to let network initialize
            StartCoroutine(StartGameAfterSpawn(carPlayer));
        }
        
        [Server]
        void SpawnCarForConnection(NetworkConnectionToClient conn)
        {
            if (!ValidateSpawnPrerequisites()) return;
            
            var playerDataContainer = PlayerDataContainer.Instance ?? FindObjectOfType<PlayerDataContainer>();
            int carIndex = 0;
            string playerName = $"Player {conn.connectionId}";
            
            if (playerDataContainer != null)
            {
                var data = playerDataContainer.GetPlayerData(conn.connectionId);
                if (data.HasValue)
                {
                    carIndex = data.Value.selectedCarIndex;
                    playerName = data.Value.playerName;
                    Debug.Log($"[GameSpawnManager] Found saved data for {playerName}: Car {carIndex}");
                }
                else
                {
                    Debug.LogWarning($"[GameSpawnManager] No saved data for connection {conn.connectionId}, using defaults");
                }
            }
            
            carIndex = Mathf.Clamp(carIndex, 0, carPrefabs.Length - 1);
            Transform spawnPoint = GetNextSpawnPoint();
            
            // CRITICAL: Validate prefab BEFORE instantiation
            if (!ValidateCarPrefab(carPrefabs[carIndex], playerName))
            {
                Debug.LogError($"[GameSpawnManager] CANNOT spawn car for {playerName} - prefab validation failed! Fix carPrefabs[{carIndex}] in Inspector.");
                return;
            }
            
            GameObject car = Instantiate(carPrefabs[carIndex], spawnPoint.position, spawnPoint.rotation);
            
            // Get required components (already validated above)
            CarPlayer carPlayer = car.GetComponent<CarPlayer>();
            
            car.name = $"Car_{playerName}_{conn.connectionId}";
            NetworkServer.Spawn(car, conn);
            
            spawnedCars.Add(car);
            Debug.Log($"[GameSpawnManager] Spawned car for {playerName}");

            // Register with race system
            if (RaceManager.Instance != null)
                RaceManager.Instance.RegisterPlayer(car.GetComponent<NetworkIdentity>(), playerName);
            
            // Start the game for this car after a frame to let network initialize
            StartCoroutine(StartGameAfterSpawn(carPlayer));
        }
        
        [Server]
        bool ValidateSpawnPrerequisites()
        {
            if (carPrefabs == null || carPrefabs.Length == 0)
            {
                Debug.LogError("[GameSpawnManager] No car prefabs assigned!");
                return false;
            }
            
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogError("[GameSpawnManager] No spawn points assigned!");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Validates that a car prefab has all required components.
        /// NetworkBehaviour components CANNOT be added at runtime!
        /// </summary>
        [Server]
        bool ValidateCarPrefab(GameObject prefab, string playerName)
        {
            if (prefab == null)
            {
                Debug.LogError($"[GameSpawnManager] Car prefab is NULL for {playerName}!");
                return false;
            }
            
            NetworkIdentity netId = prefab.GetComponent<NetworkIdentity>();
            if (netId == null)
            {
                Debug.LogError($"[GameSpawnManager] Car prefab '{prefab.name}' is missing NetworkIdentity component! Add it in the prefab, not at runtime.");
                return false;
            }
            
            CarPlayer carPlayer = prefab.GetComponent<CarPlayer>();
            if (carPlayer == null)
            {
                Debug.LogError($"[GameSpawnManager] Car prefab '{prefab.name}' is missing CarPlayer component! Add it in the prefab, not at runtime.");
                return false;
            }
            
            CarController carController = prefab.GetComponent<CarController>();
            if (carController == null)
            {
                Debug.LogWarning($"[GameSpawnManager] Car prefab '{prefab.name}' is missing CarController component. Player won't be able to drive!");
                // Don't return false - car will spawn but won't drive
            }
            
            Debug.Log($"[GameSpawnManager] ✓ Prefab '{prefab.name}' validated for {playerName}");
            return true;
        }
        
        Transform GetNextSpawnPoint()
        {
            int index = nextSpawnIndex % spawnPoints.Length;
            nextSpawnIndex++;
            return spawnPoints[index];
        }
        
        System.Collections.IEnumerator StartGameAfterSpawn(CarPlayer carPlayer)
        {
            // Wait one frame for Mirror to initialize the network identity
            yield return null;
            
            if (carPlayer == null)
            {
                Debug.LogError("[GameSpawnManager] CarPlayer is null in StartGameAfterSpawn!");
                yield break;
            }
            
            // Verify NetworkIdentity is spawned and initialized
            var netId = carPlayer.GetComponent<NetworkIdentity>();
            if (netId == null || !netId.isServer)
            {
                Debug.LogError("[GameSpawnManager] NetworkIdentity not properly initialized on car!");
                yield break;
            }
            
            // Now safe to call server methods
            carPlayer.ServerStartGame();
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
            Debug.Log("[GameSpawnManager] Game scene loaded - starting spawn sequence");
            
            // Clear any previously spawned cars
            foreach (var car in spawnedCars)
            {
                if (car != null)
                    NetworkServer.Destroy(car);
            }
            spawnedCars.Clear();
            nextSpawnIndex = 0;
            
            // Wait for clients to finish loading the scene
            StartCoroutine(WaitAndSpawnCars());
        }
        
        System.Collections.IEnumerator WaitAndSpawnCars()
        {
            // Initial wait for scene to stabilize
            yield return new WaitForSeconds(0.5f);
            
            // Wait for all connected clients to be ready
            float timeout = 15f;
            float elapsed = 0f;
            int lastReadyCount = 0;
            
            while (elapsed < timeout)
            {
                int readyCount = 0;
                int totalCount = NetworkServer.connections.Count;
                
                foreach (var conn in NetworkServer.connections.Values)
                {
                    if (conn != null && conn.isReady)
                        readyCount++;
                }
                
                // Log when count changes
                if (readyCount != lastReadyCount)
                {
                    Debug.Log($"[GameSpawnManager] Clients ready: {readyCount}/{totalCount}");
                    lastReadyCount = readyCount;
                }
                
                // Spawn when all connected clients are ready AND we have at least 1 client
                if (readyCount >= totalCount && totalCount > 0)
                {
                    Debug.Log($"[GameSpawnManager] All {totalCount} clients ready! Spawning cars...");
                    SpawnCarsFromSavedData();
                    yield break;
                }
                
                elapsed += 0.2f;
                yield return new WaitForSeconds(0.2f);
            }
            
            Debug.LogWarning($"[GameSpawnManager] Timeout waiting for clients! Spawning cars anyway...");
            SpawnCarsFromSavedData();
        }
    }
}
