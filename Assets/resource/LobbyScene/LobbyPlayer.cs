using UnityEngine;
using Mirror;

namespace resource.LobbyScene
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class LobbyPlayer : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnCarChanged))]
        public int selectedCarIndex;

        [SyncVar(hook = nameof(OnReadyStateChanged))]
        public bool isReady;

        [SyncVar]
        public string playerName = "Player";

        [SyncVar(hook = nameof(OnPlateIndexChanged))]
        public int plateIndex = -1;

        [SyncVar(hook = nameof(OnMapVoteChanged))]
        public int selectedMapIndex = 0;

        [Header("Prefabs")]
        public GameObject[] carPrefabs;

        private GameObject _previewCar;

        public override void OnStartClient()
        {
            SpawnPreviewCar();
            Debug.Log($"LobbyPlayer spawned for {playerName}");
        }

        public void SetPlatePosition(Transform plateTransform, int index)
        {
            plateIndex = index;
            
            if (isServer)
            {
                RpcUpdatePosition(plateTransform.position, plateTransform.rotation);
            }
        }

        [ClientRpc]
        void RpcUpdatePosition(Vector3 position, Quaternion rotation)
        {
            transform.position = position;
            transform.rotation = rotation;
        }

        void OnPlateIndexChanged(int _, int newIndex)
        {
            // Update position when plate index changes
            if (newIndex >= 0 && LobbyManager.Instance != null)
            {
                var spawnPoints = LobbyManager.Instance.spawnPoints;
                if (newIndex < spawnPoints.Length)
                {
                    transform.position = spawnPoints[newIndex].position;
                    transform.rotation = spawnPoints[newIndex].rotation;
                }
            }
        }

        void SpawnPreviewCar()
        {
            // Destroy existing preview car
            if (_previewCar != null)
            {
                if (isServer)
                    NetworkServer.Destroy(_previewCar);
                else
                    Destroy(_previewCar);
                _previewCar = null;
            }

            // Spawn new preview car if we have car prefabs
            if (carPrefabs != null && carPrefabs.Length > 0 && selectedCarIndex < carPrefabs.Length)
            {
                // Position car on the plate (slightly above)
                Vector3 carPosition = transform.position + Vector3.up * 0.1f;
                
                if (isServer)
                {
                    // Server spawns and has authority
                    _previewCar = Instantiate(carPrefabs[selectedCarIndex], carPosition, transform.rotation);
                    NetworkServer.Spawn(_previewCar);
                }
                else
                {
                    // Client spawns local preview only
                    _previewCar = Instantiate(carPrefabs[selectedCarIndex], carPosition, transform.rotation);
                }
                
                Debug.Log($"Spawned car preview: {carPrefabs[selectedCarIndex].name} for {playerName}");
            }
        }

        void OnCarChanged(int _, int newIndex)
        {
            SpawnPreviewCar();
        }

        void OnReadyStateChanged(bool _, bool newState)
        {
            Debug.Log($"{playerName} ready state: {newState}");
            // Update UI here if needed
        }

        [Command]
        public void CmdNextCar()
        {
            if (isReady) return; // Can't change car when ready
            if (carPrefabs == null || carPrefabs.Length == 0) return; // Safety check
            
            selectedCarIndex = (selectedCarIndex + 1) % carPrefabs.Length;
        }

        [Command]
        public void CmdPrevCar()
        {
            if (isReady) return; // Can't change car when ready
            if (carPrefabs == null || carPrefabs.Length == 0) return; // Safety check
            
            selectedCarIndex--;
            if (selectedCarIndex < 0)
                selectedCarIndex = carPrefabs.Length - 1;
        }

        [Command]
        public void CmdSetReady()
        {
            isReady = !isReady;
            Debug.Log($"{playerName} set ready to: {isReady}");
        }

        [Command]
        public void CmdSetPlayerName(string newName)
        {
            playerName = newName;
        }

        [Command]
        public void CmdVoteForMap(int mapIndex)
        {
            if (isReady) return; // Can't change vote when ready
            selectedMapIndex = mapIndex;
            Debug.Log($"{playerName} voted for map index: {mapIndex}");
            
            // Notify lobby manager to update map voting - use RPC to ensure server processes it
            RpcNotifyMapVoteChanged();
        }

        [ClientRpc]
        void RpcNotifyMapVoteChanged()
        {
            // Only server processes the vote update
            if (isServer && LobbyManager.Instance != null)
            {
                LobbyManager.Instance.OnPlayerVotedForMap();
            }
        }

        void OnMapVoteChanged(int _, int newIndex)
        {
            Debug.Log($"{playerName} map vote changed to: {newIndex}");
            // UI will update through hook
        }

        public override void OnStopClient()
        {
            if (_previewCar != null)
            {
                if (isServer)
                    NetworkServer.Destroy(_previewCar);
                else
                    Destroy(_previewCar);
                _previewCar = null;
            }
        }

        public override void OnStopServer()
        {
            if (_previewCar != null)
            {
                NetworkServer.Destroy(_previewCar);
                _previewCar = null;
            }
        }
    }
}
