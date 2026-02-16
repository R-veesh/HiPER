using UnityEngine;
using Mirror;
using resource.LobbyScene;
using resource.MainMenuScene;

namespace resource.LobbyScene
{
    public class LobbyTester : MonoBehaviour
    {
        [Header("Test Controls")]
        public bool simulateMultiplePlayers;
        public bool autoTestReadyStates;
        
        [Header("Test Results")]
        [SerializeField] private int connectedPlayers;
        [SerializeField] private int readyPlayers;
        
        private LobbyManager lobbyManager;
        private float testTimer;
        
        void Start()
        {
            lobbyManager = LobbyManager.Instance;
            if (lobbyManager == null)
            {
                Debug.LogError("LobbyManager not found!");
                return;
            }
            
            Debug.Log("🧪 Lobby Tester initialized");
            StartAutomatedTests();
        }
        
        void Update()
        {
            UpdateTestStatus();
            
            if (simulateMultiplePlayers && Input.GetKeyDown(KeyCode.T))
            {
                SimulatePlayerJoin();
            }
            
            if (autoTestReadyStates && Input.GetKeyDown(KeyCode.R))
            {
                TestReadyStates();
            }
        }
        
        void StartAutomatedTests()
        {
            Debug.Log("🔬 Starting automated lobby tests...");
            
            // Test 1: Validate spawn points
            ValidateSpawnPoints();
            
            // Test 2: Check network setup
            ValidateNetworkSetup();
            
            // Test 3: Test UI components
            ValidateUIComponents();
        }
        
        void ValidateSpawnPoints()
        {
            if (lobbyManager.spawnPoints == null || lobbyManager.spawnPoints.Length == 0)
            {
                Debug.LogError("❌ TEST FAILED: No spawn points found");
                return;
            }
            
            Debug.Log($"✅ TEST PASSED: Found {lobbyManager.spawnPoints.Length} spawn points");
            
            // Check if spawn points have valid positions
            for (int i = 0; i < lobbyManager.spawnPoints.Length; i++)
            {
                var point = lobbyManager.spawnPoints[i];
                if (point == null)
                {
                    Debug.LogError($"❌ TEST FAILED: Spawn point {i} is null");
                    continue;
                }
                
                Debug.Log($"✅ Spawn point {i}: {point.position}");
            }
        }
        
        void ValidateNetworkSetup()
        {
            var networkManager = FindObjectOfType<CustomNetworkManager>();
            if (networkManager == null)
            {
                Debug.LogError("❌ TEST FAILED: CustomNetworkManager not found");
                return;
            }
            
            if (networkManager.lobbyPlayerPrefab == null)
            {
                Debug.LogError("❌ TEST FAILED: Lobby player prefab not assigned");
                return;
            }
            
            Debug.Log("✅ TEST PASSED: Network setup validated");
        }
        
        void ValidateUIComponents()
        {
            var lobbyUI = FindObjectOfType<LobbyUI>();
            if (lobbyUI == null)
            {
                Debug.LogError("❌ TEST FAILED: LobbyUI not found");
                return;
            }
            
            // Check essential UI components
            if (lobbyUI.readyButton == null)
                Debug.LogError("❌ TEST FAILED: Ready button not assigned");
            else
                Debug.Log("✅ Ready button found");
                
            if (lobbyUI.startButton == null)
                Debug.LogError("❌ TEST FAILED: Start button not assigned");
            else
                Debug.Log("✅ Start button found");
                
            if (lobbyUI.statusText == null)
                Debug.LogError("❌ TEST FAILED: Status text not assigned");
            else
                Debug.Log("✅ Status text found");
        }
        
        void SimulatePlayerJoin()
        {
            Debug.Log("🎮 Simulating player join...");
            
            // This would normally be handled by Mirror when a client connects
            // For testing, we can manually trigger the logic
            var availableSpawn = GetAvailableSpawnPoint();
            if (availableSpawn == -1)
            {
                Debug.LogError("No available spawn points for new player");
                return;
            }
            
            Debug.Log($"✅ Simulated player would join at spawn point {availableSpawn}");
        }
        
        int GetAvailableSpawnPoint()
        {
            // Use reflection to access private field for testing
            var field = typeof(LobbyManager).GetField("usedSpawnPoints", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null && lobbyManager != null)
            {
                bool[] usedSpawnPoints = field.GetValue(lobbyManager) as bool[];
                if (usedSpawnPoints != null)
                {
                    for (int i = 0; i < usedSpawnPoints.Length; i++)
                    {
                        if (!usedSpawnPoints[i])
                            return i;
                    }
                }
            }
            return -1;
        }
        
        void TestReadyStates()
        {
            Debug.Log("🔄 Testing ready state system...");
            
            // Check if all players are ready
            bool allReady = lobbyManager.AllPlayersReady();
            Debug.Log($"All players ready: {allReady}");
            
            // Test start button visibility
            var lobbyUI = FindObjectOfType<LobbyUI>();
            if (lobbyUI != null)
            {
                bool shouldShowStart = allReady; // Simplified for testing
                Debug.Log($"Start button should be visible: {shouldShowStart}");
            }
        }
        
        void UpdateTestStatus()
        {
            // Count connected players
            var players = FindObjectsOfType<LobbyPlayer>();
            connectedPlayers = players.Length;
            
            // Count ready players
            readyPlayers = 0;
            foreach (var player in players)
            {
                if (player.isReady)
                    readyPlayers++;
            }
            
            // Display test info
            testTimer += Time.deltaTime;
            if (testTimer > 2f) // Update every 2 seconds
            {
                Debug.Log($"📊 Status: {connectedPlayers} players, {readyPlayers} ready");
                testTimer = 0f;
            }
        }
        
        [ContextMenu("Run Full Test Suite")]
        public void RunFullTestSuite()
        {
            Debug.Log("🚀 Running full lobby test suite...");
            StartAutomatedTests();
            SimulatePlayerJoin();
            TestReadyStates();
            Debug.Log("✅ Test suite completed");
        }
    }
}
