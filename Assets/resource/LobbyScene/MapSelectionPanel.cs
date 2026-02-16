using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

namespace resource.LobbyScene
{
    public class MapSelectionPanel : MonoBehaviour
    {
        [Header("UI Elements")]
        public Image mapPreviewImage;
        public TextMeshProUGUI mapNameText;
        public TextMeshProUGUI mapDescriptionText;
        public TextMeshProUGUI difficultyText;
        public TextMeshProUGUI voteCountText;
        public TextMeshProUGUI lapsText;
        public Button previousMapButton;
        public Button nextMapButton;
        public Button voteButton;
        public GameObject votedIndicator;

        [Header("Difficulty Colors")]
        public Color easyColor = new Color(0.2f, 0.8f, 0.2f, 1f);
        public Color mediumColor = new Color(1f, 0.8f, 0.2f, 1f);
        public Color hardColor = new Color(0.9f, 0.2f, 0.2f, 1f);

        [Header("Animation")]
        public Animator panelAnimator;
        public string mapChangeAnimTrigger = "OnMapChange";
        public string voteAnimTrigger = "OnVote";

        private int currentMapIndex = 0;
        private LobbyManager lobbyManager;
        private LobbyPlayer localPlayer;

        void Start()
        {
            lobbyManager = LobbyManager.Instance;
            SetupButtonListeners();
        }

        void Update()
        {
            if (localPlayer == null)
            {
                FindLocalPlayer();
            }
            else
            {
                UpdateUI();
            }
        }

        void FindLocalPlayer()
        {
            if (NetworkClient.localPlayer != null)
            {
                localPlayer = NetworkClient.localPlayer.GetComponent<LobbyPlayer>();
            }
        }

        void SetupButtonListeners()
        {
            if (previousMapButton != null)
                previousMapButton.onClick.AddListener(OnPreviousMapClicked);
            
            if (nextMapButton != null)
                nextMapButton.onClick.AddListener(OnNextMapClicked);
            
            if (voteButton != null)
                voteButton.onClick.AddListener(OnVoteClicked);
        }

        void UpdateUI()
        {
            if (lobbyManager == null || lobbyManager.availableMaps == null || lobbyManager.availableMaps.Length == 0)
            {
                // Disable UI if no maps available
                if (previousMapButton != null) previousMapButton.interactable = false;
                if (nextMapButton != null) nextMapButton.interactable = false;
                if (voteButton != null) voteButton.interactable = false;
                return;
            }

            // Clamp current map index
            currentMapIndex = Mathf.Clamp(currentMapIndex, 0, lobbyManager.availableMaps.Length - 1);
            
            MapData currentMap = lobbyManager.availableMaps[currentMapIndex];
            if (currentMap == null) return;

            // Update map info
            if (mapNameText != null)
                mapNameText.text = currentMap.mapName;

            if (mapDescriptionText != null)
                mapDescriptionText.text = currentMap.mapDescription;

            if (mapPreviewImage != null && currentMap.mapPreview != null)
                mapPreviewImage.sprite = currentMap.mapPreview;

            // Update difficulty
            if (difficultyText != null)
            {
                difficultyText.text = $"Difficulty: {currentMap.difficulty}";
                switch (currentMap.difficulty)
                {
                    case MapData.DifficultyLevel.Easy:
                        difficultyText.color = easyColor;
                        break;
                    case MapData.DifficultyLevel.Medium:
                        difficultyText.color = mediumColor;
                        break;
                    case MapData.DifficultyLevel.Hard:
                        difficultyText.color = hardColor;
                        break;
                }
            }

            // Update laps
            if (lapsText != null)
                lapsText.text = $"Laps: {currentMap.laps}";

            // Update vote count
            if (voteCountText != null)
            {
                int votes = GetMapVotes(currentMapIndex);
                voteCountText.text = $"Votes: {votes}/{lobbyManager.connectedPlayerCount}";
            }

            // Update voted indicator
            if (votedIndicator != null && localPlayer != null)
            {
                votedIndicator.SetActive(localPlayer.selectedMapIndex == currentMapIndex);
            }

            // Disable buttons if ready
            if (localPlayer != null)
            {
                bool canInteract = !localPlayer.isReady;
                if (previousMapButton != null)
                    previousMapButton.interactable = canInteract;
                if (nextMapButton != null)
                    nextMapButton.interactable = canInteract;
                if (voteButton != null)
                    voteButton.interactable = canInteract && localPlayer.selectedMapIndex != currentMapIndex;
            }
        }

        int GetMapVotes(int mapIndex)
        {
            int votes = 0;
            var players = lobbyManager.GetLobbyPlayers();
            foreach (var player in players)
            {
                if (player.selectedMapIndex == mapIndex)
                    votes++;
            }
            return votes;
        }

        void OnPreviousMapClicked()
        {
            if (lobbyManager == null || lobbyManager.availableMaps == null) return;
            
            currentMapIndex--;
            if (currentMapIndex < 0)
                currentMapIndex = lobbyManager.availableMaps.Length - 1;

            OnMapChanged();
        }

        void OnNextMapClicked()
        {
            if (lobbyManager == null || lobbyManager.availableMaps == null) return;
            
            currentMapIndex++;
            if (currentMapIndex >= lobbyManager.availableMaps.Length)
                currentMapIndex = 0;

            OnMapChanged();
        }

        void OnMapChanged()
        {
            if (panelAnimator != null)
                panelAnimator.SetTrigger(mapChangeAnimTrigger);
        }

        void OnVoteClicked()
        {
            if (localPlayer != null)
            {
                localPlayer.CmdVoteForMap(currentMapIndex);
                
                if (panelAnimator != null)
                    panelAnimator.SetTrigger(voteAnimTrigger);
            }
        }

        public void SetCurrentMapIndex(int index)
        {
            if (lobbyManager != null && lobbyManager.availableMaps != null)
            {
                currentMapIndex = Mathf.Clamp(index, 0, lobbyManager.availableMaps.Length - 1);
                OnMapChanged();
            }
        }
    }
}
