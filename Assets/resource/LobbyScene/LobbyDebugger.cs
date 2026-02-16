using UnityEngine;
using Mirror;

namespace resource.LobbyScene
{
    /// <summary>
    /// Debug helper to diagnose lobby issues at runtime
    /// Attach this to any GameObject in LobbyScene
    /// </summary>
    public class LobbyDebugger : MonoBehaviour
    {
        [Header("Debug Settings")]
        public bool showDebugGUI = true;
        public bool verboseLogging = true;
        
        private string debugLog = "";
        private Vector2 scrollPosition;
        
        void Start()
        {
            Debug.Log("[LobbyDebugger] Starting diagnostics...");
            InvokeRepeating(nameof(RunDiagnostics), 1f, 2f);
        }
        
        void RunDiagnostics()
        {
            debugLog = "";
            
            // Check NetworkManager
            AddLog("=== NETWORK STATUS ===");
            AddLog($"NetworkServer.active: {NetworkServer.active}");
            AddLog($"NetworkClient.active: {NetworkClient.active}");
            AddLog($"NetworkClient.localPlayer: {(NetworkClient.localPlayer != null ? "EXISTS" : "NULL")}");
            
            // Check LobbyManager
            AddLog("\n=== LOBBY MANAGER ===");
            if (LobbyManager.Instance != null)
            {
                AddLog("✓ LobbyManager.Instance: FOUND");
                AddLog($"  Spawn Points: {(LobbyManager.Instance.spawnPoints != null ? LobbyManager.Instance.spawnPoints.Length : 0)}");
                AddLog($"  Player Prefab: {(LobbyManager.Instance.lobbyPlayerPrefab != null ? "ASSIGNED" : "NULL")}");
                AddLog($"  Connected Players: {LobbyManager.Instance.connectedPlayerCount}");
                AddLog($"  Available Maps: {(LobbyManager.Instance.availableMaps != null ? LobbyManager.Instance.availableMaps.Length : 0)}");
            }
            else
            {
                AddLog("✗ LobbyManager.Instance: NULL");
            }
            
            // Check LobbyPlayer
            AddLog("\n=== LOCAL PLAYER ===");
            if (NetworkClient.localPlayer != null)
            {
                var lobbyPlayer = NetworkClient.localPlayer.GetComponent<LobbyPlayer>();
                if (lobbyPlayer != null)
                {
                    AddLog("✓ LobbyPlayer: FOUND");
                    AddLog($"  isLocalPlayer: {lobbyPlayer.isLocalPlayer}");
                    AddLog($"  isServer: {lobbyPlayer.isServer}");
                    AddLog($"  isClient: {lobbyPlayer.isClient}");
                    AddLog($"  Player Name: {lobbyPlayer.playerName}");
                    AddLog($"  Selected Car: {lobbyPlayer.selectedCarIndex}");
                    AddLog($"  Is Ready: {lobbyPlayer.isReady}");
                    AddLog($"  Car Prefabs: {(lobbyPlayer.carPrefabs != null ? lobbyPlayer.carPrefabs.Length : 0)}");
                    
                    if (lobbyPlayer.carPrefabs == null || lobbyPlayer.carPrefabs.Length == 0)
                    {
                        AddLog("  ⚠️ CRITICAL: carPrefabs array is empty!");
                    }
                }
                else
                {
                    AddLog("✗ LobbyPlayer component: MISSING");
                }
            }
            else
            {
                AddLog("✗ NetworkClient.localPlayer: NULL");
            }
            
            // Check LobbyUI
            AddLog("\n=== LOBBY UI ===");
            var lobbyUI = FindObjectOfType<LobbyUI>();
            if (lobbyUI != null)
            {
                AddLog("✓ LobbyUI: FOUND");
                AddLog($"  Ready Button: {(lobbyUI.readyButton != null ? "ASSIGNED" : "NULL")}");
                AddLog($"  Next Car Button: {(lobbyUI.nextCarButton != null ? "ASSIGNED" : "NULL")}");
                AddLog($"  Prev Car Button: {(lobbyUI.prevCarButton != null ? "ASSIGNED" : "NULL")}");
                AddLog($"  Start Button: {(lobbyUI.startButton != null ? "ASSIGNED" : "NULL")}");
                AddLog($"  Player Plates: {(lobbyUI.playerPlates != null ? lobbyUI.playerPlates.Length : 0)}");
            }
            else
            {
                AddLog("✗ LobbyUI: NOT FOUND");
            }
            
            // Check EventSystem
            AddLog("\n=== EVENT SYSTEM ===");
            var eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem != null)
            {
                AddLog("✓ EventSystem: FOUND");
            }
            else
            {
                AddLog("✗ EventSystem: MISSING (buttons won't work!)");
            }
            
            // Check all LobbyPlayers in scene
            AddLog("\n=== ALL LOBBY PLAYERS ===");
            var allPlayers = FindObjectsOfType<LobbyPlayer>();
            AddLog($"Total LobbyPlayers in scene: {allPlayers.Length}");
            foreach (var player in allPlayers)
            {
                AddLog($"  - {player.playerName} (Local: {player.isLocalPlayer}, Car: {player.selectedCarIndex})");
            }
            
            // Check for errors
            AddLog("\n=== CRITICAL CHECKS ===");
            if (LobbyManager.Instance?.spawnPoints == null || LobbyManager.Instance.spawnPoints.Length == 0)
            {
                AddLog("✗ ERROR: No spawn points assigned in LobbyManager!");
            }
            
            if (NetworkClient.localPlayer?.GetComponent<LobbyPlayer>()?.carPrefabs == null)
            {
                AddLog("✗ ERROR: LobbyPlayer carPrefabs not assigned!");
            }
        }
        
        void AddLog(string message)
        {
            debugLog += message + "\n";
            if (verboseLogging)
            {
                Debug.Log($"[LobbyDebugger] {message}");
            }
        }
        
        void OnGUI()
        {
            if (!showDebugGUI) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 500, 600));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("<b><size=16>LOBBY DEBUGGER</size></b>");
            GUILayout.Space(10);
            
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(500));
            GUILayout.Label(debugLog, GUILayout.ExpandHeight(true));
            GUILayout.EndScrollView();
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Run Diagnostics Now", GUILayout.Height(40)))
            {
                RunDiagnostics();
            }
            
            if (GUILayout.Button("Force Spawn Car Preview", GUILayout.Height(40)))
            {
                ForceSpawnCarPreview();
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        void ForceSpawnCarPreview()
        {
            if (NetworkClient.localPlayer != null)
            {
                var lobbyPlayer = NetworkClient.localPlayer.GetComponent<LobbyPlayer>();
                if (lobbyPlayer != null)
                {
                    Debug.Log("[LobbyDebugger] Forcing car preview spawn...");
                    // Call the spawn method via reflection or make it public
                    // For now, just change car index which triggers spawn
                    lobbyPlayer.CmdNextCar();
                }
            }
        }
    }
}
