using UnityEngine;

/// <summary>
/// Handles communication with Express.js auth endpoints.
/// Called by LoginUI and ProfileUI.
/// </summary>
public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Login(string email, string password, System.Action onSuccess, System.Action<string> onError)
    {
        var body = new LoginRequest { email = email, password = password };
        ApiClient.Instance.Post("/api/auth/login", body,
            (json) =>
            {
                var response = JsonUtility.FromJson<LoginResponse>(json);
                UserSession.Instance.SetFromLoginResponse(response);
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
        var body = new RegisterRequest { email = email, password = password, displayName = displayName };
        ApiClient.Instance.Post("/api/auth/register", body,
            (json) =>
            {
                var response = JsonUtility.FromJson<LoginResponse>(json);
                UserSession.Instance.SetFromLoginResponse(response);
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
        ApiClient.Instance.Get("/api/coins/balance",
            (json) =>
            {
                var data = JsonUtility.FromJson<BalanceResponse>(json);
                UserSession.Instance.CoinBalance = data.coinBalance;
                onSuccess?.Invoke(data.coinBalance);
            }
        );
    }

    public void Logout()
    {
        UserSession.Instance.Clear();
    }
}
