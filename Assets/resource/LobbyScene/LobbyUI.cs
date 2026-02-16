using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using System.Collections.Generic;
using resource.MainMenuScene;

namespace resource.LobbyScene
{
    public class LobbyUI : MonoBehaviour
    {
        [Header("Player Plates")]
        public PlayerPlateUI[] playerPlates; // 4 plates for 4 players
        public Transform platesContainer;

        [Header("Map Selection")]
        public MapSelectionPanel mapSelectionPanel;

        [Header("Countdown")]
        public CountdownDisplay countdownDisplay;

        [Header("Control Buttons")]
        public Button readyButton;
        public Button startButton;
        public Button nextCarButton;
        public Button prevCarButton;
        public Button leaveButton;

        [Header("UI Text Elements")]
        public TextMeshProUGUI readyButtonText;
        public TextMeshProUGUI statusText;
        public TextMeshProUGUI carSelectionText;
        public TextMeshProUGUI playerCountText;
        public TextMeshProUGUI roomCodeText;

        [Header("Status Panel")]
        public GameObject statusPanel;
        public Image statusPanelBackground;
        public Color allReadyColor = new Color(0.2f, 0.8f, 0.2f, 0.8f);
        public Color waitingColor = new Color(0.8f, 0.4f, 0.2f, 0.8f);

        [Header("Car Selection")]
        public string[] carNames;
        public Sprite[] carPreviewSprites;

        [Header("Animation")]
        public Animator uiAnimator;
        public string readyAnimTrigger = "OnReady";
        public string startAnimTrigger = "OnStart";

        private LobbyPlayer localLobbyPlayer;
        private LobbyManager lobbyManager;
        private LobbyCountdown countdownManager;
        private bool wasReady = false;

        void Start()
        {
            SetupButtonListeners();
            InitializePlayerPlates();
            HideStartButton();
            
            lobbyManager = LobbyManager.Instance;
            countdownManager = LobbyCountdown.Instance;
            
            if (lobbyManager == null)
            {
                Debug.LogError("LobbyManager not found!");
            }

            // Show initial status
            UpdateStatusPanel(false);
        }

        void Update()
        {
            if (localLobbyPlayer == null)
            {
                FindLocalPlayer();
            }
            else
            {
                UpdateUI();
                UpdatePlayerPlates();
            }
        }

        void InitializePlayerPlates()
        {
            if (playerPlates != null)
            {
                for (int i = 0; i < playerPlates.Length; i++)
                {
                    if (playerPlates[i] != null)
                    {
                        playerPlates[i].SetPlateIndex(i);
                        playerPlates[i].ClearPlayer();
                    }
                }
            }
        }

        void FindLocalPlayer()
        {
            if (NetworkClient.localPlayer != null)
            {
                localLobbyPlayer = NetworkClient.localPlayer.GetComponent<LobbyPlayer>();
                if (localLobbyPlayer != null)
                {
                    UpdateUI();
                }
            }
        }

        void SetupButtonListeners()
        {
            if (readyButton != null)
                readyButton.onClick.AddListener(OnReadyClicked);
            
            if (startButton != null)
                startButton.onClick.AddListener(OnStartClicked);
            
            if (nextCarButton != null)
                nextCarButton.onClick.AddListener(OnNextCarClicked);
            
            if (prevCarButton != null)
                prevCarButton.onClick.AddListener(OnPrevCarClicked);
            
            if (leaveButton != null)
                leaveButton.onClick.AddListener(OnLeaveClicked);
        }

