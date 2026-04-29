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
    public TMP_InputField profileImageUrlField;
    public TMP_InputField ageField;
    public TMP_InputField bioField;
    public TextMeshProUGUI statusText;
    public Button saveButton;
    public Button closeButton;
    public GameObject panel;

    void Awake()
    {
        if (panel == null)
            panel = gameObject;
    }

    private void Start()
    {
        UserSession.EnsureExists();
        AuthManager.EnsureExists();
        ApiClient.EnsureExists();

        WireButtons();
    }

    public void Show()
    {
        UserSession session = UserSession.EnsureExists();
        ApiClient client = ApiClient.EnsureExists();
        WireButtons();

        if (panel != null)
            panel.SetActive(true);

        if (statusText != null)
            statusText.text = "";

        if (!session.IsLoggedIn) return;

        // Load current profile
        client.Get("/api/profile/" + session.UserId,
            (json) =>
            {
                var profile = JsonUtility.FromJson<ProfileData>(json);
                if (displayNameField != null) displayNameField.text = profile.displayName;
                if (profileImageUrlField != null) profileImageUrlField.text = profile.profilePicUrl;
                if (ageField != null) ageField.text = profile.age.ToString();
                if (bioField != null) bioField.text = profile.bio;
            },
            (err) =>
            {
                if (statusText != null)
                    statusText.text = "Failed to load profile";
            }
        );
    }

    void WireButtons()
    {
        if (saveButton != null)
        {
            saveButton.onClick.RemoveListener(OnSaveClicked);
            saveButton.onClick.AddListener(OnSaveClicked);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(ClosePanel);
            closeButton.onClick.AddListener(ClosePanel);
        }
    }

    void ClosePanel()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    private void OnSaveClicked()
    {
        UserSession session = UserSession.EnsureExists();
        ApiClient client = ApiClient.EnsureExists();

        var body = new ProfileUpdateRequest
        {
            displayName = displayNameField.text.Trim(),
            profilePicUrl = profileImageUrlField != null ? profileImageUrlField.text.Trim() : string.Empty,
            age = int.TryParse(ageField.text, out int a) ? a : 0,
            bio = bioField.text.Trim()
        };

        saveButton.interactable = false;
        statusText.text = "Saving...";

        client.Put("/api/profile", body,
            (json) =>
            {
                saveButton.interactable = true;
                statusText.text = "Profile saved!";
                session.DisplayName = body.displayName;
                session.ProfileImageUrl = body.profilePicUrl;
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
    public string profilePicUrl;
    public int age;
    public string bio;
}
