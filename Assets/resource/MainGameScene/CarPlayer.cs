using Mirror;
using UnityEngine;
using System.Collections;

public class CarPlayer : NetworkBehaviour
{
    private PrometeoCarController prometeoController;
    private CarController carController;
    private bool setupDone = false;

    [SyncVar(hook = nameof(OnGameStartedChanged))]
    public bool gameStarted = false;

    // ── HUD SyncVars (set by server, read by clients for HUD display) ──
    [SyncVar] public string playerName;
    [SyncVar] public int racePosition;   // 1-based position in race
    [SyncVar] public int totalRacers;    // total players in race
    [SyncVar] public int syncedLap;      // current lap (0-based)
    [SyncVar] public int syncedTotalLaps;

    void Awake()
    {
        // Support both controller types — prefabs may use either one
        prometeoController = GetComponent<PrometeoCarController>();
        carController = GetComponent<CarController>();

        if (prometeoController != null)
            prometeoController.enabled = false;
        else if (carController != null)
            carController.enabled = false;
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

        if (prometeoController != null)
        {
            prometeoController.enabled = true;
            Debug.Log($"[CarPlayer] PrometeoCarController enabled for {gameObject.name}");
        }
        else if (carController != null)
        {
            carController.enabled = true;
            Debug.Log($"[CarPlayer] CarController enabled for {gameObject.name}");
        }

        StartCoroutine(SetupCameraRoutine());
        setupDone = true;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log($"[CarPlayer] OnStartClient: {gameObject.name}, isLocalPlayer={isLocalPlayer}, isOwned={isOwned}");
        if ((isLocalPlayer || isOwned) && !setupDone)
            SetupLocalPlayer();

        // Spawn floating name label above this car for all clients
        StartCoroutine(SpawnNameLabelWhenReady());
    }

    IEnumerator SpawnNameLabelWhenReady()
    {
        // Wait until playerName SyncVar is populated
        float timeout = 5f;
        while (string.IsNullOrEmpty(playerName) && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (!string.IsNullOrEmpty(playerName))
        {
            bool isLocal = isOwned || isLocalPlayer;
            FloatingNameLabel.CreateForCar(transform, playerName, isLocal);
        }
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

    // ── Race Result RPCs (TargetRpc ensures delivery to each specific client) ──

    [TargetRpc]
    public void TargetShowRaceResult(NetworkConnection target, string playerName, int position, bool isYou)
    {
        Debug.Log($"[CarPlayer] TargetShowRaceResult received: {playerName} finished position {position}, isYou={isYou}");

        RaceResultUI ui = FindObjectOfType<RaceResultUI>();
        if (ui != null)
        {
            ui.ShowPlayerFinished(playerName, position, isYou);
        }
        else
        {
            Debug.LogError("[CarPlayer] RaceResultUI NOT FOUND in scene! Add a Canvas with RaceResultUI component to the game scene.");
        }
    }

    [TargetRpc]
    public void TargetShowRaceComplete(NetworkConnection target)
    {
        Debug.Log("[CarPlayer] TargetShowRaceComplete received");

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
