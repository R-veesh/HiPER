using UnityEngine;
using Mirror;
using resource.LobbyScene;
using resource.script;

namespace resource.MainMenuScene
{
    public class CustomNetworkManager : NetworkManager
    {
        [Header("Scene Management")]
        [Scene] public string mainMenuScene = "MainMenuScene";
        [Scene] public string lobbyScene = "LobbyScene";
        [Scene] public string gameScene = "MainGameScene";
        private string selectedGameScene = "MainGameScene";

        [Header("Lobby Settings")]
        public GameObject lobbyPlayerPrefab;

        private LobbyManager lobbyManager;

        public override void OnStartHost()
        {
            Debug.Log("HOST button pressed – starting host and loading LobbyScene");
            base.OnStartHost();
            // Automatically transition to LobbyScene after starting host
            ServerChangeScene(lobbyScene);
        }

        public override void OnClientConnect()
        {
            Debug.Log("JOIN button pressed – client connected successfully");
            base.OnClientConnect();
            
            // Client should automatically go to LobbyScene when connected
            // The server will handle the scene change
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            Debug.Log($"CustomNetworkManager: Adding player for connection {conn.connectionId}");

            // Spawn LobbyPlayer prefab for this connection
            if (lobbyPlayerPrefab != null)
            {
                GameObject player = Instantiate(lobbyPlayerPrefab);
                NetworkServer.AddPlayerForConnection(conn, player);
                
                // Let LobbyManager handle the rest
                if (lobbyManager != null)
                {
                    lobbyManager.OnPlayerAdded(conn);
                }
            }
            else
            {
                Debug.LogError("LobbyPlayer prefab not assigned!");
                base.OnServerAddPlayer(conn);
            }
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            Debug.Log($"Client disconnected: {conn.connectionId}");
            
            if (lobbyManager != null)
            {
                lobbyManager.OnPlayerRemoved(conn);
            }

            base.OnServerDisconnect(conn);
        }

        public override void OnServerSceneChanged(string sceneName)
        {
            base.OnServerSceneChanged(sceneName);
            
            // Find lobby manager when in lobby scene
            if (sceneName == lobbyScene)
            {
                lobbyManager = FindObjectOfType<LobbyManager>();
                if (lobbyManager != null)
                {
                    Debug.Log("LobbyManager found and connected");
                }
                else
                {
                    Debug.LogWarning("LobbyManager not found in LobbyScene!");
                }
            }
            // Handle game scene loading
            else if (sceneName == selectedGameScene)
            {
                Debug.Log($"Game scene {sceneName} loaded - initializing game");
                var gameSpawnManager = FindObjectOfType<GameSpawnManager>();
                if (gameSpawnManager != null)
                {
                    gameSpawnManager.OnSceneLoaded();
                }
                else
                {
                    Debug.LogWarning($"GameSpawnManager not found in {sceneName}!");
                }
            }
        }

        public void SetGameScene(string sceneName)
        {
            selectedGameScene = sceneName;
            Debug.Log($"Selected game scene set to: {sceneName}");
        }

        public void LoadGameScene()
        {
            if (NetworkServer.active)
            {
                Debug.Log($"Loading Game Scene: {selectedGameScene}");
                ServerChangeScene(selectedGameScene);
            }
        }

        public void ReturnToMainMenu()
        {
            Debug.Log("Returning to MainMenuScene");
            
            // Stop all network activity
            if (NetworkServer.active)
            {
                NetworkManager.singleton.StopHost();
            }
            else if (NetworkClient.active)
            {
                NetworkManager.singleton.StopClient();
            }
            
            // Load main menu scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuScene);
        }
    }
}
