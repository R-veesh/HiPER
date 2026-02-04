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
        
        if (LobbyUI.Instance != null)
        {
            LobbyUI.Instance.RegisterPlayer(this);
        }
        Debug.Log("LobbyPlayer: OnStartServer called. Added player: " + playerName);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        if (LobbyUI.Instance != null)
        {
            LobbyUI.Instance.RegisterPlayer(this);
        }
        Debug.Log("LobbyPlayer: OnStartClient called for " + playerName);
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        
        if (LobbyUI.Instance != null)
        {
            LobbyUI.Instance.SetLocalPlayer(this);
        }
        Debug.Log("LobbyPlayer: OnStartLocalPlayer called for " + playerName);
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        
        if (LobbyUI.Instance != null)
        {
            LobbyUI.Instance.UnregisterPlayer(this);
        }
        Debug.Log("LobbyPlayer: Removed player on disconnect");
    }

    private void OnReadyChanged(bool oldValue, bool newValue)
    {
        if (LobbyUI.Instance != null)
        {
            LobbyUI.Instance.Refresh();
        }
        Debug.Log("LobbyPlayer: Ready changed to " + newValue);
    }

    [Command]
    public void CmdToggleReady()
    {
        isReady = !isReady;
        Debug.Log("LobbyPlayer: CmdToggleReady executed on server. New ready: " + isReady);
    }
}