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
            int i = 0;
            foreach (var conn in NetworkServer.connections.Values)
            {
                var lobbyPlayer = conn.identity.GetComponent<LobbyPlayer>();

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
