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
            returnButton.onClick.AddListener(OnReturnClicked);
        else
            Debug.LogWarning("[RaceResultUI] returnButton is NOT assigned in Inspector!");

        if (resultText == null)
            Debug.LogError("[RaceResultUI] resultText is NOT assigned in Inspector!");

        if (statusText == null)
            Debug.LogError("[RaceResultUI] statusText is NOT assigned in Inspector!");
    }

    /// <summary>
    /// Called from RaceManager RPC when a player crosses the finish line.
    /// </summary>
    public void ShowPlayerFinished(string playerName, int position)
    {
        Debug.Log($"[RaceResultUI] ShowPlayerFinished: {playerName} at position {position}");

        if (resultPanel != null)
            resultPanel.SetActive(true);

        string suffix;
        switch (position)
        {
            case 1: suffix = "st"; break;
            case 2: suffix = "nd"; break;
            case 3: suffix = "rd"; break;
            default: suffix = "th"; break;
        }

        finishEntries.Add($"{position}{suffix} - {playerName}");

        if (resultText != null)
            resultText.text = string.Join("\n", finishEntries);

        // Check if this is the local player
        bool isLocalPlayer = IsLocalPlayerName(playerName);

        if (statusText != null)
        {
            if (isLocalPlayer)
            {
                localPlayerFinished = true;
                if (position == 1)
                    statusText.text = "YOU WIN!";
                else
                    statusText.text = $"DEFEAT - You finished {position}{suffix}";
            }
            else if (!localPlayerFinished)
            {
                // Another player finished first and local player hasn't yet
                if (position == 1)
                    statusText.text = $"{playerName} Wins!";
            }
        }
    }

    /// <summary>
    /// Check if the given player name matches the local player's car.
    /// </summary>
    private bool IsLocalPlayerName(string playerName)
    {
        if (NetworkClient.localPlayer == null) return false;

        // The car name format from GameSpawnManager is "Car_{playerName}_{connectionId}"
        string carName = NetworkClient.localPlayer.gameObject.name;
        return carName.Contains(playerName);
    }

    /// <summary>
    /// Called from RaceManager RPC when all players have finished.
    /// </summary>
    public void ShowRaceComplete()
    {
        Debug.Log("[RaceResultUI] ShowRaceComplete called");

        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (statusText != null && string.IsNullOrEmpty(statusText.text))
            statusText.text = "Race Over!";

        if (returnButton != null)
            returnButton.gameObject.SetActive(true);
    }

    void OnReturnClicked()
    {
        // if host, stop host; if client, disconnect
        if (Mirror.NetworkServer.active && Mirror.NetworkClient.isConnected)
            Mirror.NetworkManager.singleton.StopHost();
        else if (Mirror.NetworkClient.isConnected)
            Mirror.NetworkManager.singleton.StopClient();
    }
}
