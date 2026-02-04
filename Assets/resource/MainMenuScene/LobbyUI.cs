using Mirror;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Collections.Generic;

public class LobbyUI : MonoBehaviour
{
    public static LobbyUI Instance { get; private set; }

    [Header("UI References")]
    public TextMeshProUGUI playerListText;
    public Button readyButton;
    public Button startGameButton;

    private LobbyPlayer localPlayer;
    private readonly List<LobbyPlayer> lobbyPlayers = new List<LobbyPlayer>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (readyButton != null)
            readyButton.onClick.AddListener(OnClickReady);
        
        if (startGameButton != null)
            startGameButton.onClick.AddListener(OnClickStartGame);

        Refresh();
    }

    void OnDestroy()
    {
        if (readyButton != null)
            readyButton.onClick.RemoveListener(OnClickReady);
        
        if (startGameButton != null)
            startGameButton.onClick.RemoveListener(OnClickStartGame);
    }

    public void RegisterPlayer(LobbyPlayer player)
    {
        if (!lobbyPlayers.Contains(player))
        {
            lobbyPlayers.Add(player);
            Debug.Log($"LobbyUI: Registered player {player.playerName}");
            Refresh();
        }
    }

    public void UnregisterPlayer(LobbyPlayer player)
    {
        if (lobbyPlayers.Contains(player))
        {
            lobbyPlayers.Remove(player);
            Debug.Log($"LobbyUI: Unregistered player {player.playerName}");
            Refresh();
        }
    }

    public void SetLocalPlayer(LobbyPlayer player)
    {
        localPlayer = player;
        Debug.Log($"LobbyUI: Local player set to {player.playerName}");
        Refresh();
    }

    public void OnClickReady()
    {
        if (localPlayer != null)
        {
            localPlayer.CmdToggleReady();
            Debug.Log("LobbyUI: Ready button clicked");
        }
    }

    public void OnClickStartGame()
    {
        if (isServer)
        {
            Debug.Log("LobbyUI: Starting game - Changing to GameScene");
            CustomNetworkManager.Singleton.ServerChangeScene("GameScene");
        }
    }

    public void Refresh()
    {
        UpdatePlayerList();
        UpdateReadyButton();
        UpdateStartButton();
    }

    private void UpdatePlayerList()
    {
        if (playerListText == null) return;

        string list = string.Join("\n", lobbyPlayers.Where(p => p != null).Select(p =>
        {
            return $"{p.playerName} {(p.isReady ? "(Ready)" : "")}";
        }));

        playerListText.text = list;
        if (!string.IsNullOrEmpty(list))
        {
            Debug.Log($"LobbyUI: Updated player list:\n{list}");
        }
    }

    private void UpdateReadyButton()
    {
        if (readyButton == null || localPlayer == null) return;

        TextMeshProUGUI buttonText = readyButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = localPlayer.isReady ? "Unready" : "Ready";
        }
    }

    private void UpdateStartButton()
    {
        if (startGameButton == null) return;

        bool allReady = lobbyPlayers.Count > 0 && lobbyPlayers.All(p => p != null && p.isReady);
        startGameButton.interactable = isServer && allReady;
        
        Debug.Log($"LobbyUI: Start button {startGameButton.interactable} (All ready: {allReady}, IsServer: {isServer})");
    }

    private bool isServer => NetworkServer.active;
}