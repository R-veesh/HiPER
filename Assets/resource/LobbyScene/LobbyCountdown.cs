using UnityEngine;
using Mirror;
using TMPro;

namespace resource.LobbyScene
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class LobbyCountdown : NetworkBehaviour
    {
        public static LobbyCountdown Instance;

        [Header("Settings")]
        [SerializeField] private float countdownDuration = 5f;
        [SerializeField] private bool autoStartEnabled = true;

        [Header("Events")]
        [SyncVar] public bool isCountingDown = false;
        [SyncVar] public float remainingTime = 5f;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        void Update()
        {
            if (!isServer || !isCountingDown) return;

            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0)
            {
                remainingTime = 0;
                StopCountdown();
                RpcStartGame();
                ActuallyStartGame();
            }
        }

        [Server]
        public void StartCountdown()
        {
            if (isCountingDown) return;
            
            isCountingDown = true;
            remainingTime = countdownDuration;
            RpcOnCountdownStarted(countdownDuration);
            Debug.Log("Countdown started!");
        }

        [Server]
        public void StopCountdown()
        {
            isCountingDown = false;
            remainingTime = 0;
            RpcOnCountdownStopped();
            Debug.Log("Countdown stopped!");
        }

        [ClientRpc]
        void RpcOnCountdownStarted(float duration)
        {
            Debug.Log($"Countdown started: {duration} seconds");
            // UI will subscribe to this
        }

        [ClientRpc]
        void RpcOnCountdownStopped()
        {
            Debug.Log("Countdown stopped!");
            // UI will subscribe to this
        }

        [ClientRpc]
        void RpcStartGame()
        {
            Debug.Log("GO! Starting game...");
            // UI animation for GO!
        }

        [Server]
        void ActuallyStartGame()
        {
            Debug.Log("[LobbyCountdown] ActuallyStartGame called - initiating game start");
            
            // First save player data before anything gets destroyed
            var playerDataContainer = FindObjectOfType<resource.MainMenuScene.PlayerDataContainer>();
            if (playerDataContainer != null)
            {
                playerDataContainer.SaveAllPlayerData();
            }
            
            // Then trigger game start through LobbyManager
            if (LobbyManager.Instance != null)
            {
                LobbyManager.Instance.StartGame();
            }
            else
            {
                // Fallback: directly trigger scene change if LobbyManager is already destroyed
                Debug.LogWarning("[LobbyCountdown] LobbyManager instance is null, attempting direct scene change");
                var networkManager = NetworkManager.singleton as resource.MainMenuScene.CustomNetworkManager;
                if (networkManager != null)
                {
                    networkManager.LoadGameScene();
                }
                else
                {
                    Debug.LogError("[LobbyCountdown] Cannot start game - both LobbyManager and CustomNetworkManager are null!");
                }
            }
        }

        [Server]
        public void CheckAndStartCountdown(LobbyManager lobbyManager)
        {
            if (!autoStartEnabled) return;
            
            int playerCount = lobbyManager.connectedPlayerCount;
            int minPlayers = lobbyManager.minPlayers;
            
            // Only start if we have minimum players, all are ready, and countdown isn't running
            if (playerCount >= minPlayers && lobbyManager.AllPlayersReady() && !isCountingDown)
            {
                StartCountdown();
            }
            else if ((!lobbyManager.AllPlayersReady() || playerCount < minPlayers) && isCountingDown)
            {
                StopCountdown();
            }
        }

        public void OnPlayerReadinessChanged(LobbyManager lobbyManager)
        {
            if (!isServer) return;
            
            if (autoStartEnabled)
            {
                CheckAndStartCountdown(lobbyManager);
            }
        }
    }
}
