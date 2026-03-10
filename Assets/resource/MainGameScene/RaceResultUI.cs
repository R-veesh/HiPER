using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Mirror;

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
    public Button returnButton;   // back to lobby

    private List<string> finishEntries = new List<string>();
    private bool localPlayerFinished = false;

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
            returnButton.gameObject.SetActive(false);
        }
        else
            Debug.LogWarning("[RaceResultUI] returnButton is NOT assigned in Inspector!");

        if (resultText == null)
            Debug.LogError("[RaceResultUI] resultText is NOT assigned in Inspector!");

        if (statusText == null)
            Debug.LogError("[RaceResultUI] statusText is NOT assigned in Inspector!");
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

            // Show return button immediately for the finisher
            if (returnButton != null)
                returnButton.gameObject.SetActive(true);
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

        if (returnButton != null)
            returnButton.gameObject.SetActive(true);
    }

    void OnReturnClicked()
    {
        Debug.Log("[RaceResultUI] Return button clicked");

        // Use CustomNetworkManager's ReturnToMainMenu which properly handles
        // stopping network + loading main menu scene
        var netManager = NetworkManager.singleton as resource.MainMenuScene.CustomNetworkManager;
        if (netManager != null)
        {
            netManager.ReturnToMainMenu();
        }
        else
        {
            // Fallback: stop network and load main menu
            Debug.LogWarning("[RaceResultUI] CustomNetworkManager not found, using fallback");
            if (NetworkServer.active && NetworkClient.isConnected)
                NetworkManager.singleton.StopHost();
            else if (NetworkClient.isConnected)
                NetworkManager.singleton.StopClient();
        }
    }
}
