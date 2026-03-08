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
        }
    }

    /// <summary>
    /// Find any valid CarPlayer to use for broadcasting RPCs.
    /// RaceManager is a scene object whose RPCs may not work, so we route through CarPlayer.
    /// </summary>
    [Server]
    CarPlayer GetAnyCarPlayer()
    {
        foreach (var kvp in playerData)
        {
            if (kvp.Value.carPlayer != null)
                return kvp.Value.carPlayer;
        }
        return null;
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
                    position = position
                });
                Debug.Log($"[RaceManager] {data.playerName} FINISHED in position {position}!");

                // Route RPC through CarPlayer (scene-object RPCs on RaceManager don't work)
                CarPlayer broadcaster = GetAnyCarPlayer();
                if (broadcaster != null)
                {
                    Debug.Log($"[RaceManager] Broadcasting finish via CarPlayer: {broadcaster.gameObject.name}");
                    broadcaster.RpcShowRaceResult(data.playerName, position);
                }
                else
                {
                    Debug.LogError("[RaceManager] No CarPlayer found to broadcast RPC!");
                }

                // check if all players finished
                if (finishOrder.Count >= playerData.Count)
                {
                    raceFinished = true;
                    if (broadcaster != null)
                    {
                        Debug.Log("[RaceManager] Broadcasting race complete via CarPlayer");
                        broadcaster.RpcShowRaceComplete();
                    }
                }
            }
        }
    }
}
