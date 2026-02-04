using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField ipInput;
    public Button hostButton;
    public Button joinButton;
    public Button localMultiplayerButton;
    public Button quitButton;

    private CustomNetworkManager networkManager;

    void Start()
    {
        networkManager = FindObjectOfType<CustomNetworkManager>();
        
        if (hostButton != null)
            hostButton.onClick.AddListener(OnClickHost);
        
        if (joinButton != null)
            joinButton.onClick.AddListener(OnClickJoin);
        
        if (localMultiplayerButton != null)
            localMultiplayerButton.onClick.AddListener(OnClickLocalMultiplayer);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(OnClickQuit);
    }

    void OnDestroy()
    {
        if (hostButton != null)
            hostButton.onClick.RemoveListener(OnClickHost);
        
        if (joinButton != null)
            joinButton.onClick.RemoveListener(OnClickJoin);
        
        if (localMultiplayerButton != null)
            localMultiplayerButton.onClick.RemoveListener(OnClickLocalMultiplayer);
        
        if (quitButton != null)
            quitButton.onClick.RemoveListener(OnClickQuit);
    }

    public void OnClickHost()
    {
        if (networkManager != null)
        {
            networkManager.OnClickHost();
        }
    }

    public void OnClickJoin()
    {
        if (networkManager != null)
        {
            networkManager.OnClickJoin();
        }
    }

    public void OnClickLocalMultiplayer()
    {
        if (networkManager != null)
        {
            networkManager.StartLocalMultiplayer();
        }
    }

    public void OnClickQuit()
    {
        Application.Quit();
    }
}