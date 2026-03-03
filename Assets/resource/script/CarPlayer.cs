using Mirror;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

public class CarPlayer : NetworkBehaviour
{
    private CarController carController;

    [SyncVar(hook = nameof(OnGameStartedChanged))]
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
        Debug.Log("[CarPlayer] OnStartLocalPlayer called for: " + gameObject.name);
        
        // Enable control for local player
        EnableControl();
        
        // Retry camera assignment if it failed (race condition safety)
        CameraFollow cam = FindObjectOfType<CameraFollow>();
        if (cam == null || cam.target != transform)
        {
            StartCoroutine(RetryCameraAssignment());
        }
    }

    private System.Collections.IEnumerator RetryCameraAssignment()
    {
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(0.5f);
            CameraFollow cam = FindObjectOfType<CameraFollow>();
            if (cam != null && cam.target == null)
            {
                cam.SetTarget(transform);
                Debug.Log("[CarPlayer] Camera target set on retry #" + (i + 1) + " for: " + gameObject.name);
                yield break;
            }
        }
        Debug.LogError("[CarPlayer] Failed to assign camera target after 5 retries for: " + gameObject.name);
    }

    void OnGameStartedChanged(bool oldValue, bool newValue)
    {
        Debug.Log("[CarPlayer] gameStarted changed to: " + newValue + " for " + gameObject.name);
        if (newValue && isLocalPlayer)
        {
            EnableControl();
        }
    }

    public void EnableControl()
    {
        if (!isLocalPlayer) 
        {
            Debug.Log("[CarPlayer] Not local player, skipping control enable for: " + gameObject.name);
            return;
        }

        if (carController != null)
        {
            carController.enabled = true;
            Debug.Log("[CarPlayer] CarController ENABLED for local player: " + gameObject.name);
        }
        else
        {
            Debug.LogError("[CarPlayer] CarController component not found on: " + gameObject.name);
        }

        // Set camera target
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

    // Called by server to start the game for all players
    [Server]
    public void ServerStartGame()
    {
        // Safety: verify NetworkIdentity is properly initialized
        if (netIdentity == null || !netIdentity.isServer)
        {
            Debug.LogError("[CarPlayer] Cannot start game - NetworkIdentity not initialized! Car: " + gameObject.name);
            return;
        }
        
        gameStarted = true;
        RpcStartGame();
    }

    [ClientRpc]
    void RpcStartGame()
    {
        Debug.Log("[CarPlayer] RpcStartGame received on: " + gameObject.name + " isLocalPlayer: " + isLocalPlayer);
        if (isLocalPlayer)
            EnableControl();
    }
}