        void UpdateUI()
        {
            if (localLobbyPlayer == null) return;

            // Update ready button
            if (readyButtonText != null)
            {
                readyButtonText.text = localLobbyPlayer.isReady ? "CANCEL READY" : "READY";
            }
            
            // Trigger animation when ready state changes
            if (localLobbyPlayer.isReady != wasReady)
            {
                wasReady = localLobbyPlayer.isReady;
                if (uiAnimator != null)
                    uiAnimator.SetTrigger(readyAnimTrigger);
            }
            
            // Update car selection text
            if (carSelectionText != null && carNames != null && carNames.Length > localLobbyPlayer.selectedCarIndex)
            {
                carSelectionText.text = $"Selected: {carNames[localLobbyPlayer.selectedCarIndex]}";
            }

            // Update car selection buttons
            bool canSelectCar = !localLobbyPlayer.isReady;
            if (nextCarButton != null)
                nextCarButton.interactable = canSelectCar;
            if (prevCarButton != null)
                prevCarButton.interactable = canSelectCar;

            // Update player count
            if (playerCountText != null && lobbyManager != null)
            {
                int readyCount = lobbyManager.GetReadyPlayerCount();
                int totalCount = lobbyManager.connectedPlayerCount;
                playerCountText.text = $"Players: {readyCount}/{totalCount} Ready";
            }

            // Update status panel and start button
            if (lobbyManager != null)
            {
                bool allReady = lobbyManager.AllPlayersReady();
                UpdateStatusPanel(allReady);
                
                // START button only for HOST
                bool isHost = NetworkServer.active && NetworkClient.active;
                bool showStart = isHost && allReady && 
                    (countdownManager == null || !countdownManager.isCountingDown);
                
                ShowStartButton(showStart);
            }
        }

        void UpdatePlayerPlates()
        {
            if (lobbyManager == null || playerPlates == null) return;

            var players = lobbyManager.GetLobbyPlayers();
            HashSet<int> occupiedPlates = new HashSet<int>();

            // Update occupied plates
            foreach (var player in players)
            {
                if (player != null && player.plateIndex >= 0 && player.plateIndex < playerPlates.Length)
                {
                    occupiedPlates.Add(player.plateIndex);
                    
                    bool isLocal = (player == localLobbyPlayer);
                    string carName = (carNames != null && carNames.Length > player.selectedCarIndex) 
                        ? carNames[player.selectedCarIndex] 
                        : "Unknown Car";

                    playerPlates[player.plateIndex].SetPlayerInfo(
                        player.playerName,
                        carName,
                        player.isReady,
                        isLocal
                    );

                    // Set car preview if available
                    if (carPreviewSprites != null && carPreviewSprites.Length > player.selectedCarIndex)
                    {
                        playerPlates[player.plateIndex].SetCarPreview(carPreviewSprites[player.selectedCarIndex]);
                    }
                }
            }

            // Clear unoccupied plates
            for (int i = 0; i < playerPlates.Length; i++)
            {
                if (!occupiedPlates.Contains(i))
                {
                    playerPlates[i].ClearPlayer();
                }
            }
        }

        void UpdateStatusPanel(bool allReady)
        {
            if (statusPanelBackground != null)
            {
                statusPanelBackground.color = allReady ? allReadyColor : waitingColor;
            }

            if (statusText != null)
            {
                if (countdownManager != null && countdownManager.isCountingDown)
                {
                    statusText.text = "Game Starting Soon...";
                }
                else if (allReady)
                {
                    statusText.text = "All Players Ready!";
                    if (NetworkServer.active)
                        statusText.text += " - Host can start";
                }
                else
                {
                    statusText.text = "Waiting for players...";
                }
            }
        }

        void ShowStartButton(bool show)
        {
            if (startButton != null)
            {
                startButton.gameObject.SetActive(show);
            }
        }

        void HideStartButton()
        {
            if (startButton != null)
            {
                startButton.gameObject.SetActive(false);
            }
        }

        void OnReadyClicked()
        {
            lobbyManager?.OnReadyClicked();
        }

        void OnStartClicked()
        {
            if (uiAnimator != null)
                uiAnimator.SetTrigger(startAnimTrigger);
            
            lobbyManager?.OnStartClicked();
        }

        void OnNextCarClicked()
        {
            localLobbyPlayer?.CmdNextCar();
        }

        void OnPrevCarClicked()
        {
            localLobbyPlayer?.CmdPrevCar();
        }

        void OnLeaveClicked()
        {
            // Return to main menu
            var networkManager = NetworkManager.singleton as CustomNetworkManager;
            if (networkManager != null)
            {
                networkManager.ReturnToMainMenu();
            }
        }

        public void OnPlayerDisconnected()
        {
            localLobbyPlayer = null;
            HideStartButton();
        }
    }
}
