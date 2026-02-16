using UnityEngine;
using Mirror;

namespace resource.LobbyScene
{
    /// <summary>
    /// Auto-fixes common lobby setup issues
    /// Attach to LobbyManager GameObject
    /// </summary>
    public class LobbyAutoFix : MonoBehaviour
    {
        [Header("Auto Fix Settings")]
        public bool autoFixOnStart = true;
        public bool showFixesInConsole = true;
        
        void Start()
        {
            if (autoFixOnStart)
            {
                RunAutoFix();
            }
        }
        
        [ContextMenu("Run Auto Fix")]
        public void RunAutoFix()
        {
            Debug.Log("[LobbyAutoFix] Running auto-fix...");
            
            int fixesApplied = 0;
            
            // Fix 1: Check and create EventSystem
            fixesApplied += FixEventSystem();
            
            // Fix 2: Check LobbyManager references
            fixesApplied += FixLobbyManager();
            
            // Fix 3: Check LobbyUI references
            fixesApplied += FixLobbyUI();
            
            // Fix 4: Check NetworkManager
            fixesApplied += FixNetworkManager();
            
            Debug.Log($"[LobbyAutoFix] Applied {fixesApplied} fixes!");
        }
        
        int FixEventSystem()
        {
            var eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem == null)
            {
                var go = new GameObject("EventSystem");
                go.AddComponent<UnityEngine.EventSystems.EventSystem>();
                go.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Debug.Log("[LobbyAutoFix] ✓ Created EventSystem (buttons will now work!)");
                return 1;
            }
            return 0;
        }
        
        int FixLobbyManager()
        {
            int fixes = 0;
            var lobbyManager = GetComponent<LobbyManager>();
            
            if (lobbyManager == null)
            {
                Debug.LogError("[LobbyAutoFix] ✗ LobbyManager component not found on this GameObject!");
                return 0;
            }
            
            // Check spawn points
            if (lobbyManager.spawnPoints == null || lobbyManager.spawnPoints.Length == 0)
            {
                Debug.LogError("[LobbyAutoFix] ✗ LobbyManager.spawnPoints is empty! Please assign spawn points in Inspector.");
            }
            
            // Check player prefab
            if (lobbyManager.lobbyPlayerPrefab == null)
            {
                Debug.LogError("[LobbyAutoFix] ✗ LobbyManager.lobbyPlayerPrefab is not assigned!");
            }
            else
            {
                // Check if prefab has car prefabs
                var lobbyPlayer = lobbyManager.lobbyPlayerPrefab.GetComponent<LobbyPlayer>();
                if (lobbyPlayer != null)
                {
                    if (lobbyPlayer.carPrefabs == null || lobbyPlayer.carPrefabs.Length == 0)
                    {
                        Debug.LogError("[LobbyAutoFix] ✗ LobbyPlayer prefab has NO CAR PREFABS assigned! This is why cars won't spawn.");
                    }
                    else
                    {
                        if (showFixesInConsole)
                        {
                            Debug.Log($"[LobbyAutoFix] ✓ LobbyPlayer has {lobbyPlayer.carPrefabs.Length} car prefabs");
                        }
                    }
                }
            }
            
            return fixes;
        }
        
        int FixLobbyUI()
        {
            int fixes = 0;
            var lobbyUI = FindObjectOfType<LobbyUI>();
            
            if (lobbyUI == null)
            {
                Debug.LogError("[LobbyAutoFix] ✗ LobbyUI not found in scene!");
                return 0;
            }
            
            // Check button references
            if (lobbyUI.readyButton == null)
                Debug.LogError("[LobbyAutoFix] ✗ LobbyUI.readyButton is not assigned!");
            
            if (lobbyUI.nextCarButton == null)
                Debug.LogError("[LobbyAutoFix] ✗ LobbyUI.nextCarButton is not assigned!");
            
            if (lobbyUI.prevCarButton == null)
                Debug.LogError("[LobbyAutoFix] ✗ LobbyUI.prevCarButton is not assigned!");
                
            if (lobbyUI.playerPlates == null || lobbyUI.playerPlates.Length == 0)
                Debug.LogError("[LobbyAutoFix] ✗ LobbyUI.playerPlates is not assigned!");
            
            return fixes;
        }
        
        int FixNetworkManager()
        {
            int fixes = 0;
            var networkManager = NetworkManager.singleton as resource.MainMenuScene.CustomNetworkManager;
            
            if (networkManager == null)
            {
                Debug.LogError("[LobbyAutoFix] ✗ CustomNetworkManager not found as singleton!");
                return 0;
            }
            
            if (networkManager.lobbyPlayerPrefab == null)
            {
                Debug.LogError("[LobbyAutoFix] ✗ CustomNetworkManager.lobbyPlayerPrefab is not assigned!");
            }
            else
            {
                if (showFixesInConsole)
                {
                    Debug.Log("[LobbyAutoFix] ✓ NetworkManager has lobby player prefab assigned");
                }
            }
            
            return fixes;
        }
        
        void OnGUI()
        {
            if (GUI.Button(new Rect(10, Screen.height - 60, 200, 50), "Run Auto Fix"))
            {
                RunAutoFix();
            }
        }
    }
}
