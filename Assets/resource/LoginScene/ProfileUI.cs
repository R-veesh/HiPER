using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Optional profile editing UI. Can be placed in MainMenuScene or its own scene.
/// Allows editing display name, age, bio via the Express API.
/// </summary>
public class ProfileUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField displayNameField;
    public TMP_InputField ageField;
    public TMP_InputField bioField;
    public TextMeshProUGUI statusText;
    public Button saveButton;
    public Button closeButton;
    public GameObject panel;

    private void Start()
    {
        saveButton.onClick.AddListener(OnSaveClicked);
        if (closeButton != null) closeButton.onClick.AddListener(() => panel.SetActive(false));
    }

    public void Show()
    {
        panel.SetActive(true);
        statusText.text = "";

        if (UserSession.Instance == null || !UserSession.Instance.IsLoggedIn) return;

        // Load current profile
        ApiClient.Instance.Get("/api/profile/" + UserSession.Instance.UserId,
            (json) =>
            {
                var profile = JsonUtility.FromJson<ProfileData>(json);
                displayNameField.text = profile.displayName;
                ageField.text = profile.age.ToString();
                bioField.text = profile.bio;
            },
            (err) => statusText.text = "Failed to load profile"
        );
    }

    private void OnSaveClicked()
    {
        var body = new ProfileUpdateRequest
        {
            displayName = displayNameField.text.Trim(),
            age = int.TryParse(ageField.text, out int a) ? a : 0,
            bio = bioField.text.Trim()
        };

        saveButton.interactable = false;
        statusText.text = "Saving...";

        ApiClient.Instance.Put("/api/profile", body,
            (json) =>
            {
                saveButton.interactable = true;
                statusText.text = "Profile saved!";
                UserSession.Instance.DisplayName = body.displayName;
            },
            (err) =>
            {
                saveButton.interactable = true;
                statusText.text = "Save failed";
            }
        );
    }
}

[System.Serializable]
public class ProfileData
{
    public string userId;
    public string displayName;
    public int age;
    public string bio;
    public string profilePicUrl;
    public string createdAt;
}

[System.Serializable]
public class ProfileUpdateRequest
{
    public string displayName;
    public int age;
    public string bio;
}
