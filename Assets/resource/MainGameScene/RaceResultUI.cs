using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Mirror;
using resource.MainMenuScene;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach to a Canvas in the game scene.
/// Assign the child objects in the Inspector.
/// </summary>
public class RaceResultUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject resultPanel;
    public Text resultText;       // shows finish entries as they arrive
    public Text statusText;       // "YOU WIN!" / "DEFEAT" / "Race Over"
    public Image resultImage;     // optional image to show win/defeat art
    public Button returnButton;   // back to lobby
    public Button nextChallengeButton;
    public Text progressText;

    [Header("Result Sprites")]
    public Sprite winSprite;
    public Sprite defeatSprite;

    private List<string> finishEntries = new List<string>();
    private bool localPlayerFinished = false;
    private int localFinishPosition = -1;
    private Sprite defaultResultSprite;
    private Color defaultResultColor = Color.white;
    private bool defaultResultImageEnabled;
    private bool resultImageDefaultsCached;

    void Awake()
    {
        Debug.Log("[RaceResultUI] Awake - component initialized");

        if (resultPanel != null)
            resultPanel.SetActive(false);
        else
            Debug.LogError("[RaceResultUI] resultPanel is NOT assigned in Inspector!");

        if (returnButton != null)
        {
            returnButton.onClick.AddListener(OnReturnClicked);
            returnButton.interactable = true;
            returnButton.gameObject.SetActive(false);
        }
        else
            Debug.LogWarning("[RaceResultUI] returnButton is NOT assigned in Inspector!");

        if (nextChallengeButton != null)
        {
            nextChallengeButton.onClick.AddListener(OnNextChallengeClicked);
            nextChallengeButton.gameObject.SetActive(false);
        }

        if (resultText == null)
            Debug.LogError("[RaceResultUI] resultText is NOT assigned in Inspector!");

        if (statusText == null)
            Debug.LogError("[RaceResultUI] statusText is NOT assigned in Inspector!");

        ResolveResultImageReference();
        SetResultImage(null);
    }

    /// <summary>
    /// Called from CarPlayer TargetRpc when a player crosses the finish line.
    /// </summary>
    public void ShowPlayerFinished(string playerName, int position, bool isYou)
    {
        Debug.Log($"[RaceResultUI] ShowPlayerFinished: {playerName} at position {position}, isYou={isYou}");

        string suffix;
        switch (position)
        {
            case 1: suffix = "st"; break;
            case 2: suffix = "nd"; break;
            case 3: suffix = "rd"; break;
            default: suffix = "th"; break;
        }

        finishEntries.Add($"{position}{suffix} - {playerName}");

        if (isYou)
        {
            // This client's player just finished — show the panel NOW
            localPlayerFinished = true;
            localFinishPosition = position;

            if (resultPanel != null)
                resultPanel.SetActive(true);

            if (resultText != null)
                resultText.text = string.Join("\n", finishEntries);

            if (statusText != null)
            {
                if (position == 1)
                    statusText.text = "YOU WIN!";
                else
                    statusText.text = $"DEFEAT - You finished {position}{suffix}";
            }

            if (position == 1)
                SetResultImage(winSprite);
            else
                SetResultImage(defeatSprite);

            // Show return button immediately for the finisher
            if (returnButton != null)
                returnButton.gameObject.SetActive(true);

            // Ensure UI can be clicked when race ends.
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            UpdateOfflineButtons();
        }
        else if (localPlayerFinished)
        {
            // Local player already finished — update the result list with other finishers
            if (resultText != null)
                resultText.text = string.Join("\n", finishEntries);
        }
        // If local player hasn't finished yet, don't show anything — no spoilers
    }

    /// <summary>
    /// Called from CarPlayer TargetRpc when all players have finished.
    /// </summary>
    public void ShowRaceComplete()
    {
        Debug.Log("[RaceResultUI] ShowRaceComplete called");

        if (resultPanel != null)
            resultPanel.SetActive(true);

        // Update results list with all entries
        if (resultText != null)
            resultText.text = string.Join("\n", finishEntries);

        if (statusText != null && string.IsNullOrEmpty(statusText.text))
            statusText.text = "Race Over!";

        if (localPlayerFinished && localFinishPosition > 0)
        {
            if (localFinishPosition == 1)
                SetResultImage(winSprite);
            else
                SetResultImage(defeatSprite);
        }
        else
        {
            SetResultImage(null);
        }

        if (returnButton != null)
            returnButton.gameObject.SetActive(true);

        // Ensure UI can be clicked when race ends.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ApplyOfflineProgressIfNeeded();
        UpdateOfflineButtons();
    }

    void OnReturnClicked()
    {
        Debug.Log("[RaceResultUI] Return button clicked");
        if (returnButton != null)
            returnButton.interactable = false;

        StartCoroutine(ReturnToMenuRoutine());
    }

    System.Collections.IEnumerator ReturnToMenuRoutine()
    {
        // Use CustomNetworkManager's ReturnToMainMenu when available.
        var customManager = NetworkManager.singleton as resource.MainMenuScene.CustomNetworkManager;
        if (customManager != null)
        {
            customManager.ReturnToMainMenu();
            yield break;
        }

        Debug.LogWarning("[RaceResultUI] CustomNetworkManager not found, using fallback return flow");

        NetworkManager baseManager = NetworkManager.singleton;
        if (baseManager != null)
        {
            if (NetworkServer.active && NetworkClient.isConnected)
                baseManager.StopHost();
            else if (NetworkServer.active)
                baseManager.StopServer();
            else if (NetworkClient.isConnected || NetworkClient.active)
                baseManager.StopClient();
        }

        // Let Mirror finish shutdown before forcing scene load.
        yield return null;

        if (SceneManager.GetActiveScene().name != "MainMenuScene")
        {
            SceneManager.LoadScene("MainMenuScene");
        }
    }

    void OnNextChallengeClicked()
    {
        var netManager = NetworkManager.singleton as CustomNetworkManager;
        if (netManager != null && netManager.TryLoadNextOfflineChallenge())
        {
            if (resultPanel != null)
                resultPanel.SetActive(false);
        }
    }

    void ApplyOfflineProgressIfNeeded()
    {
        OfflineRaceConfig config = OfflineRaceConfig.Instance;
        if (config == null || !config.IsOfflineMode || config.HasAppliedResult)
            return;

        bool won = localFinishPosition == 1;
        string result = ChallengeProgressService.EnsureExists().ApplyOfflineRaceResult(config.SelectedMapIndex, won);
        config.MarkResultApplied();

        if (progressText != null)
            progressText.text = result;
    }

    void UpdateOfflineButtons()
    {
        OfflineRaceConfig config = OfflineRaceConfig.Instance;
        CustomNetworkManager netManager = NetworkManager.singleton as CustomNetworkManager;

        if (nextChallengeButton == null)
            return;

        bool canShowNext = config != null && config.IsOfflineMode && localFinishPosition == 1 && netManager != null;
        if (canShowNext)
        {
            canShowNext = ChallengeProgressService.EnsureExists().TryGetNextChallengeIndex(
                config.SelectedMapIndex,
                netManager.offlineChallengeMaps != null ? netManager.offlineChallengeMaps.Length : 0,
                out _);
        }

        nextChallengeButton.gameObject.SetActive(canShowNext);
    }

    void SetResultImage(Sprite sprite)
    {
        if (resultImage == null)
            return;

        if (sprite != null)
        {
            resultImage.sprite = sprite;
            resultImage.color = Color.white;
            resultImage.enabled = true;
            return;
        }

        if (!resultImageDefaultsCached)
        {
            resultImage.enabled = false;
            return;
        }

        // Restore initial panel art when we don't need win/defeat image yet.
        resultImage.sprite = defaultResultSprite;
        resultImage.color = defaultResultColor;
        resultImage.enabled = defaultResultImageEnabled;
    }

    void ResolveResultImageReference()
    {
        if (resultImage == null && resultPanel != null)
            resultImage = resultPanel.GetComponent<Image>();

        if (resultImage == null && resultPanel != null)
        {
            Image[] images = resultPanel.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                if (img != null)
                {
                    resultImage = img;
                    break;
                }
            }
        }

        if (resultImage == null)
        {
            Debug.LogWarning("[RaceResultUI] resultImage could not be auto-resolved. Assign it in Inspector.");
            return;
        }

        defaultResultSprite = resultImage.sprite;
        defaultResultColor = resultImage.color;
        defaultResultImageEnabled = resultImage.enabled;
        resultImageDefaultsCached = true;
    }
}
