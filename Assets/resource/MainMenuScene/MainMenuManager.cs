using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using resource.LobbyScene;

namespace resource.MainMenuScene
{
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Root Navigation")]
        public GameObject modeSelectPanel;
        public GameObject lanPanel;
        public GameObject offlinePanel;
        public Button offlineGameButton;
        public Button lanGameButton;
        public Button quitButton;
        public Button backFromLanButton;
        public Button backFromOfflineButton;
        public Button storeButton;
        public Button profileButton;

        [Header("LAN Controls")]
        public Button hostButton;
        public Button joinButton;
        public InputField addressInputField;
        public Text connectionStatusText;
        public string networkAddress = "127.0.0.1";

        [Header("Offline Controls")]
        public Button playOfflineButton;
        public Button nextMapButton;
        public Button prevMapButton;
        public Button nextCarButton;
        public Button prevCarButton;

        [Header("Offline Text")]
        public TextMeshProUGUI offlineMapNameText;
        public TextMeshProUGUI offlineMapDescriptionText;
        public TextMeshProUGUI offlineDifficultyText;
        public TextMeshProUGUI offlineProgressText;
        public TextMeshProUGUI offlineCarNameText;
        public TextMeshProUGUI offlineStatusText;
        public TextMeshProUGUI ownedCarsText;

        [Header("Offline Visuals")]
        public Image offlineMapPreviewImage;
        public Image offlineCarPreviewImage;
        public Sprite[] offlineCarPreviewSprites;
        public string[] offlineCarNames;

        [Header("Store/Profile")]
        public string storeUrl = "http://localhost:3000/store";
        public ProfileSummaryPanel profileSummaryPanel;
        public ProfileUI profileEditPanel;

        CustomNetworkManager networkManager;
        ChallengeProgressService progressService;
        int selectedOfflineMapIndex;
        int selectedOfflineCarIndex;

        void Start()
        {
            UserSession.EnsureExists();
            AuthManager.EnsureExists();
            ApiClient.EnsureExists();

            progressService = ChallengeProgressService.EnsureExists();
            OfflineRaceConfig.EnsureExists();

            networkManager = NetworkManager.singleton as CustomNetworkManager;
            SetupButtonListeners();
            SetupNetworkManager();
            SetupAddressInput();

            LoadOfflineSelections();
            ShowModeSelect();
            RefreshOfflineUI();
        }

        void SetupButtonListeners()
        {
            AddListener(offlineGameButton, OnOfflineMenuClicked);
            AddListener(lanGameButton, OnLanMenuClicked);
            AddListener(quitButton, OnQuitClicked);
            AddListener(backFromLanButton, ShowModeSelect);
            AddListener(backFromOfflineButton, ShowModeSelect);
            AddListener(storeButton, OnStoreClicked);
            AddListener(profileButton, OnProfileClicked);

            AddListener(hostButton, OnHostClicked);
            AddListener(joinButton, OnJoinClicked);

            AddListener(playOfflineButton, OnPlayOfflineClicked);
            AddListener(nextMapButton, OnNextMapClicked);
            AddListener(prevMapButton, OnPrevMapClicked);
            AddListener(nextCarButton, OnNextCarClicked);
            AddListener(prevCarButton, OnPrevCarClicked);
        }

        void SetupAddressInput()
        {
            if (addressInputField == null)
                return;

            addressInputField.text = networkAddress;
            addressInputField.onEndEdit.AddListener(OnAddressChanged);
            UpdateConnectionStatus(string.Empty);
        }

        void SetupNetworkManager()
        {
            if (networkManager == null)
                networkManager = NetworkManager.singleton as CustomNetworkManager;

            if (networkManager != null)
                networkManager.networkAddress = networkAddress;
        }

        void ShowModeSelect()
        {
            SetPanelState(modeSelectPanel, true);
            SetPanelState(lanPanel, false);
            SetPanelState(offlinePanel, false);
            RefreshOfflineUI();
        }

        void OnLanMenuClicked()
        {
            SetPanelState(modeSelectPanel, false);
            SetPanelState(lanPanel, true);
            SetPanelState(offlinePanel, false);
        }

        void OnOfflineMenuClicked()
        {
            SetPanelState(modeSelectPanel, false);
            SetPanelState(lanPanel, false);
            SetPanelState(offlinePanel, true);
            RefreshOfflineUI();
        }

