using Mirror;
using UnityEngine;

public class LobbyPlayer : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnReadyChanged))]
    public bool isReady;

    [SyncVar]
    public string playerName = "Player";

    public override void OnStartServer()
    {
        base.OnStartServer();
        playerName = "Player " + connectionToClient.connectionId;
        LobbyManager.Instance.AddPlayer(netIdentity);
        Debug.Log("LobbyPlayer: OnStartServer called. Added player: " + playerName);  // NEW: Confirm player added
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        LobbyManager.Instance.SetLocalPlayer(this);
        Debug.Log("LobbyPlayer: OnStartLocalPlayer called for " + playerName);  // NEW: Confirm local player set
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        if (isServer)
        {
            LobbyManager.Instance.RemovePlayer(netIdentity);
            Debug.Log("LobbyPlayer: Removed player on disconnect");  // NEW
        }
    }

    private void OnReadyChanged(bool oldValue, bool newValue)
    {
        if (isLocalPlayer)
        {
            LobbyManager.Instance.UpdateReadyButton();
            Debug.Log("LobbyPlayer: Ready changed to " + newValue);  // NEW: Confirm ready toggle
        }
    }

    [Command]
    public void CmdToggleReady()
    {
        isReady = !isReady;
        Debug.Log("LobbyPlayer: CmdToggleReady executed on server. New ready: " + isReady);  // NEW
    }
}