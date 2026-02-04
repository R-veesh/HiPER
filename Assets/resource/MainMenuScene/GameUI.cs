using UnityEngine;

public class GameUI : MonoBehaviour
{
    public static GameUI instance;

    public GameObject mainMenuPanel;
    public GameObject lobbyPanel;

    void Awake()
    {
        instance = this;
    }

    public void ShowLobby()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (lobbyPanel != null) lobbyPanel.SetActive(true);
    }

    public void ShowMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
    }
}