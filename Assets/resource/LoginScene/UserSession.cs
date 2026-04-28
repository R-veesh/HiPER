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
    public string ProfileImageUrl { get; set; }
    public int PlayerLevel { get; set; } = 1;
    public int TotalMatches { get; set; }
    public int MatchesWon { get; set; }
    public int[] OwnedCarIndices { get; private set; } = new[] { 0 };
    public int PreferredCarIndex { get; set; }

    public bool IsLoggedIn => !string.IsNullOrEmpty(Token) && !string.IsNullOrEmpty(UserId);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutoBootstrap()
    {
        EnsureExists();
    }

    public static UserSession EnsureExists()
    {
        if (Instance != null)
            return Instance;

        UserSession existing = FindObjectOfType<UserSession>();
        if (existing != null)
        {
            Instance = existing;
            EnsureCompanions(existing.gameObject);
            DontDestroyOnLoad(existing.gameObject);
            return existing;
        }

        GameObject bootstrap = new GameObject("SessionBootstrap");
        UserSession session = bootstrap.AddComponent<UserSession>();
        EnsureCompanions(bootstrap);
        return session;
    }

    static void EnsureCompanions(GameObject target)
    {
        if (target.GetComponent<AuthManager>() == null)
            target.AddComponent<AuthManager>();

        if (target.GetComponent<ApiClient>() == null)
            target.AddComponent<ApiClient>();
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

    public void SetFromLoginResponse(LoginResponse data)
    {
        if (data == null || data.user == null)
        {
            Debug.LogWarning("[UserSession] Login response or user payload was null");
            return;
        }

        UserId = data.user.id;
        Email = data.user.email;
        DisplayName = data.user.displayName;
        CoinBalance = data.user.coinBalance;
        ProfileImageUrl = data.user.profilePicUrl;
        PlayerLevel = Mathf.Max(1, data.user.level);
        TotalMatches = data.user.totalMatches;
        MatchesWon = data.user.matchesWon;
        SetOwnedCarIndices(data.user.ownedCarIndices);
        Token = data.token;
    }

    public void SetOwnedCarIndices(int[] ownedCars)
    {
        if (ownedCars == null || ownedCars.Length == 0)
        {
            OwnedCarIndices = new[] { 0 };
            return;
        }

        OwnedCarIndices = ownedCars;

        if (!OwnsCar(PreferredCarIndex))
            PreferredCarIndex = OwnedCarIndices[0];
    }

    public bool OwnsCar(int carIndex)
    {
        if (OwnedCarIndices == null) return carIndex == 0;

        for (int i = 0; i < OwnedCarIndices.Length; i++)
        {
            if (OwnedCarIndices[i] == carIndex)
                return true;
        }

        return false;
    }

    public void Clear()
    {
        UserId = null;
        Email = null;
        DisplayName = null;
        Token = null;
        CoinBalance = 0;
        ProfileImageUrl = null;
        PlayerLevel = 1;
        TotalMatches = 0;
        MatchesWon = 0;
        OwnedCarIndices = new[] { 0 };
        PreferredCarIndex = 0;
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
    public string profilePicUrl;
    public int level;
    public int totalMatches;
    public int matchesWon;
    public int[] ownedCarIndices;
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
