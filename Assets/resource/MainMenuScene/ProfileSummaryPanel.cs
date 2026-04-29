using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace resource.MainMenuScene
{
    public class ProfileSummaryPanel : MonoBehaviour
    {
        [Header("Panel")]
        public GameObject panel;

        [Header("Text")]
        public TextMeshProUGUI displayNameText;
        public TextMeshProUGUI emailText;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI matchStatsText;
        public TextMeshProUGUI coinText;
        public TextMeshProUGUI ownedCarsText;
        public TextMeshProUGUI statusText;

        [Header("Buttons")]
        public Button editButton;
        public Button closeButton;
        public Button refreshButton;
        public ProfileUI profileEditPanel;

        void Awake()
        {
            ResolveReferences();
            WireButtons();
        }

        void Start()
        {
            ResolveReferences();
        }

        public void Show()
        {
            ResolveReferences();
            WireButtons();

            if (panel != null)
                panel.SetActive(true);

            Refresh();
        }

        public void Hide()
        {
            if (panel != null)
                panel.SetActive(false);
        }

        public void Refresh()
        {
            UserSession session = UserSession.EnsureExists();
            AuthManager authManager = AuthManager.EnsureExists();
            ChallengeProgressService.EnsureExists();

            if (session.IsLoggedIn)
            {
                authManager.RefreshBalance(UpdateView);
            }
            else
            {
                UpdateView(session.CoinBalance);
            }
        }

        void UpdateView(int _)
        {
            UserSession session = UserSession.EnsureExists();

            if (displayNameText != null)
                displayNameText.text = string.IsNullOrWhiteSpace(session.DisplayName) ? "Guest Player" : session.DisplayName;

            if (emailText != null)
                emailText.text = string.IsNullOrWhiteSpace(session.Email) ? "No email" : session.Email;

            if (levelText != null)
                levelText.text = $"Level {session.PlayerLevel}";

            if (matchStatsText != null)
                matchStatsText.text = $"Matches {session.TotalMatches} | Wins {session.MatchesWon}";

            if (coinText != null)
                coinText.text = $"Coins {session.CoinBalance}";

            if (ownedCarsText != null)
                ownedCarsText.text = $"Owned Cars {GetOwnedCarCount(session)}";

            if (statusText != null)
                statusText.text = session.IsLoggedIn ? "Profile ready" : "Logged out";
        }

        void OpenEdit()
        {
            ResolveReferences();

            if (profileEditPanel != null)
            {
                Hide();
                profileEditPanel.Show();
            }
            else if (statusText != null)
            {
                statusText.text = "Profile edit panel not assigned";
            }
        }

        void ResolveReferences()
        {
            if (panel == null)
                panel = gameObject;

            if (profileEditPanel == null)
            {
#if UNITY_6000_0_OR_NEWER
                profileEditPanel = FindFirstObjectByType<ProfileUI>(FindObjectsInactive.Include);
#else
                profileEditPanel = FindObjectOfType<ProfileUI>(true);
#endif
            }

            if (profileEditPanel == null)
            {
#if UNITY_6000_0_OR_NEWER
                MainMenuManager mainMenuManager = FindFirstObjectByType<MainMenuManager>(FindObjectsInactive.Include);
#else
                MainMenuManager mainMenuManager = FindObjectOfType<MainMenuManager>(true);
#endif
                if (mainMenuManager != null)
                    profileEditPanel = mainMenuManager.profileEditPanel;
            }
        }

        void WireButtons()
        {
            if (editButton != null)
            {
                editButton.onClick.RemoveListener(OpenEdit);
                editButton.onClick.AddListener(OpenEdit);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Hide);
                closeButton.onClick.AddListener(Hide);
            }

            if (refreshButton != null)
            {
                refreshButton.onClick.RemoveListener(Refresh);
                refreshButton.onClick.AddListener(Refresh);
            }
        }

        static int GetOwnedCarCount(UserSession session)
        {
            return session.OwnedCarIndices != null ? session.OwnedCarIndices.Length : 0;
        }
    }
}
