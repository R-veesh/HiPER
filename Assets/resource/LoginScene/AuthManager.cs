using UnityEngine;

/// <summary>
/// Handles communication with Express.js auth endpoints.
/// Called by LoginUI and ProfileUI.
/// </summary>
public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    public static AuthManager EnsureExists()
    {
        UserSession.EnsureExists();

        if (Instance != null)
            return Instance;

        AuthManager existing = FindObjectOfType<AuthManager>();
        if (existing != null)
        {
            Instance = existing;
            DontDestroyOnLoad(existing.gameObject);
            return existing;
        }

        return UserSession.EnsureExists().GetComponent<AuthManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void Login(string email, string password, System.Action onSuccess, System.Action<string> onError)
    {
        UserSession session = UserSession.EnsureExists();
        ApiClient client = ApiClient.EnsureExists();
        var body = new LoginRequest { email = email, password = password };
        client.Post("/api/auth/login", body,
            (json) =>
            {
                var response = JsonUtility.FromJson<LoginResponse>(json);
                session.SetFromLoginResponse(response);
                onSuccess?.Invoke();
            },
            (errJson) =>
            {
                string msg = "Login failed";
                try { msg = JsonUtility.FromJson<ErrorResponse>(errJson).error; } catch { }
                onError?.Invoke(msg);
            }
        );
    }

    public void Register(string email, string password, string displayName,
        System.Action onSuccess, System.Action<string> onError)
    {
        UserSession session = UserSession.EnsureExists();
        ApiClient client = ApiClient.EnsureExists();
        var body = new RegisterRequest { email = email, password = password, displayName = displayName };
        client.Post("/api/auth/register", body,
            (json) =>
            {
                var response = JsonUtility.FromJson<LoginResponse>(json);
                session.SetFromLoginResponse(response);
                onSuccess?.Invoke();
            },
            (errJson) =>
            {
                string msg = "Registration failed";
                try { msg = JsonUtility.FromJson<ErrorResponse>(errJson).error; } catch { }
                onError?.Invoke(msg);
            }
        );
    }

    public void RefreshBalance(System.Action<int> onSuccess = null)
    {
        UserSession session = UserSession.EnsureExists();
        ApiClient.EnsureExists().Get("/api/coins/balance",
            (json) =>
            {
                var data = JsonUtility.FromJson<BalanceResponse>(json);
                session.CoinBalance = data.coinBalance;
                onSuccess?.Invoke(data.coinBalance);
            }
        );
    }

    public void Logout()
    {
        UserSession.EnsureExists().Clear();
    }
}
