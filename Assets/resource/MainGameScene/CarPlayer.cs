using Mirror;
using UnityEngine;
using System.Collections;

public class CarPlayer : NetworkBehaviour
{
    private CarController carController;
    private bool setupDone = false;

    [SyncVar(hook = nameof(OnGameStartedChanged))]
    public bool gameStarted = false;

    void Awake()
    {
        carController = GetComponent<CarController>();
        if (carController != null)
            carController.enabled = false;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log($"[CarPlayer] OnStartClient: {gameObject.name}, isLocalPlayer={isLocalPlayer}, isOwned={isOwned}");
        if ((isLocalPlayer || isOwned) && !setupDone)
            SetupLocalPlayer();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Debug.Log($"[CarPlayer] OnStartLocalPlayer: {gameObject.name}");
        if (!setupDone)
            SetupLocalPlayer();
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        Debug.Log($"[CarPlayer] OnStartAuthority: {gameObject.name}");
        if (!setupDone)
            SetupLocalPlayer();
    }

    void SetupLocalPlayer()
    {
        if (setupDone) return;
        if (!isLocalPlayer && !isOwned)
        {
            Debug.Log("[CarPlayer] not owned, skipping setup");
            return;
        }

        if (carController != null)
        {
            carController.enabled = true;
            Debug.Log($"[CarPlayer] CarController enabled for {gameObject.name}");
        }

        StartCoroutine(SetupCameraRoutine());
        setupDone = true;
    }

    IEnumerator SetupCameraRoutine()
    {
        // wait one frame so scene objects are ready
        yield return null;

        for (int i = 0; i < 10; i++)
        {
            CameraFollow cam = FindObjectOfType<CameraFollow>();
            if (cam != null)
            {
                cam.SetTarget(transform);
                Debug.Log($"[CarPlayer] Camera target set to: {gameObject.name}");
                yield break;
            }
            yield return new WaitForSeconds(0.3f);
        }
        Debug.LogError("[CarPlayer] CameraFollow not found after retries!");
    }

    void OnGameStartedChanged(bool oldVal, bool newVal)
    {
        Debug.Log($"[CarPlayer] gameStarted: {newVal} for {gameObject.name}, isLocalPlayer={isLocalPlayer}, isOwned={isOwned}");
        if (newVal && (isLocalPlayer || isOwned) && !setupDone)
            SetupLocalPlayer();
    }

    [Server]
    public void ServerStartGame()
    {
        if (!isServer) return;
        gameStarted = true;
        RpcStartGame();
    }

    [ClientRpc]
    void RpcStartGame()
    {
        Debug.Log($"[CarPlayer] RpcStartGame on {gameObject.name}, isLocalPlayer={isLocalPlayer}, isOwned={isOwned}");
        if ((isLocalPlayer || isOwned) && !setupDone)
            SetupLocalPlayer();
    }

    void OnDestroy()
    {
        if (isOwned)
        {
            CameraFollow cam = FindObjectOfType<CameraFollow>();
            if (cam != null) cam.ClearTarget();
        }
    }

    // ── Checkpoint / Race detection (additive – does not touch anything above) ──

    void OnTriggerEnter(Collider other)
    {
        if (!isOwned) return; // only the owning client sends the command

        Checkpoint cp = other.GetComponent<Checkpoint>();
        if (cp != null)
        {
            CmdReportCheckpoint(cp.checkpointIndex, cp.isFinishLine);
        }
    }

    [Command]
    void CmdReportCheckpoint(int checkpointIndex, bool isFinishLine)
    {
        if (RaceManager.Instance != null)
        {
            RaceManager.Instance.ServerReportCheckpoint(netIdentity, checkpointIndex, isFinishLine);
        }
    }

    // ── Race Result RPCs (routed through CarPlayer because scene-object RPCs may not work) ──

    [ClientRpc]
    public void RpcShowRaceResult(string playerName, int position)
    {
        Debug.Log($"[CarPlayer] RpcShowRaceResult received: {playerName} finished position {position}");

        RaceResultUI ui = FindObjectOfType<RaceResultUI>();
        if (ui != null)
        {
            ui.ShowPlayerFinished(playerName, position);
        }
        else
        {
            Debug.LogError("[CarPlayer] RaceResultUI NOT FOUND in scene! Add a Canvas with RaceResultUI component to the game scene.");
        }
    }

    [ClientRpc]
    public void RpcShowRaceComplete()
    {
        Debug.Log("[CarPlayer] RpcShowRaceComplete received");

        RaceResultUI ui = FindObjectOfType<RaceResultUI>();
        if (ui != null)
        {
            ui.ShowRaceComplete();
        }
        else
        {
            Debug.LogError("[CarPlayer] RaceResultUI NOT FOUND in scene! Add a Canvas with RaceResultUI component to the game scene.");
        }
    }
}
