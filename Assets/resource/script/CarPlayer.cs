using Mirror;
using UnityEngine;

public class CarPlayer : NetworkBehaviour
{
    private PrometeoCarController carController;

    [SyncVar]
    public bool gameStarted = false;

    void Awake()
    {
        carController = GetComponent<PrometeoCarController>();

        // Disable control for everyone by default
        if (carController != null)
            carController.enabled = false;
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        // If the game already started, enable control now
        if (gameStarted)
            EnableControl();
    }

    void EnableControl()
    {
        if (!isLocalPlayer) return;

        if (carController != null)
            carController.enabled = true;

        // ✅ Correct camera assignment (local only)
        CameraFollow cam = FindFirstObjectByType<CameraFollow>();
        if (cam != null)
            cam.SetTarget(transform);
    }

    // ---------- LOBBY / GAME FLOW ----------

    [Command]
    public void CmdStartGame()
    {
        gameStarted = true;
        RpcStartGame();
    }

    [ClientRpc]
    void RpcStartGame()
    {
        if (isLocalPlayer)
            EnableControl();
    }
}