        void OnAddressChanged(string newAddress)
        {
            if (string.IsNullOrWhiteSpace(newAddress))
                return;

            networkAddress = newAddress.Trim();
            if (networkManager != null)
                networkManager.networkAddress = networkAddress;
        }

        void OnHostClicked()
        {
            if (!NetworkServer.active && !NetworkClient.active)
            {
                UpdateConnectionStatus("Starting host...");
                NetworkManager.singleton.StartHost();
            }
            else
            {
                UpdateConnectionStatus("Already connected");
            }
        }

        void OnJoinClicked()
        {
            if (NetworkClient.active)
            {
                UpdateConnectionStatus("Already connected");
                return;
            }

            string connectAddress = networkAddress;
            if (connectAddress.Equals("localhost", System.StringComparison.OrdinalIgnoreCase))
                connectAddress = "127.0.0.1";

            UpdateConnectionStatus($"Connecting to {connectAddress}...");
            NetworkManager.singleton.networkAddress = connectAddress;
            NetworkManager.singleton.StartClient();
            StartCoroutine(ConnectionTimeoutCheck());
        }

        System.Collections.IEnumerator ConnectionTimeoutCheck()
        {
            float timeout = 10f;
            float elapsed = 0f;

            while (elapsed < timeout && NetworkClient.active && !NetworkClient.isConnected)
            {
                elapsed += 0.5f;
                yield return new WaitForSeconds(0.5f);
            }

            if (!NetworkClient.isConnected && NetworkClient.active)
            {
                UpdateConnectionStatus("Connection failed - is server running?");
                NetworkManager.singleton.StopClient();
            }
        }

        void OnNextMapClicked()
        {
            MapData[] maps = GetOfflineMaps();
            if (maps == null || maps.Length == 0)
                return;

            selectedOfflineMapIndex = (selectedOfflineMapIndex + 1) % maps.Length;
            RefreshOfflineUI();
        }

        void OnPrevMapClicked()
        {
            MapData[] maps = GetOfflineMaps();
            if (maps == null || maps.Length == 0)
                return;

            selectedOfflineMapIndex--;
            if (selectedOfflineMapIndex < 0)
                selectedOfflineMapIndex = maps.Length - 1;

            RefreshOfflineUI();
        }

        void OnNextCarClicked()
        {
            int totalCars = GetTotalCarCount();
            if (totalCars == 0)
                return;

            selectedOfflineCarIndex = progressService.GetNextOwnedCarIndex(selectedOfflineCarIndex, totalCars);
            progressService.SetSelectedOfflineCarIndex(selectedOfflineCarIndex);
            RefreshOfflineUI();
        }

        void OnPrevCarClicked()
        {
            int totalCars = GetTotalCarCount();
            if (totalCars == 0)
                return;

            selectedOfflineCarIndex = progressService.GetPreviousOwnedCarIndex(selectedOfflineCarIndex, totalCars);
            progressService.SetSelectedOfflineCarIndex(selectedOfflineCarIndex);
            RefreshOfflineUI();
        }

        void OnPlayOfflineClicked()
        {
            MapData selectedMap = GetSelectedOfflineMap();
            if (selectedMap == null)
            {
                SetOfflineStatus("No offline map assigned");
                return;
            }

            if (!progressService.IsMapUnlocked(selectedOfflineMapIndex))
            {
                SetOfflineStatus("This challenge is locked");
                return;
            }

            if (!progressService.IsCarOwned(selectedOfflineCarIndex))
            {
                SetOfflineStatus("This car is locked in store");
                return;
            }

            if (networkManager == null)
            {
                networkManager = NetworkManager.singleton as CustomNetworkManager;
                if (networkManager == null)
                {
                    SetOfflineStatus("Network manager missing");
                    return;
                }
            }

            SetOfflineStatus($"Loading {selectedMap.mapName}...");
            networkManager.StartOfflineGame(selectedMap, selectedOfflineCarIndex);
        }

        void OnStoreClicked()
        {
            if (string.IsNullOrEmpty(storeUrl))
            {
                SetOfflineStatus("Store URL not configured");
                return;
            }

            Application.OpenURL(storeUrl);
            SetOfflineStatus("Store opened in browser");
        }

