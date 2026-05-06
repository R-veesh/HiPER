using Mirror;
using UnityEngine;
using resource.LobbyScene;
using resource.MainMenuScene;

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
                if (conn == null) continue;

                var lobbyPlayer = conn.identity != null ? conn.identity.GetComponent<LobbyPlayer>() : null;
                int requestedCarIndex = 0;
                string playerName = $"Player {conn.connectionId}";

                if (lobbyPlayer != null)
                {
                    requestedCarIndex = lobbyPlayer.selectedCarIndex;
                    playerName = lobbyPlayer.playerName;
                }
                else if (OfflineRaceConfig.Instance != null && OfflineRaceConfig.Instance.IsOfflineMode)
                {
                    requestedCarIndex = OfflineRaceConfig.Instance.SelectedCarIndex;
                    if (UserSession.Instance != null && !string.IsNullOrWhiteSpace(UserSession.Instance.DisplayName))
                        playerName = UserSession.Instance.DisplayName;
                    else
                        playerName = "Offline Player";

                    Debug.Log($"[CarSpawner] Using offline car selection {requestedCarIndex} for {playerName}");
                }
                else
                {
                    continue;
                }

                if (i >= spawnPoints.Length)
                {
                    Debug.LogWarning("[CarSpawner] Not enough spawn points for all players.");
                    break;
                }

                int carIndex = Mathf.Clamp(requestedCarIndex, 0, realCars.Length - 1);
                if (carIndex != requestedCarIndex)
                {
                    Debug.LogWarning($"[CarSpawner] Requested car index {requestedCarIndex} is out of range. Clamped to {carIndex}.");
                }

                if (realCars[carIndex] == null)
                {
                    Debug.LogError($"[CarSpawner] Car prefab at index {carIndex} is null. Cannot spawn for {playerName}.");
                    continue;
                }

                Debug.Log($"[CarSpawner] Spawning car {carIndex} for player {playerName} at spawn point {i}");

                GameObject car = Instantiate(
                    realCars[carIndex],
                    spawnPoints[i].position,
                    spawnPoints[i].rotation
                );

                NetworkServer.Spawn(car, conn);
                i++;
            }
        }
    }
}
