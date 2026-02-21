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
            Debug.Log($"[LobbyPlayer] OnStartClient called for {playerName} - isLocalPlayer: {isLocalPlayer}, isServer: {isServer}");
            
            // Only server spawns cars - clients will see them via network sync
            if (isServer)
            {
                // Server spawns immediately for ALL players (including remote ones)
                SpawnPreviewCar();
            }
            // Clients do NOT spawn cars locally - they receive them from server
        }

        public void SetPlatePosition(Transform plateTransform, int index)
        {
            Debug.Log($"[LobbyPlayer] SetPlatePosition called - Index: {index}, isServer: {isServer}");
            
            plateIndex = index;
            
            // Move immediately on server
            transform.position = plateTransform.position;
            transform.rotation = plateTransform.rotation;
            
            // Sync to clients
            if (isServer)
            {
                RpcUpdatePosition(plateTransform.position, plateTransform.rotation);
            }
            
            // Spawn car at new position (server only)
            if (isServer)
            {
                SpawnPreviewCar();
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
            Debug.Log($"[LobbyPlayer] SpawnPreviewCar called - isServer: {isServer}, isLocalPlayer: {isLocalPlayer}, carPrefabs: {(carPrefabs != null ? carPrefabs.Length : 0)}");
            
            // Destroy existing preview car
            if (_previewCar != null)
            {
                if (isServer && _previewCar.GetComponent<NetworkIdentity>() != null)
                    NetworkServer.Destroy(_previewCar);
                else
                    Destroy(_previewCar);
                _previewCar = null;
            }

            // Spawn new preview car if we have car prefabs
            if (carPrefabs == null || carPrefabs.Length == 0)
            {
                Debug.LogError("[LobbyPlayer] Cannot spawn car - carPrefabs array is empty! Make sure to assign car prefabs in the LobbyPlayer prefab.");
                return;
            }
            
            if (selectedCarIndex >= carPrefabs.Length)
            {
                Debug.LogError($"[LobbyPlayer] selectedCarIndex ({selectedCarIndex}) is out of range! Array length: {carPrefabs.Length}");
                return;
            }
            
            if (carPrefabs[selectedCarIndex] == null)
            {
                Debug.LogError($"[LobbyPlayer] Car prefab at index {selectedCarIndex} is null!");
                return;
            }
            
            // Only spawn on server - clients will see it via network sync
            if (!isServer) return;
            
            // Position car on the plate (slightly above)
            Vector3 carPosition = transform.position + Vector3.up * 0.1f;
            
            // Server spawns networked car
            _previewCar = Instantiate(carPrefabs[selectedCarIndex], carPosition, transform.rotation);
            NetworkServer.Spawn(_previewCar);
            Debug.Log($"[SERVER] Spawned car preview: {carPrefabs[selectedCarIndex].name} for {playerName}");
        }

        void OnCarChanged(int oldIndex, int newIndex)
        {
            Debug.Log($"[LobbyPlayer] Car changed from {oldIndex} to {newIndex} for {playerName}");
            // Only server spawns cars
            if (isServer)
            {
                SpawnPreviewCar();
            }
        }

        void OnReadyStateChanged(bool oldState, bool newState)
        {
            Debug.Log($"[LobbyPlayer] {playerName} ready state changed: {oldState} -> {newState}");
            
            // Notify LobbyManager to update ready counts
            if (isServer && LobbyManager.Instance != null)
            {
                LobbyManager.Instance.UpdatePlayerReadyState(this);
            }
            
            // Trigger UI update on all clients
            if (LobbyUI.Instance != null)
            {
                LobbyUI.Instance.UpdateUI();
            }
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
            Debug.Log($"[LobbyPlayer] {playerName} set ready to: {isReady} (server: {isServer})");
            
            // Notify LobbyManager immediately on server
            if (isServer && LobbyManager.Instance != null)
            {
                LobbyManager.Instance.UpdatePlayerReadyState(this);
            }
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
