using UnityEngine;
using Mirror;
using UnityEngine;

public class CustomNetworkManager : NetworkManager
{
    [Header("Scene Management")]
    [Scene] public string mainMenuScene = "MainMenuScene";
    [Scene] public string lobbyScene = "LobbyScene";
    [Scene] public string gameScene = "GameScene";

    [Header("Lobby Settings")]
    public GameObject lobbyPlayerPrefab;

    private LobbyManager lobbyManager;

    public override void OnStartHost()
    {
        Debug.Log("Host started – going to LobbyScene");
        ServerChangeScene(lobbyScene);
    }

    public override void OnClientConnect()
    {
        Debug.Log("Client connected – staying in current scene");
        base.OnClientConnect();
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        Debug.Log($"CustomNetworkManager: Adding player for connection {conn.connectionId}");

        if (lobbyManager != null)
        {
            lobbyManager.OnPlayerAdded(conn);
        }
        else
        {
            // Fallback to default behavior if no lobby manager
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
    }

    public void LoadGameScene()
    {
        if (NetworkServer.active)
        {
            Debug.Log("Loading Game Scene");
            ServerChangeScene(gameScene);
        }
    }

    public void ReturnToMainMenu()
    {
        if (NetworkServer.active)
        {
            ServerChangeScene(mainMenuScene);
        }
        else
        {
            NetworkManager.singleton.StopClient();
            UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuScene);
        }
    }
}