using Mirror;
using UnityEngine;

public class CarPlayer : NetworkBehaviour
{
    private CarController carController;

    [SyncVar]
    public bool gameStarted = false;

    void Awake()
    {
        carController = GetComponent<CarController>();

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
        else
            StartCoroutine(WaitForCameraAndSetup());
    }

    System.Collections.IEnumerator WaitForCameraAndSetup()
    {
        // Wait a few frames for CameraFollow to initialize
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        
        // Try to find CameraFollow component
        CameraFollow cam = FindObjectOfType<CameraFollow>();
        if (cam != null)
        {
            cam.SetTarget(transform);
            Debug.Log("[CarPlayer] Camera target set to: " + gameObject.name + " (via WaitForCamera)");
        }
        else
        {
            Debug.LogError("[CarPlayer] CameraFollow component NOT FOUND in scene!");
        }
    }

    public void EnableControl()
    {
        if (!isLocalPlayer) return;

        if (carController != null)
        {
            carController.enabled = true;
            Debug.Log("[CarPlayer] CarController enabled for: " + gameObject.name);
        }
        else
        {
            Debug.LogError("[CarPlayer] CarController component not found!");
        }

        // ✅ Set camera target - critical for camera following
        CameraFollow cam = FindObjectOfType<CameraFollow>();
        if (cam != null)
        {
            cam.SetTarget(transform);
            Debug.Log("[CarPlayer] Camera target set to: " + gameObject.name);
        }
        else
        {
            Debug.LogError("[CarPlayer] CameraFollow component NOT FOUND in scene!");
        }
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