using UnityEngine;

namespace resource.LobbyScene
{
    /// <summary>
    /// Dynamic lobby camera that zooms out as more players join.
    /// Set up 4 Transform presets in the lobby scene (empty GameObjects) 
    /// marking the ideal camera position/rotation for 1, 2, 3, and 4 players.
    /// Hook: LobbyManager.connectedPlayerCount changes trigger camera transitions.
    /// </summary>
    public class LobbyCameraController : MonoBehaviour
    {
        [Header("Camera Presets (1-4 players)")]
        [Tooltip("Array of 4 Transform presets. Index 0 = 1 player, Index 3 = 4 players.")]
        public Transform[] cameraPositions = new Transform[4];

        [Header("Depth By Player Count")]
        [Tooltip("When enabled, camera Z depth becomes -playerCount (1=-1, 2=-2, 3=-3, 4=-4).")]
        public bool usePlayerCountDepth = false;

        [Header("Transition")]
        public float transitionSpeed = 3f;

        private int lastPlayerCount = 0;
        private Vector3 targetPosition;
        private Quaternion targetRotation;
        private bool initialized;

        private void Start()
        {
            AudioListenerEnforcer.KeepOnly(GetComponent<AudioListener>());

            if (cameraPositions.Length > 0 && cameraPositions[0] != null)
            {
                int initialPlayerCount = Mathf.Max(GetCurrentPlayerCount(), 1);
                int initialIndex = Mathf.Clamp(initialPlayerCount - 1, 0, cameraPositions.Length - 1);

                transform.position = cameraPositions[initialIndex].position;
                transform.rotation = cameraPositions[initialIndex].rotation;
                targetPosition = cameraPositions[initialIndex].position;
                targetRotation = cameraPositions[initialIndex].rotation;
                lastPlayerCount = initialPlayerCount;
                initialized = true;
                SetPreset(initialPlayerCount);
                Debug.Log($"[LobbyCameraController] Initialized at preset {initialIndex + 1} for {initialPlayerCount} player(s)");
            }
            else
            {
                Debug.LogError("[LobbyCameraController] No camera positions assigned!");
            }
        }

        private void Update()
        {
            if (!initialized) return;

            // Check for player count changes via LobbyManager
            int currentCount = GetCurrentPlayerCount();

            if (currentCount != lastPlayerCount && currentCount > 0)
            {
                Debug.Log($"[LobbyCameraController] Player count changed: {lastPlayerCount} → {currentCount}");
                lastPlayerCount = currentCount;
                SetPreset(currentCount);
            }

            // Smooth lerp to target
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * transitionSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * transitionSpeed);
        }

        int GetCurrentPlayerCount()
        {
            if (LobbyManager.Instance != null)
            {
                return LobbyManager.Instance.connectedPlayerCount;
            }

            var lobbyManager = FindObjectOfType<LobbyManager>();
            if (lobbyManager != null)
            {
                return lobbyManager.connectedPlayerCount;
            }

            return 0;
        }

        /// <summary>
        /// Set camera to the preset for the given player count (1-4).
        /// </summary>
        public void SetPreset(int playerCount)
        {
            int index = Mathf.Clamp(playerCount - 1, 0, cameraPositions.Length - 1);
            int clampedPlayerCount = Mathf.Clamp(playerCount, 1, 4);

            if (cameraPositions[index] != null)
            {
                targetPosition = cameraPositions[index].position;
                if (usePlayerCountDepth)
                {
                    targetPosition.z = -clampedPlayerCount;
                }

                targetRotation = cameraPositions[index].rotation;
                Debug.Log($"[LobbyCameraController] Camera moving to preset {index + 1} for {playerCount} player(s)");
            }
            else
            {
                Debug.LogWarning($"[LobbyCameraController] Camera position {index} is null!");
            }
        }
    }
}
