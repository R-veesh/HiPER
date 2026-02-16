using UnityEngine;
using Mirror;

namespace resource.LobbyScene
{
    public class LobbySetupValidator : MonoBehaviour
    {
        [Header("Validation Results")]
        public bool isValid = false;
        public string errorMessage = "";

        void Start()
        {
            ValidateSetup();
        }

        public void ValidateSetup()
        {
            isValid = true;
            errorMessage = "";

            // Check LobbyManager
            var lobbyManager = FindObjectOfType<LobbyManager>();
            if (lobbyManager == null)
            {
                AddError("❌ LobbyManager not found in scene!");
            }
            else
            {
                if (lobbyManager.spawnPoints == null || lobbyManager.spawnPoints.Length == 0)
                    AddError("❌ LobbyManager: No spawn points assigned!");
                else if (lobbyManager.spawnPoints.Length < 4)
                    AddWarning($"⚠️ LobbyManager: Only {lobbyManager.spawnPoints.Length} spawn points (recommend 4)");
                else
                    AddSuccess($"✅ LobbyManager: {lobbyManager.spawnPoints.Length} spawn points");

                if (lobbyManager.lobbyPlayerPrefab == null)
                    AddError("❌ LobbyManager: LobbyPlayer prefab not assigned!");
                else
                    AddSuccess("✅ LobbyManager: LobbyPlayer prefab assigned");

                if (lobbyManager.availableMaps == null || lobbyManager.availableMaps.Length == 0)
                    AddWarning("⚠️ LobbyManager: No maps assigned (create MapData asset)");
                else
                    AddSuccess($"✅ LobbyManager: {lobbyManager.availableMaps.Length} maps available");
            }

            // Check LobbyCountdown
            var countdown = FindObjectOfType<LobbyCountdown>();
            if (countdown == null)
                AddError("❌ LobbyCountdown not found in scene!");
            else
                AddSuccess("✅ LobbyCountdown found");

            // Check CustomNetworkManager
            var networkManager = NetworkManager.singleton as resource.MainMenuScene.CustomNetworkManager;
            if (networkManager == null)
            {
                AddError("❌ CustomNetworkManager not found or not singleton!");
            }
            else
            {
                if (networkManager.lobbyPlayerPrefab == null)
                    AddError("❌ CustomNetworkManager: LobbyPlayer prefab not assigned!");
                else
                    AddSuccess("✅ CustomNetworkManager: LobbyPlayer prefab assigned");
            }

            // Check LobbyUI
            var lobbyUI = FindObjectOfType<LobbyUI>();
            if (lobbyUI == null)
                AddWarning("⚠️ LobbyUI not found in scene");
            else
                AddSuccess("✅ LobbyUI found");

            // Check player plates
            if (lobbyUI != null)
            {
                if (lobbyUI.playerPlates == null || lobbyUI.playerPlates.Length == 0)
                    AddWarning("⚠️ LobbyUI: No player plates assigned");
                else
                    AddSuccess($"✅ LobbyUI: {lobbyUI.playerPlates.Length} player plates");
            }

            // Final result
            if (isValid && string.IsNullOrEmpty(errorMessage))
            {
                Debug.Log("<color=green>✅ LOBBY SETUP VALIDATION PASSED</color>");
            }
            else if (!isValid)
            {
                Debug.LogError("<color=red>❌ LOBBY SETUP VALIDATION FAILED</color>\n" + errorMessage);
            }
            else
            {
                Debug.LogWarning("<color=yellow>⚠️ LOBBY SETUP VALIDATION WARNINGS</color>\n" + errorMessage);
            }
        }

        void AddError(string message)
        {
            isValid = false;
            errorMessage += message + "\n";
        }

        void AddWarning(string message)
        {
            errorMessage += message + "\n";
        }

        void AddSuccess(string message)
        {
            errorMessage += message + "\n";
        }

        void OnGUI()
        {
            if (Application.isEditor && !string.IsNullOrEmpty(errorMessage))
            {
                GUILayout.BeginArea(new Rect(10, 10, 400, 300));
                GUILayout.BeginVertical("box");
                
                GUILayout.Label("Lobby Setup Validation", GUILayout.Height(30));
                GUILayout.Space(10);
                
                GUILayout.Label(errorMessage);
                
                if (GUILayout.Button("Re-Validate", GUILayout.Height(40)))
                {
                    ValidateSetup();
                }
                
                GUILayout.EndVertical();
                GUILayout.EndArea();
            }
        }
    }
}
