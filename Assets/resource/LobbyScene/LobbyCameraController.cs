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

        [Header("Transition")]
        public float transitionSpeed = 3f;

        private int lastPlayerCount = 0;
        private Vector3 targetPosition;
        private Quaternion targetRotation;
        private bool initialized;

        private void Start()
        {
            // Default to first preset
            if (cameraPositions.Length > 0 && cameraPositions[0] != null)
            {
                transform.position = cameraPositions[0].position;
                transform.rotation = cameraPositions[0].rotation;
                targetPosition = cameraPositions[0].position;
                targetRotation = cameraPositions[0].rotation;
                initialized = true;
                Debug.Log("[LobbyCameraController] Initialized at position 1");
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
            int currentCount = 0;
            if (LobbyManager.Instance != null)
            {
                currentCount = LobbyManager.Instance.connectedPlayerCount;
            }
            else
            {
                // Try to find it if Instance is null (can happen on client)
                var lobbyManager = FindObjectOfType<LobbyManager>();
                if (lobbyManager != null)
                {
                    currentCount = lobbyManager.connectedPlayerCount;
                }
            }

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

        /// <summary>
        /// Set camera to the preset for the given player count (1-4).
        /// </summary>
        public void SetPreset(int playerCount)
        {
            int index = Mathf.Clamp(playerCount - 1, 0, cameraPositions.Length - 1);

            if (cameraPositions[index] != null)
            {
                targetPosition = cameraPositions[index].position;
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
