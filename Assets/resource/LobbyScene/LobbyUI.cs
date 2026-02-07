using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class LobbyUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button readyButton;
    public Button startButton;
    public Button nextCarButton;
    public Button prevCarButton;

    [Header("UI Elements")]
    public TextMeshProUGUI readyButtonText;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI carSelectionText;

    [Header("Car Selection")]
    public string[] carNames;

    private LobbyPlayer localLobbyPlayer;
    private LobbyManager lobbyManager;

    void Start()
    {
        SetupButtonListeners();
        HideStartButton();
        
        lobbyManager = LobbyManager.Instance;
        if (lobbyManager == null)
        {
            Debug.LogError("LobbyManager not found!");
        }
    }

    void Update()
    {
        if (localLobbyPlayer == null)
        {
            FindLocalPlayer();
        }
        else
        {
            UpdateUI();
        }
    }

    void FindLocalPlayer()
    {
        if (NetworkClient.localPlayer != null)
        {
            localLobbyPlayer = NetworkClient.localPlayer.GetComponent<LobbyPlayer>();
            if (localLobbyPlayer != null)
            {
                UpdateUI();
            }
        }
    }

    void SetupButtonListeners()
    {
        readyButton.onClick.AddListener(OnReadyClicked);
        startButton.onClick.AddListener(OnStartClicked);
        nextCarButton.onClick.AddListener(OnNextCarClicked);
        prevCarButton.onClick.AddListener(OnPrevCarClicked);
    }

    void UpdateUI()
    {
        if (localLobbyPlayer == null) return;

        // Update ready button
        readyButtonText.text = localLobbyPlayer.isReady ? "NOT READY" : "READY";
        
        // Update car selection text
        if (carNames != null && carNames.Length > localLobbyPlayer.selectedCarIndex)
        {
            carSelectionText.text = $"Car: {carNames[localLobbyPlayer.selectedCarIndex]}";
        }

        // Update car selection buttons
        bool canSelectCar = !localLobbyPlayer.isReady;
        nextCarButton.interactable = canSelectCar;
        prevCarButton.interactable = canSelectCar;

        // Update status text
        if (lobbyManager != null && NetworkServer.active)
        {
            bool allReady = lobbyManager.AllPlayersReady();
            statusText.text = allReady ? "All players ready!" : "Waiting for players...";
            ShowStartButton(allReady && NetworkServer.active && NetworkManager.singleton.mode == NetworkManagerMode.Host);
        }
    }

    void ShowStartButton(bool show)
    {
        startButton.gameObject.SetActive(show);
    }

    void HideStartButton()
    {
        startButton.gameObject.SetActive(false);
    }

    void OnReadyClicked()
    {
        lobbyManager?.OnReadyClicked();
    }

    void OnStartClicked()
    {
        lobbyManager?.OnStartClicked();
    }

    void OnNextCarClicked()
    {
        localLobbyPlayer?.CmdNextCar();
    }

    void OnPrevCarClicked()
    {
        localLobbyPlayer?.CmdPrevCar();
    }

    public void OnPlayerDisconnected()
    {
        localLobbyPlayer = null;
        HideStartButton();
    }
}