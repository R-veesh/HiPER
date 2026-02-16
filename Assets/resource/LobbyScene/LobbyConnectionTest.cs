using UnityEngine;
using Mirror;

namespace resource.LobbyScene
{
    /// <summary>
    /// Simple test to verify the lobby player connection is working
    /// Attach this to any GameObject in LobbyScene
    /// </summary>
    public class LobbyConnectionTest : MonoBehaviour
    {
        [Header("Test Results")]
        public bool playerFound = false;
        public string playerName = "";
        public int carIndex = -1;
        public bool isReady = false;
        public string statusMessage = "Testing...";
        
        void Start()
        {
            Debug.Log("[LobbyConnectionTest] Starting connection test...");
            InvokeRepeating(nameof(TestConnection), 0.5f, 1f);
        }
        
        void TestConnection()
        {
            // Test 1: Check if NetworkClient is active
            if (!NetworkClient.active)
            {
                statusMessage = "NetworkClient not active!";
                Debug.LogError("[LobbyConnectionTest] NetworkClient is not active!");
                return;
            }
            
            // Test 2: Check local player
            if (NetworkClient.localPlayer == null)
            {
                statusMessage = "Local player is NULL!";
                Debug.LogError("[LobbyConnectionTest] NetworkClient.localPlayer is NULL!");
                return;
            }
            
            // Test 3: Check LobbyPlayer component
            var lobbyPlayer = NetworkClient.localPlayer.GetComponent<LobbyPlayer>();
            if (lobbyPlayer == null)
            {
                statusMessage = "LobbyPlayer component missing!";
                Debug.LogError("[LobbyConnectionTest] LobbyPlayer component is missing from local player!");
                    Debug.Log($"[LobbyConnectionTest] Local player name: {NetworkClient.localPlayer.name}");
                    var components = NetworkClient.localPlayer.GetComponents<Component>();
                    string componentNames = "";
                    foreach (var comp in components)
                    {
                        componentNames += comp.GetType().Name + ", ";
                    }
                    Debug.Log($"[LobbyConnectionTest] Local player components: {componentNames}");
                return;
            }
            
            // Success!
            playerFound = true;
            playerName = lobbyPlayer.playerName;
            carIndex = lobbyPlayer.selectedCarIndex;
            isReady = lobbyPlayer.isReady;
            statusMessage = $"Player connected: {playerName}";
            
            Debug.Log($"[LobbyConnectionTest] ✓ SUCCESS! Player: {playerName}, Car: {carIndex}, Ready: {isReady}");
            
            // Cancel after success
            CancelInvoke(nameof(TestConnection));
        }
        
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 310, 10, 300, 150));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("<b><size=14>CONNECTION TEST</size></b>");
            GUILayout.Space(5);
            
            GUI.color = playerFound ? Color.green : Color.red;
            GUILayout.Label($"Status: {statusMessage}");
            GUI.color = Color.white;
            
            if (playerFound)
            {
                GUILayout.Label($"Player: {playerName}");
                GUILayout.Label($"Car Index: {carIndex}");
                GUILayout.Label($"Ready: {isReady}");
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
