using Mirror;
using UnityEngine;
using System.Collections.Generic;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Player Prefab")]
    public GameObject lobbyPlayerPrefab;

    private List<LobbyPlayer> lobbyPlayers = new List<LobbyPlayer>();
    private bool[] usedSpawnPoints;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        usedSpawnPoints = new bool[spawnPoints.Length];
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("LobbyManager started on server");
    }

    public void OnPlayerAdded(NetworkConnectionToClient conn)
    {
        Debug.Log($"Adding player for connection {conn.connectionId}");

        // Find available spawn point
        int spawnIndex = GetAvailableSpawnPoint();
        if (spawnIndex == -1)
        {
            Debug.LogError("No available spawn points!");
            return;
        }

        // Spawn player at assigned position
        GameObject player = Instantiate(lobbyPlayerPrefab, spawnPoints[spawnIndex].position, spawnPoints[spawnIndex].rotation);
        NetworkServer.AddPlayerForConnection(conn, player);

        // Assign spawn point to player
        var lobbyPlayer = player.GetComponent<LobbyPlayer>();
        lobbyPlayer.SetPlatePosition(spawnPoints[spawnIndex]);

        // Mark spawn point as used
        usedSpawnPoints[spawnIndex] = true;
        lobbyPlayers.Add(lobbyPlayer);

        Debug.Log($"Player {conn.connectionId} spawned at plate {spawnIndex + 1}");
    }

    public void OnPlayerRemoved(NetworkConnectionToClient conn)
    {
        if (conn.identity != null)
        {
            var lobbyPlayer = conn.identity.GetComponent<LobbyPlayer>();
            if (lobbyPlayer != null)
            {
                // Free up spawn point
                for (int i = 0; i < spawnPoints.Length; i++)
                {
                    if (spawnPoints[i].position == lobbyPlayer.transform.position)
                    {
                        usedSpawnPoints[i] = false;
                        break;
                    }
                }

                lobbyPlayers.Remove(lobbyPlayer);
            }
        }
    }

    int GetAvailableSpawnPoint()
    {
        for (int i = 0; i < usedSpawnPoints.Length; i++)
        {
            if (!usedSpawnPoints[i])
                return i;
        }
        return -1;
    }

    public bool AllPlayersReady()
    {
        foreach (var player in lobbyPlayers)
        {
            if (!player.isReady)
                return false;
        }
        return lobbyPlayers.Count > 0;
    }

    public void OnStartClicked()
    {
        if (!isServer) return; // HOST ONLY

        if (!AllPlayersReady())
        {
            Debug.Log("Not all players are ready!");
            return;
        }

        Debug.Log("All players ready! Starting game...");
        var networkManager = NetworkManager.singleton as CustomNetworkManager;
        if (networkManager != null)
        {
            networkManager.LoadGameScene();
        }
    }

    public void OnReadyClicked()
    {
        if (NetworkClient.localPlayer != null)
        {
            NetworkClient.localPlayer.GetComponent<LobbyPlayer>()?.CmdSetReady();
        }
    }
}