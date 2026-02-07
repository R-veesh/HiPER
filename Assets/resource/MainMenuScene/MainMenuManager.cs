using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Buttons")]
    public Button hostButton;
    public Button joinButton;
    public Button quitButton;

    [Header("Network Settings")]
    public string networkAddress = "localhost";

    private void Start()
    {
        SetupButtonListeners();
        SetupNetworkManager();
    }

    void SetupButtonListeners()
    {
        hostButton.onClick.AddListener(OnHostClicked);
        joinButton.onClick.AddListener(OnJoinClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
    }

    void SetupNetworkManager()
    {
        var networkManager = NetworkManager.singleton as CustomNetworkManager;
        if (networkManager != null)
        {
            networkManager.networkAddress = networkAddress;
        }
    }

    void OnHostClicked()
    {
        if (!NetworkServer.active && !NetworkClient.active)
        {
            Debug.Log("Starting Host – will load LobbyScene");
            NetworkManager.singleton.StartHost();
        }
        else
        {
            Debug.LogWarning("Already connected as host or client");
        }
    }

    void OnJoinClicked()
    {
        if (!NetworkClient.active)
        {
            Debug.Log($"Joining game at {networkAddress}");
            NetworkManager.singleton.networkAddress = networkAddress;
            NetworkManager.singleton.StartClient();
        }
        else
        {
            Debug.LogWarning("Already connected as client");
        }
    }

    void OnQuitClicked()
    {
        Debug.Log("Quitting game");
        Application.Quit();
    }
}