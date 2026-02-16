using UnityEngine;
using Mirror;
using resource.LobbyScene;
using resource.MainMenuScene;

namespace resource.LobbyScene
{
    public class LobbySetupHelper : MonoBehaviour
    {
        [Header("Auto-Setup")]
        public bool autoFindSpawnPoints = true;
        public bool autoAssignPrefabs = true;
        
        [Header("Debug")]
        public bool logSetupInfo = true;
        
        void Start()
        {
            SetupLobbyManager();
        }
        
        void SetupLobbyManager()
        {
            var lobbyManager = FindObjectOfType<LobbyManager>();
            if (lobbyManager == null)
            {
                Debug.LogError("LobbyManager not found!");
                return;
            }
            
            // Auto-find spawn points
            if (autoFindSpawnPoints)
            {
                var lobbySpawns = GameObject.Find("LobbySpawns");
                if (lobbySpawns != null)
                {
                    var spawnPointTransforms = lobbySpawns.GetComponentsInChildren<Transform>();
                    
                    // Filter out the parent transform
                    var spawnPoints = new System.Collections.Generic.List<Transform>();
                    foreach (var t in spawnPointTransforms)
                    {
                        if (t != lobbySpawns.transform)
                        {
                            spawnPoints.Add(t);
                        }
                    }
                    
                    lobbyManager.spawnPoints = spawnPoints.ToArray();
                    
                    if (logSetupInfo)
                    {
                        Debug.Log($"Auto-assigned {spawnPoints.Count} spawn points to LobbyManager");
                    }
                }
                else
                {
                    Debug.LogError("LobbySpawns object not found in scene!");
                }
            }
            
            // Auto-assign lobby player prefab
            if (autoAssignPrefabs)
            {
                var networkManager = FindObjectOfType<CustomNetworkManager>();
                if (networkManager != null && networkManager.lobbyPlayerPrefab != null)
                {
                    lobbyManager.lobbyPlayerPrefab = networkManager.lobbyPlayerPrefab;
                    
                    if (logSetupInfo)
                    {
                        Debug.Log("Auto-assigned lobby player prefab to LobbyManager");
                    }
                }
            }
            
            // Validate setup
            ValidateLobbySetup(lobbyManager);
        }
        
        void ValidateLobbySetup(LobbyManager lobbyManager)
        {
            bool isValid = true;
            
            if (lobbyManager.spawnPoints == null || lobbyManager.spawnPoints.Length == 0)
            {
                Debug.LogError("❌ No spawn points assigned!");
                isValid = false;
            }
            else
            {
                Debug.Log($"✅ Found {lobbyManager.spawnPoints.Length} spawn points");
            }
            
            if (lobbyManager.lobbyPlayerPrefab == null)
            {
                Debug.LogError("❌ No lobby player prefab assigned!");
                isValid = false;
            }
            else
            {
                Debug.Log("✅ Lobby player prefab assigned");
            }
            
            if (isValid)
            {
                Debug.Log("🎯 Lobby setup validation PASSED");
            }
            else
            {
                Debug.LogError("❌ Lobby setup validation FAILED");
            }
        }
    }
}
