using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class CustomNetworkManager : NetworkManager
{
    public static CustomNetworkManager Singleton => (CustomNetworkManager)singleton;

    public GameObject mainMenuPanel;  // Drag MainMenuPanel in Inspector
    public GameObject lobbyPanel;     // Drag LobbyPanel in Inspector
    public GameObject playerSetupPanel; // Drag PlayerSetupPanel in Inspector
    public InputField ipInputField;   // Drag IPInputField in Inspector
    public Button addPlayerButton;    // Add Player button for local multiplayer
    public Button startLocalGameButton; // Start Local Game button

    // Lobby UI reference
    public LobbyUI lobbyUI;
    
    // Local multiplayer state
    private int localPlayerCount = 0;
    private bool isLocalMultiplayer = false;

    public override void OnStartHost()
    {
        // Register spawnable prefabs programmatically if needed
        RegisterSpawnablePrefabs();
        
        base.OnStartHost();
        SwitchToLobby();
        Debug.Log("CustomNetworkManager: OnStartHost - Switching to Lobby");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        SwitchToLobby();
        Debug.Log("CustomNetworkManager: OnStartClient - Switching to Lobby");
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        SwitchToMainMenu();
        Debug.Log("CustomNetworkManager: OnStopClient - Switching to MainMenu");
    }

    public override void OnStopHost()
    {
        base.OnStopHost();
        SwitchToMainMenu();
        Debug.Log("CustomNetworkManager: OnStopHost - Switching to MainMenu");
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        Debug.Log($"CustomNetworkManager: Player {conn.connectionId} connected");
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
        Debug.Log($"CustomNetworkManager: Player {conn.connectionId} disconnected");
        
        if (lobbyUI != null)
        {
            lobbyUI.Refresh();
        }
    }

    public void OnClickHost()
    {
        StartHost();
    }

    public void OnClickJoin()
    {
        if (string.IsNullOrEmpty(ipInputField.text))
        {
            Debug.LogWarning("IP is empty! Using localhost.");
            networkAddress = "localhost";
        }
        else
        {
            networkAddress = ipInputField.text;
        }
        StartClient();
    }

    public void StartLocalMultiplayer()
    {
        isLocalMultiplayer = true;
        localPlayerCount = 0;
        SwitchToPlayerSetup();
        Debug.Log("CustomNetworkManager: Starting local multiplayer setup");
    }

    public void AddLocalPlayer()
    {
        if (localPlayerCount >= 4)
        {
            Debug.LogWarning("Maximum 4 local players supported");
            return;
        }

        localPlayerCount++;
        CreateLocalLobbyPlayer(localPlayerCount);
        UpdateLocalPlayerSetup();
        
        if (startLocalGameButton != null)
            startLocalGameButton.gameObject.SetActive(localPlayerCount >= 2);
            
        Debug.Log($"CustomNetworkManager: Added local player {localPlayerCount}");
    }

    public void StartLocalGame()
    {
        if (localPlayerCount < 2)
        {
            Debug.LogWarning("Need at least 2 players to start local game");
            return;
        }

        // Start host for local game
        StartHost();
        Debug.Log("CustomNetworkManager: Starting local multiplayer game");
    }

    private void CreateLocalLobbyPlayer(int playerId)
    {
        // Create local player instance
        GameObject playerObj = Instantiate(playerPrefab);
        LobbyPlayer lobbyPlayer = playerObj.GetComponent<LobbyPlayer>();
        
        if (lobbyPlayer != null)
        {
            lobbyPlayer.playerName = $"Player {playerId}";
            
            // Register with LobbyUI
            if (lobbyUI != null)
            {
                lobbyUI.RegisterPlayer(lobbyPlayer);
            }
            
            // Don't spawn on network - just local instance
            Debug.Log($"Created local lobby player: {lobbyPlayer.playerName}");
        }
        else
        {
            Destroy(playerObj);
            Debug.LogError("Player prefab missing LobbyPlayer component");
        }
    }

    private void UpdateLocalPlayerSetup()
    {
        if (addPlayerButton != null)
            addPlayerButton.gameObject.SetActive(localPlayerCount < 4);
    }

    private void SwitchToPlayerSetup()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
        if (playerSetupPanel != null) playerSetupPanel.SetActive(true);
        
        UpdateLocalPlayerSetup();
        if (startLocalGameButton != null)
            startLocalGameButton.gameObject.SetActive(false);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);
        Debug.Log("CustomNetworkManager: Server scene changed to " + sceneName);
    }

    public override void OnClientSceneChanged()
    {
        base.OnClientSceneChanged();
        Debug.Log("CustomNetworkManager: Client scene changed to " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    private void SwitchToLobby()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (lobbyPanel != null) lobbyPanel.SetActive(true);
        if (playerSetupPanel != null) playerSetupPanel.SetActive(false);
    }

    private void SwitchToMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
        if (playerSetupPanel != null) playerSetupPanel.SetActive(false);
        
        // Reset local multiplayer state
        isLocalMultiplayer = false;
        localPlayerCount = 0;
        
        // Clear LobbyUI local players if any
        if (lobbyUI != null)
        {
            lobbyUI.Refresh();
        }
    }

    // Call this to register spawnable prefabs programmatically
    private void RegisterSpawnablePrefabs()
    {
        // Example: Register a prefab at runtime
        // GameObject playerPrefab = Resources.Load<GameObject>("Prefabs/PlayerPrefab");
        // if (playerPrefab != null)
        // {
        //     NetworkClient.RegisterPrefab(playerPrefab);
        //     Debug.Log("Registered player prefab for spawning");
        // }
        
        // You can also add prefabs to the spawnablePrefabs list
        // spawnablePrefabs.Add(yourPrefab);
    }
}