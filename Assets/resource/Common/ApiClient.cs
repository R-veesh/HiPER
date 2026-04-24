using System;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// HTTP client wrapper for the Express.js backend API.
/// Handles JWT token injection, JSON serialization, and async requests.
/// </summary>
public class ApiClient : MonoBehaviour
{
    public static ApiClient Instance { get; private set; }

    [Header("API Configuration")]
    [Tooltip("Base URL of the Express.js backend (e.g. https://your-app.onrender.com)")]
    public string baseUrl = "http://localhost:3000";

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

    // --- Public API Methods ---

    public void Get(string endpoint, Action<string> onSuccess, Action<string> onError = null)
    {
        StartCoroutine(SendRequest("GET", endpoint, null, onSuccess, onError));




        
    }

    public void Post(string endpoint, object body, Action<string> onSuccess, Action<string> onError = null)
    {
        string json = body != null ? JsonUtility.ToJson(body) : null;
        StartCoroutine(SendRequest("POST", endpoint, json, onSuccess, onError));
    }

    public void Put(string endpoint, object body, Action<string> onSuccess, Action<string> onError = null)
    {
        string json = body != null ? JsonUtility.ToJson(body) : null;
        StartCoroutine(SendRequest("PUT", endpoint, json, onSuccess, onError));
    }

    public void PostRaw(string endpoint, string json, Action<string> onSuccess, Action<string> onError = null)
    {
        StartCoroutine(SendRequest("POST", endpoint, json, onSuccess, onError));
    }

    // --- Core Request Handler ---

    private IEnumerator SendRequest(string method, string endpoint, string jsonBody,
        Action<string> onSuccess, Action<string> onError)
    {
        string url = baseUrl.TrimEnd('/') + endpoint;

        UnityWebRequest request;
        if (method == "GET")
        {
            request = UnityWebRequest.Get(url);
        }
        else
        {
            byte[] bodyBytes = jsonBody != null ? Encoding.UTF8.GetBytes(jsonBody) : new byte[0];
            request = new UnityWebRequest(url, method);
            request.uploadHandler = new UploadHandlerRaw(bodyBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
        }

        // Inject JWT token if available
        if (UserSession.Instance != null && !string.IsNullOrEmpty(UserSession.Instance.Token))
        {
            request.SetRequestHeader("Authorization", "Bearer " + UserSession.Instance.Token);
        }

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            onSuccess?.Invoke(request.downloadHandler.text);
        }
        else
        {
            string errorBody = request.downloadHandler?.text ?? request.error;
            Debug.LogWarning($"[ApiClient] {method} {endpoint} failed: {request.responseCode} - {errorBody}");
            onError?.Invoke(errorBody);
        }

        request.Dispose();
    }
}