        void OnProfileClicked()
        {
            if (profileSummaryPanel != null)
            {
                profileSummaryPanel.Show();
            }
            else if (profileEditPanel != null)
            {
                profileEditPanel.Show();
            }
        }

        void RefreshOfflineUI()
        {
            MapData selectedMap = GetSelectedOfflineMap();
            if (selectedMap == null)
            {
                SetOfflineStatus("Assign offline challenge maps in CustomNetworkManager");
                return;
            }

            bool isUnlocked = progressService.IsMapUnlocked(selectedOfflineMapIndex);

            if (offlineMapNameText != null)
                offlineMapNameText.text = selectedMap.mapName;

            if (offlineMapDescriptionText != null)
                offlineMapDescriptionText.text = selectedMap.mapDescription;

            if (offlineDifficultyText != null)
                offlineDifficultyText.text = $"Difficulty: {selectedMap.difficulty} | Laps: {selectedMap.laps}";

            if (offlineMapPreviewImage != null)
                offlineMapPreviewImage.sprite = selectedMap.mapPreview;

            if (offlineProgressText != null)
                offlineProgressText.text = isUnlocked ? "Unlocked" : "Locked - win previous challenge";

            if (offlineCarNameText != null)
                offlineCarNameText.text = GetCarName(selectedOfflineCarIndex);

            if (offlineCarPreviewImage != null && offlineCarPreviewSprites != null && selectedOfflineCarIndex < offlineCarPreviewSprites.Length)
                offlineCarPreviewImage.sprite = offlineCarPreviewSprites[selectedOfflineCarIndex];

            if (ownedCarsText != null && UserSession.Instance != null)
                ownedCarsText.text = $"Owned Cars: {UserSession.Instance.OwnedCarIndices.Length}";

            if (playOfflineButton != null)
                playOfflineButton.interactable = isUnlocked;

            SetOfflineStatus($"Level {UserSession.Instance?.PlayerLevel ?? 1} | Wins {UserSession.Instance?.MatchesWon ?? 0}");
        }

        void LoadOfflineSelections()
        {
            selectedOfflineCarIndex = progressService.GetSelectedOfflineCarIndex();
            if (!progressService.IsCarOwned(selectedOfflineCarIndex))
                selectedOfflineCarIndex = 0;

            selectedOfflineMapIndex = 0;
            MapData[] maps = GetOfflineMaps();
            if (maps == null || maps.Length == 0)
                return;

            int preferredMap = Mathf.Clamp(progressService.GetProgress().currentChallengeIndex, 0, maps.Length - 1);
            selectedOfflineMapIndex = progressService.IsMapUnlocked(preferredMap) ? preferredMap : 0;
        }

        MapData[] GetOfflineMaps()
        {
            if (networkManager == null)
                networkManager = NetworkManager.singleton as CustomNetworkManager;

            return networkManager != null ? networkManager.offlineChallengeMaps : null;
        }

        MapData GetSelectedOfflineMap()
        {
            MapData[] maps = GetOfflineMaps();
            if (maps == null || maps.Length == 0)
                return null;

            selectedOfflineMapIndex = Mathf.Clamp(selectedOfflineMapIndex, 0, maps.Length - 1);
            return maps[selectedOfflineMapIndex];
        }

        int GetTotalCarCount()
        {
            int previewCount = offlineCarPreviewSprites != null ? offlineCarPreviewSprites.Length : 0;
            int nameCount = offlineCarNames != null ? offlineCarNames.Length : 0;
            return Mathf.Max(previewCount, nameCount);
        }

        string GetCarName(int index)
        {
            if (offlineCarNames != null && index >= 0 && index < offlineCarNames.Length)
                return offlineCarNames[index];

            return $"Car {index + 1}";
        }

        void SetOfflineStatus(string status)
        {
            if (offlineStatusText != null)
                offlineStatusText.text = status;
        }

        void UpdateConnectionStatus(string status)
        {
            if (connectionStatusText != null)
                connectionStatusText.text = status;
        }

        void SetPanelState(GameObject panel, bool isVisible)
        {
            if (panel != null)
                panel.SetActive(isVisible);
        }

        void OnQuitClicked()
        {
            Application.Quit();
        }

        static void AddListener(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
                button.onClick.AddListener(action);
        }
    }
}
