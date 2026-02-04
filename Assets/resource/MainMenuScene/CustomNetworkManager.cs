using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class CustomNetworkManager : NetworkManager
{
    public static CustomNetworkManager Singleton => (CustomNetworkManager)singleton;

    public GameObject mainMenuPanel;  // Drag MainMenuPanel in Inspector
    public GameObject lobbyPanel;     // Drag LobbyPanel in Inspector
    public InputField ipInputField;   // Drag IPInputField in Inspector

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (lobbyPanel != null) lobbyPanel.SetActive(true);
        Debug.Log("CustomNetworkManager: OnStartClient - Showing LobbyPanel");  // NEW
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
        Debug.Log("CustomNetworkManager: OnStopClient - Showing MainMenuPanel");  // NEW
    }

    public void JoinGame()
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

    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);
        Debug.Log("CustomNetworkManager: Server scene changed to " + sceneName);  // NEW: Confirm scene load on server
    }

    public override void OnClientSceneChanged()
    {
        base.OnClientSceneChanged();
        Debug.Log("CustomNetworkManager: Client scene changed to " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);  // NEW: Confirm on client (host is client too)
    }
}