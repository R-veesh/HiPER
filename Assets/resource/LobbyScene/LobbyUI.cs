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
            
            // DISABLE buttons until player is found
            DisableAllButtons();
            
            lobbyManager = LobbyManager.Instance;
            countdownManager = LobbyCountdown.Instance;
            
            Debug.Log($"[LobbyUI] mapSelectionPanel: {(mapSelectionPanel != null ? "ASSIGNED" : "NULL")}");
            
            if (lobbyManager == null)
            {
                Debug.LogError("[LobbyUI] LobbyManager not found!");
            }
            else
            {
                Debug.Log("[LobbyUI] Initialized, waiting for local player...");
            }

            // Show initial status
            UpdateStatusPanel(false);
            
            if (statusText != null)
            {
                statusText.text = "Loading...";
            }
        }

        void Update()
        {
            if (localLobbyPlayer == null)
            {
                FindLocalPlayer();
                
                // Log every few seconds if still waiting
                if (Time.frameCount % 120 == 0) // Every ~2 seconds at 60fps
                {
                    if (NetworkClient.localPlayer == null)
                    {
                        Debug.LogWarning("[LobbyUI] Still waiting for NetworkClient.localPlayer...");
                    }
                    else
                    {
                        var lobbyPlayer = NetworkClient.localPlayer.GetComponent<LobbyPlayer>();
                        if (lobbyPlayer == null)
                        {
                            Debug.LogWarning("[LobbyUI] NetworkClient.localPlayer exists but has no LobbyPlayer component!");
                        }
                    }
                }
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
                Debug.Log("[LobbyUI] NetworkClient.localPlayer found!");
                localLobbyPlayer = NetworkClient.localPlayer.GetComponent<LobbyPlayer>();
                
                if (localLobbyPlayer != null)
                {
                    Debug.Log($"[LobbyUI] Local player found! Name: {localLobbyPlayer.playerName}, Car: {localLobbyPlayer.selectedCarIndex}");
                    EnableAllButtons();
                    UpdateUI();
                    
                    if (statusText != null && (statusText.text == "Loading..." || statusText.text.Contains("Waiting")))
                    {
                        statusText.text = "Select your car and click READY";
                    }
                }
                else
                {
                    Debug.LogWarning("[LobbyUI] NetworkClient.localPlayer exists but missing LobbyPlayer component!");
                }
            }
        }

        void DisableAllButtons()
        {
            if (readyButton != null) readyButton.interactable = false;
            if (nextCarButton != null) nextCarButton.interactable = false;
            if (prevCarButton != null) prevCarButton.interactable = false;
            if (startButton != null) startButton.interactable = false;
            Debug.Log("[LobbyUI] Buttons disabled - waiting for player");
        }

        void EnableAllButtons()
        {
            if (readyButton != null) readyButton.interactable = true;
            if (nextCarButton != null) nextCarButton.interactable = true;
            if (prevCarButton != null) prevCarButton.interactable = true;
            // Start button will be enabled by UpdateUI based on conditions
            Debug.Log("[LobbyUI] Buttons enabled");
        }

        void SetupButtonListeners()
        {
            Debug.Log($"[LobbyUI] SetupButtonListeners - readyButton: {(readyButton != null ? "ASSIGNED" : "NULL")}");
            
            if (readyButton != null)
            {
                readyButton.onClick.AddListener(OnReadyClicked);
                Debug.Log("[LobbyUI] Ready button listener added");
            }
            else
            {
                Debug.LogError("[LobbyUI] readyButton is NULL! Assign it in Inspector.");
            }
            
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
                
                Debug.Log($"[LobbyUI] StartButton - isHost: {isHost}, allReady: {allReady}, playerCount: {lobbyManager.lobbyPlayers.Count}, showStart: {showStart}");
                
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

            if (statusText != null && lobbyManager != null)
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
                    // Show specific status message
                    statusText.text = lobbyManager.GetReadyStatusMessage();
                }
            }
        }

        void ShowStartButton(bool show)
        {
            if (startButton != null)
            {
                startButton.gameObject.SetActive(show);
                startButton.interactable = show;
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
            Debug.Log("[LobbyUI] Ready button clicked");
            Debug.Log($"[LobbyUI] localLobbyPlayer: {(localLobbyPlayer != null ? localLobbyPlayer.playerName : "NULL")}");
            Debug.Log($"[LobbyUI] lobbyManager: {(lobbyManager != null ? "ASSIGNED" : "NULL")}");
            Debug.Log($"[LobbyUI] NetworkClient.localPlayer: {(NetworkClient.localPlayer != null ? NetworkClient.localPlayer.name : "NULL")}");
            
            // Try to find player if null
            if (localLobbyPlayer == null)
            {
                FindLocalPlayer();
            }
            
            if (lobbyManager == null)
            {
                Debug.LogError("[LobbyUI] Cannot click ready - LobbyManager is null!");
                statusText.text = "ERROR: LobbyManager not found!";
                return;
            }
            
            if (localLobbyPlayer == null)
            {
                Debug.LogError("[LobbyUI] Cannot click ready - Local player not spawned yet!");
                statusText.text = "Waiting for player to spawn...";
                return;
            }
            
            lobbyManager.OnReadyClicked();
        }

        void OnStartClicked()
        {
            Debug.Log("[LobbyUI] Start button clicked");
            
            if (lobbyManager == null)
            {
                Debug.LogError("[LobbyUI] Cannot start game - LobbyManager is null!");
                return;
            }
            
            if (uiAnimator != null)
                uiAnimator.SetTrigger(startAnimTrigger);
            
            lobbyManager.OnStartClicked();
        }

        void OnNextCarClicked()
        {
            Debug.Log("[LobbyUI] Next Car button clicked");
            
            // Try to find player if null
            if (localLobbyPlayer == null)
            {
                FindLocalPlayer();
            }
            
            if (localLobbyPlayer == null)
            {
                Debug.LogError("[LobbyUI] Cannot change car - Local player not spawned yet! Waiting...");
                statusText.text = "Waiting for player to spawn...";
                return;
            }
            
            if (localLobbyPlayer.isReady)
            {
                Debug.Log("[LobbyUI] Cannot change car - Player is already ready!");
                statusText.text = "Cannot change car - Already ready!";
                return;
            }
            
            if (localLobbyPlayer.carPrefabs == null || localLobbyPlayer.carPrefabs.Length == 0)
            {
                Debug.LogError("[LobbyUI] Cannot change car - No car prefabs assigned! Check LobbyPlayer prefab.");
                statusText.text = "ERROR: No cars assigned!";
                return;
            }
            
            localLobbyPlayer.CmdNextCar();
            statusText.text = "Car changed!";
        }

        void OnPrevCarClicked()
        {
            Debug.Log("[LobbyUI] Prev Car button clicked");
            
            // Try to find player if null
            if (localLobbyPlayer == null)
            {
                FindLocalPlayer();
            }
            
            if (localLobbyPlayer == null)
            {
                Debug.LogError("[LobbyUI] Cannot change car - Local player not spawned yet! Waiting...");
                statusText.text = "Waiting for player to spawn...";
                return;
            }
            
            if (localLobbyPlayer.isReady)
            {
                Debug.Log("[LobbyUI] Cannot change car - Player is already ready!");
                statusText.text = "Cannot change car - Already ready!";
                return;
            }
            
            if (localLobbyPlayer.carPrefabs == null || localLobbyPlayer.carPrefabs.Length == 0)
            {
                Debug.LogError("[LobbyUI] Cannot change car - No car prefabs assigned! Check LobbyPlayer prefab.");
                statusText.text = "ERROR: No cars assigned!";
                return;
            }
            
            localLobbyPlayer.CmdPrevCar();
            statusText.text = "Car changed!";
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
