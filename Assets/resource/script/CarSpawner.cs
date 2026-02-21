using Mirror;
using UnityEngine;
using resource.LobbyScene;

namespace resource.script
{
    public class CarSpawner : NetworkBehaviour
    {
        public GameObject[] realCars;
        public Transform[] spawnPoints;

        public override void OnStartServer()
        {
            Debug.Log($"[CarSpawner] OnStartServer called. Connections: {NetworkServer.connections.Count}");

            if (realCars == null || realCars.Length == 0)
            {
                Debug.LogError("[CarSpawner] realCars array is empty!");
                return;
            }

            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogError("[CarSpawner] spawnPoints array is empty!");
                return;
            }

            int i = 0;
            foreach (var conn in NetworkServer.connections.Values)
            {
                if (conn == null || conn.identity == null) continue;

                var lobbyPlayer = conn.identity.GetComponent<LobbyPlayer>();
                if (lobbyPlayer == null) continue;

                Debug.Log($"[CarSpawner] Spawning car {lobbyPlayer.selectedCarIndex} for player {lobbyPlayer.playerName} at spawn point {i}");

                GameObject car = Instantiate(
                    realCars[lobbyPlayer.selectedCarIndex],
                    spawnPoints[i].position,
                    spawnPoints[i].rotation
                );

                NetworkServer.Spawn(car, conn);
                i++;
            }
        }
    }
}
