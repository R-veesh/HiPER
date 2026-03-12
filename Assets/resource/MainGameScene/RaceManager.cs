using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System.Linq;

public class RaceManager : NetworkBehaviour
{
    public static RaceManager Instance;

    [Header("Race Settings")]
    public int totalLaps = 1;
    public int totalCheckpoints = 3; // set in Inspector to match scene checkpoint count

    // server-only data
    private Dictionary<uint, PlayerRaceData> playerData = new Dictionary<uint, PlayerRaceData>();
    private List<RaceFinishEntry> finishOrder = new List<RaceFinishEntry>();
    private bool raceFinished = false;
    private float positionUpdateTimer = 0f;
    private const float POSITION_UPDATE_INTERVAL = 0.25f;

    class PlayerRaceData
    {
        public string playerName;
        public int currentCheckpoint; // next expected checkpoint index
        public int currentLap;
        public bool finished;
        public CarPlayer carPlayer; // reference for RPC routing
    }

    public struct RaceFinishEntry
    {
        public string playerName;
        public int position;
        public uint netId;
    }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// Called by GameSpawnManager after spawning a car. Server only.
    /// </summary>
    [Server]
    public void RegisterPlayer(NetworkIdentity carIdentity, string playerName)
    {
        uint netId = carIdentity.netId;
        if (!playerData.ContainsKey(netId))
        {
            playerData[netId] = new PlayerRaceData
            {
                playerName = playerName,
                currentCheckpoint = 0,
                currentLap = 0,
                finished = false,
                carPlayer = carIdentity.GetComponent<CarPlayer>()
            };
            Debug.Log($"[RaceManager] Registered player: {playerName} (netId={netId}, carPlayer={playerData[netId].carPlayer != null})");

            // Set initial HUD SyncVars on CarPlayer
            CarPlayer cp = playerData[netId].carPlayer;
            if (cp != null)
            {
                cp.syncedTotalLaps = totalLaps;
                cp.totalRacers = playerData.Count;
            }

            // Update totalRacers for all players when a new player joins
            foreach (var kvp in playerData)
            {
                if (kvp.Value.carPlayer != null)
                    kvp.Value.carPlayer.totalRacers = playerData.Count;
            }
        }
    }

    void Update()
    {
        if (!isServer) return;
        if (raceFinished) return;

        positionUpdateTimer -= Time.deltaTime;
        if (positionUpdateTimer <= 0f)
        {
            positionUpdateTimer = POSITION_UPDATE_INTERVAL;
            UpdateRacePositions();
        }
    }

    /// <summary>
    /// Calculate real-time race positions based on lap, checkpoint, and distance progress.
    /// Runs on server, writes to CarPlayer SyncVars.
    /// </summary>
    [Server]
    void UpdateRacePositions()
    {
        if (playerData.Count == 0) return;

        // Build a sortable list
        var sortList = new List<KeyValuePair<uint, PlayerRaceData>>(playerData);

        // Sort: finished players first (by finish position), then by lap desc, checkpoint desc
        sortList.Sort((a, b) =>
        {
            // Finished players rank by their recorded finish position
            if (a.Value.finished && b.Value.finished)
                return GetFinishPosition(a.Key).CompareTo(GetFinishPosition(b.Key));
            if (a.Value.finished) return -1;
            if (b.Value.finished) return 1;

            // Compare by lap (desc)
            int lapCmp = b.Value.currentLap.CompareTo(a.Value.currentLap);
            if (lapCmp != 0) return lapCmp;

            // Compare by checkpoint (desc)
            int cpCmp = b.Value.currentCheckpoint.CompareTo(a.Value.currentCheckpoint);
            if (cpCmp != 0) return cpCmp;

            // Same lap and checkpoint — compare by distance to next checkpoint
            // (closer = ahead, so smaller distance = better = lower rank number)
            // Since we don't have checkpoint transforms here, keep current order
            return 0;
        });

        // Assign positions
        for (int i = 0; i < sortList.Count; i++)
        {
            CarPlayer cp = sortList[i].Value.carPlayer;
            if (cp != null)
            {
                cp.racePosition = i + 1;
                cp.syncedLap = sortList[i].Value.currentLap;
            }
        }
    }

    int GetFinishPosition(uint netId)
    {
        for (int i = 0; i < finishOrder.Count; i++)
        {
            if (finishOrder[i].netId == netId)
                return finishOrder[i].position;
        }
        return finishOrder.Count + 1;
    }

    /// <summary>
    /// Find any valid CarPlayer to use for broadcasting RPCs.
    /// RaceManager is a scene object whose RPCs may not work, so we route through CarPlayer.
    /// </summary>
    [Server]
    void NotifyAllPlayersOfFinish(string finishedPlayerName, int position, uint finisherNetId)
    {
        int sent = 0;
        foreach (var kvp in playerData)
        {
            CarPlayer cp = kvp.Value.carPlayer;
            if (cp != null && cp.connectionToClient != null)
            {
                bool isYou = (kvp.Key == finisherNetId);
                cp.TargetShowRaceResult(cp.connectionToClient, finishedPlayerName, position, isYou);
                sent++;
            }
        }
        Debug.Log($"[RaceManager] Sent finish notification to {sent} clients");
    }

    [Server]
    void NotifyAllPlayersRaceComplete()
    {
        int sent = 0;
        foreach (var kvp in playerData)
        {
            CarPlayer cp = kvp.Value.carPlayer;
            if (cp != null && cp.connectionToClient != null)
            {
                cp.TargetShowRaceComplete(cp.connectionToClient);
                sent++;
            }
        }
        Debug.Log($"[RaceManager] Sent race complete notification to {sent} clients");
    }

    /// <summary>
    /// Called by CarPlayer via Command when hitting a checkpoint.
    /// </summary>
    [Server]
    public void ServerReportCheckpoint(NetworkIdentity carIdentity, int checkpointIndex, bool isFinishLine)
    {
        uint netId = carIdentity.netId;
        if (!playerData.ContainsKey(netId)) return;

        PlayerRaceData data = playerData[netId];
        if (data.finished) return;

        // must hit checkpoints in order
        if (checkpointIndex != data.currentCheckpoint)
        {
            Debug.Log($"[RaceManager] {data.playerName} hit checkpoint {checkpointIndex} but expected {data.currentCheckpoint} — ignored");
            return;
        }

        Debug.Log($"[RaceManager] {data.playerName} passed checkpoint {checkpointIndex}");
        data.currentCheckpoint++;

        // crossed finish line after all checkpoints?
        if (isFinishLine && data.currentCheckpoint >= totalCheckpoints)
        {
            data.currentLap++;
            data.currentCheckpoint = 0;
            Debug.Log($"[RaceManager] {data.playerName} completed lap {data.currentLap}/{totalLaps}");

            if (data.currentLap >= totalLaps)
            {
                data.finished = true;
                int position = finishOrder.Count + 1;
                finishOrder.Add(new RaceFinishEntry
                {
                    playerName = data.playerName,
                    position = position,
                    netId = netId
                });
                Debug.Log($"[RaceManager] {data.playerName} FINISHED in position {position}!");

                // Notify ALL clients via each player's own CarPlayer TargetRpc
                // This guarantees delivery to every client through their own connection
                NotifyAllPlayersOfFinish(data.playerName, position, netId);

                // check if all players finished
                if (finishOrder.Count >= playerData.Count)
                {
                    raceFinished = true;
                    Debug.Log("[RaceManager] Race complete! Notifying all clients.");
                    NotifyAllPlayersRaceComplete();
                }
            }
        }
    }
}
