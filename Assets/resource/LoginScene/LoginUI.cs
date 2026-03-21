using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Login/Register UI for the LoginScene.
/// Handles email/password input, login/register buttons, and scene transition.
/// 
/// Setup in Unity:
/// 1. Create a Canvas in LoginScene
/// 2. Add two panels: LoginPanel and RegisterPanel
/// 3. Assign all UI references in Inspector
/// 4. Attach this script + AuthManager + UserSession + ApiClient to a GameObject
/// </summary>
public class LoginUI : MonoBehaviour
{
    [Header("Login Panel")]
    public GameObject loginPanel;
    public TMP_InputField loginEmailField;
    public TMP_InputField loginPasswordField;
    public Button loginButton;
    public Button switchToRegisterButton;
    public TextMeshProUGUI loginErrorText;

    [Header("Register Panel")]
    public GameObject registerPanel;
    public TMP_InputField registerNameField;
    public TMP_InputField registerEmailField;
    public TMP_InputField registerPasswordField;
    public TMP_InputField registerConfirmPasswordField;
    public Button registerButton;
    public Button switchToLoginButton;
    public TextMeshProUGUI registerErrorText;

    [Header("Loading")]
    public GameObject loadingOverlay;

    [Header("Scene")]
    public string mainMenuSceneName = "MainMenuScene";

    private void Start()
    {
        ShowLoginPanel();

        loginButton.onClick.AddListener(OnLoginClicked);
        switchToRegisterButton.onClick.AddListener(ShowRegisterPanel);
        registerButton.onClick.AddListener(OnRegisterClicked);
        switchToLoginButton.onClick.AddListener(ShowLoginPanel);

        // If already logged in, skip to main menu
        if (UserSession.Instance != null && UserSession.Instance.IsLoggedIn)
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    private void ShowLoginPanel()
    {
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);
        loginErrorText.text = "";
    }

    private void ShowRegisterPanel()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
        registerErrorText.text = "";
    }

    private void OnLoginClicked()
    {
        string email = loginEmailField.text.Trim();
        string password = loginPasswordField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            loginErrorText.text = "Please fill in all fields";
            return;
        }

        SetLoading(true);
        loginErrorText.text = "";

        AuthManager.Instance.Login(email, password,
            () =>
            {
                SetLoading(false);
                SceneManager.LoadScene(mainMenuSceneName);
            },
            (error) =>
            {
                SetLoading(false);
                loginErrorText.text = error;
            }
        );
    }

    private void OnRegisterClicked()
    {
        string name = registerNameField.text.Trim();
        string email = registerEmailField.text.Trim();
        string password = registerPasswordField.text;
        string confirm = registerConfirmPasswordField.text;

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(password))
        {
            registerErrorText.text = "Please fill in all fields";
            return;
        }

        if (password != confirm)
        {
            registerErrorText.text = "Passwords do not match";
            return;
        }

        if (password.Length < 8)
        {
            registerErrorText.text = "Password must be at least 8 characters";
            return;
        }

        SetLoading(true);
        registerErrorText.text = "";

        AuthManager.Instance.Register(email, password, name,
            () =>
            {
                SetLoading(false);
                SceneManager.LoadScene(mainMenuSceneName);
            },
            (error) =>
            {
                SetLoading(false);
                registerErrorText.text = error;
            }
        );
    }

    private void SetLoading(bool loading)
    {
        if (loadingOverlay != null) loadingOverlay.SetActive(loading);
        loginButton.interactable = !loading;
        registerButton.interactable = !loading;
    }
}
