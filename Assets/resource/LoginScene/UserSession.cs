using UnityEngine;

/// <summary>
/// Persistent singleton that holds user session data across all scenes.
/// Created in LoginScene, persists via DontDestroyOnLoad.
/// </summary>
public class UserSession : MonoBehaviour
{
    public static UserSession Instance { get; private set; }

    public string UserId { get; set; }
    public string Email { get; set; }
    public string DisplayName { get; set; }
    public string Token { get; set; }
    public int CoinBalance { get; set; }

    public bool IsLoggedIn => !string.IsNullOrEmpty(Token) && !string.IsNullOrEmpty(UserId);

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

    public void SetFromLoginResponse(LoginResponse data)
    {
        UserId = data.user.id;
        Email = data.user.email;
        DisplayName = data.user.displayName;
        CoinBalance = data.user.coinBalance;
        Token = data.token;
    }

    public void Clear()
    {
        UserId = null;
        Email = null;
        DisplayName = null;
        Token = null;
        CoinBalance = 0;
    }
}

// --- JSON data classes for API responses ---

[System.Serializable]
public class LoginRequest
{
    public string email;
    public string password;
}

[System.Serializable]
public class RegisterRequest
{
    public string email;
    public string password;
    public string displayName;
}

[System.Serializable]
public class LoginResponse
{
    public string token;
    public UserData user;
}

[System.Serializable]
public class UserData
{
    public string id;
    public string email;
    public string displayName;
    public int coinBalance;
}

[System.Serializable]
public class BalanceResponse
{
    public int coinBalance;
}

[System.Serializable]
public class ErrorResponse
{
    public string error;
}
