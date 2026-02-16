using UnityEngine;
using Mirror;
using System.Collections.Generic;
using resource.LobbyScene;

namespace resource.script
{
    public class GameSpawnManager : NetworkBehaviour
    {
        public static GameSpawnManager Instance;
        
        [Header("Car Prefabs")]
        public GameObject[] carPrefabs;
        
        [Header("Spawn Points")]
        public Transform[] spawnPoints;
        
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
            Debug.Log("GameSpawnManager started on server");
            
            // Spawn cars for all connected players
            SpawnCarsForAllPlayers();
        }
        
        void SpawnCarsForAllPlayers()
        {
            var players = FindObjectsOfType<LobbyPlayer>();
            Debug.Log($"Spawning cars for {players.Length} players");
            
            foreach (var lobbyPlayer in players)
            {
                if (lobbyPlayer != null)
                {
                    SpawnCarForPlayer(lobbyPlayer);
                }
            }
        }
        
        [Server]
        void SpawnCarForPlayer(LobbyPlayer lobbyPlayer)
        {
            if (carPrefabs == null || carPrefabs.Length == 0)
            {
                Debug.LogError("No car prefabs assigned!");
                return;
            }
            
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogError("No spawn points assigned!");
                return;
            }
            
            // Get selected car index (ensure it's valid)
            int carIndex = Mathf.Clamp(lobbyPlayer.selectedCarIndex, 0, carPrefabs.Length - 1);
            
            // Get spawn position
            int spawnIndex = nextSpawnIndex % spawnPoints.Length;
            Transform spawnPoint = spawnPoints[spawnIndex];
            nextSpawnIndex++;
            
            // Spawn the car
            GameObject car = Instantiate(carPrefabs[carIndex], spawnPoint.position, spawnPoint.rotation);
            NetworkServer.Spawn(car);
            
            // Assign car to player
            AssignCarToPlayer(car, lobbyPlayer.connectionToClient);
            
            spawnedCars.Add(car);
            
            Debug.Log($"Spawned car {carPrefabs[carIndex].name} for player {lobbyPlayer.playerName} at spawn point {spawnIndex}");
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
                Debug.LogError($"Car prefab {car.name} is missing NetworkIdentity component!");
            }
            
            Debug.Log($"Assigned car authority to connection {conn.connectionId}");
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
            Debug.Log("MainGameScene loaded - spawning cars for players");
            
            // Wait a frame for all players to be ready
            Invoke(nameof(SpawnCarsForAllPlayers), 0.1f);
        }
    }
}
