using Mirror;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance { get; private set; }

    public Text playerListText;      // Drag PlayerListText (or TMPro if using)
    public Button readyButton;       // Drag ReadyButton
    public Button startGameButton;   // Drag StartGameButton

    private LobbyPlayer localPlayer;
    private readonly SyncList<NetworkIdentity> lobbyPlayers = new SyncList<NetworkIdentity>();

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
        DontDestroyOnLoad(gameObject);  // NEW: Ensure persistence across scenes
    }

    public void AddPlayer(NetworkIdentity playerId)
    {
        if (!isServer) return;
        lobbyPlayers.Add(playerId);
        Debug.Log("LobbyManager: Added player. Total players: " + lobbyPlayers.Count);  // NEW
    }

    public void RemovePlayer(NetworkIdentity playerId)
    {
        if (!isServer) return;
        lobbyPlayers.Remove(playerId);
        Debug.Log("LobbyManager: Removed player. Total players: " + lobbyPlayers.Count);  // NEW
    }

    public void SetLocalPlayer(LobbyPlayer player)
    {
        localPlayer = player;
        UpdateReadyButton();
        Debug.Log("LobbyManager: Local player set: " + player.playerName);  // NEW
    }

    public void UpdateReadyButton()
    {
        if (readyButton == null || localPlayer == null) return;
        readyButton.GetComponentInChildren<Text>().text = localPlayer.isReady ? "Unready" : "Ready";
        Debug.Log("LobbyManager: Updated ready button text");  // NEW
    }

    [ClientCallback]
    private void Update()
    {
        if (!isClient) return;
        UpdatePlayerListUI();
        UpdateStartButton();
    }

    private void UpdatePlayerListUI()
    {
        if (playerListText == null) return;
        string list = string.Join("\n", lobbyPlayers.Where(p => p != null).Select(p =>
        {
            var lp = p.GetComponent<LobbyPlayer>();
            return lp.playerName + (lp.isReady ? " (Ready)" : "");
        }));
        playerListText.text = list;
        if (!string.IsNullOrEmpty(list)) Debug.Log("LobbyManager: Updated player list: " + list);  // NEW: Confirm list update
    }

    private void UpdateStartButton()
    {
        if (startGameButton == null) return;
        bool allReady = lobbyPlayers.Count > 0 && lobbyPlayers.All(p => p != null && p.GetComponent<LobbyPlayer>().isReady);
        startGameButton.interactable = isServer && allReady;
        Debug.Log("LobbyManager: Start button interactable: " + startGameButton.interactable + " (All ready: " + allReady + ", IsServer: " + isServer + ")");  // NEW: Key debug for button enable
    }

    public void StartGame()
    {
        if (!isServer) return;
        Debug.Log("LobbyManager: Starting game - Changing to GameScene");  // NEW: Confirm start triggered
        CustomNetworkManager.Singleton.ServerChangeScene("GameScene");
    }

    public void ToggleReady()
    {
        if (localPlayer != null)
        {
            localPlayer.CmdToggleReady();
            Debug.Log("LobbyManager: ToggleReady called");  // NEW
        }
    }
}