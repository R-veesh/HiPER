using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple race minimap/navigation overlay for MainGameScene.
/// Displays player car markers and checkpoint markers in a 2D map rect.
/// </summary>
public class RacingNavigationMap : MonoBehaviour
{
    [Header("Map UI")]
    public RectTransform mapRect;
    public RectTransform markerRoot;
    public RectTransform playerMarkerPrefab;
    public RectTransform localPlayerMarkerPrefab;
    public RectTransform checkpointMarkerPrefab;

    [Header("World Mapping (XZ Plane)")]
    public float worldMinX = -200f;
    public float worldMaxX = 200f;
    public float worldMinZ = -200f;
    public float worldMaxZ = 200f;

    [Header("Behaviour")]
    public bool rotateMarkersWithCar = true;
    public float refreshInterval = 0.15f;
    public bool enableDebugLogs = true;

    private readonly Dictionary<uint, RectTransform> playerMarkers = new Dictionary<uint, RectTransform>();
    private readonly List<RectTransform> checkpointMarkers = new List<RectTransform>();
    private float refreshTimer;
    private Transform localTarget;

    void Awake()
    {
        if (mapRect == null)
            mapRect = GetComponent<RectTransform>();

        if (markerRoot == null)
            markerRoot = mapRect;

        ValidateSetup();
    }

    void Start()
    {
        BuildCheckpointMarkers();
    }

    void Update()
    {
        refreshTimer -= Time.deltaTime;
        if (refreshTimer > 0f)
            return;

        refreshTimer = Mathf.Max(0.03f, refreshInterval);
        RefreshPlayerMarkers();
    }

    public void SetLocalTarget(Transform target)
    {
        localTarget = target;
    }

    void BuildCheckpointMarkers()
    {
        if (markerRoot == null)
            return;

#if UNITY_6000_0_OR_NEWER
        Checkpoint[] checkpoints = FindObjectsByType<Checkpoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#else
        Checkpoint[] checkpoints = FindObjectsOfType<Checkpoint>();
#endif

        for (int i = 0; i < checkpoints.Length; i++)
        {
            RectTransform marker = Instantiate(GetCheckpointMarkerPrefab(), markerRoot);
            marker.gameObject.SetActive(true);
            marker.anchoredPosition = ToMapPosition(checkpoints[i].transform.position);

            Image image = marker.GetComponent<Image>();
            if (image != null)
                image.color = checkpoints[i].isFinishLine ? new Color(0.2f, 1f, 0.2f, 1f) : new Color(1f, 0.85f, 0.2f, 1f);

            checkpointMarkers.Add(marker);
        }
    }

    void RefreshPlayerMarkers()
    {
        if (markerRoot == null || mapRect == null)
            return;

#if UNITY_6000_0_OR_NEWER
        CarPlayer[] cars = FindObjectsByType<CarPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#else
        CarPlayer[] cars = FindObjectsOfType<CarPlayer>();
#endif

        HashSet<uint> seen = new HashSet<uint>();

        for (int i = 0; i < cars.Length; i++)
        {
            CarPlayer car = cars[i];
            if (car == null || car.netIdentity == null)
                continue;

            uint id = car.netIdentity.netId;
            seen.Add(id);

            bool isLocal = car.isOwned || car.isLocalPlayer || (localTarget != null && car.transform == localTarget);
            RectTransform marker = GetOrCreateMarker(id, isLocal);
            if (marker == null)
                continue;

            marker.anchoredPosition = ToMapPosition(car.transform.position);

            if (rotateMarkersWithCar)
            {
                marker.localRotation = Quaternion.Euler(0f, 0f, -car.transform.eulerAngles.y);
            }
        }

        RemoveMissingMarkers(seen);
    }

    RectTransform GetOrCreateMarker(uint id, bool isLocal)
    {
        if (playerMarkers.TryGetValue(id, out RectTransform existing) && existing != null)
            return existing;

        RectTransform prefab = isLocal && localPlayerMarkerPrefab != null
            ? localPlayerMarkerPrefab
            : GetPlayerMarkerPrefab();
        if (prefab == null || markerRoot == null)
            return null;

        RectTransform marker = Instantiate(prefab, markerRoot);
        marker.gameObject.SetActive(true);
        playerMarkers[id] = marker;
        return marker;
    }

    void RemoveMissingMarkers(HashSet<uint> seenIds)
    {
        List<uint> toRemove = new List<uint>();
        foreach (var kvp in playerMarkers)
        {
            if (!seenIds.Contains(kvp.Key))
                toRemove.Add(kvp.Key);
        }

        for (int i = 0; i < toRemove.Count; i++)
        {
            uint id = toRemove[i];
            if (playerMarkers.TryGetValue(id, out RectTransform marker) && marker != null)
                Destroy(marker.gameObject);
            playerMarkers.Remove(id);
        }
    }

    Vector2 ToMapPosition(Vector3 worldPos)
    {
        float tX = Mathf.InverseLerp(worldMinX, worldMaxX, worldPos.x);
        float tZ = Mathf.InverseLerp(worldMinZ, worldMaxZ, worldPos.z);

        float width = mapRect.rect.width;
        float height = mapRect.rect.height;

        return new Vector2((tX - 0.5f) * width, (tZ - 0.5f) * height);
    }

    RectTransform GetPlayerMarkerPrefab()
    {
        if (playerMarkerPrefab != null)
            return playerMarkerPrefab;

        playerMarkerPrefab = CreateRuntimeMarker("AutoPlayerMarker", new Color(1f, 0.85f, 0.2f, 1f), 12f);
        return playerMarkerPrefab;
    }

    RectTransform GetCheckpointMarkerPrefab()
    {
        if (checkpointMarkerPrefab != null)
            return checkpointMarkerPrefab;

        checkpointMarkerPrefab = CreateRuntimeMarker("AutoCheckpointMarker", Color.white, 8f);
        return checkpointMarkerPrefab;
    }

    RectTransform CreateRuntimeMarker(string name, Color color, float size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(size, size);
        Image image = go.GetComponent<Image>();
        image.color = color;
        go.SetActive(false);
        return rect;
    }

    void ValidateSetup()
    {
        if (!enableDebugLogs)
            return;

        if (mapRect == null)
            Debug.LogError("[RacingNavigationMap] mapRect is missing. Assign a RectTransform panel.");
        if (markerRoot == null)
            Debug.LogError("[RacingNavigationMap] markerRoot is missing and could not default from mapRect.");
        if (worldMaxX <= worldMinX || worldMaxZ <= worldMinZ)
            Debug.LogWarning("[RacingNavigationMap] World bounds are invalid. Check min/max X and Z values.");
        if (playerMarkerPrefab == null)
            Debug.LogWarning("[RacingNavigationMap] playerMarkerPrefab not assigned. Using auto-generated marker.");
        if (checkpointMarkerPrefab == null)
            Debug.LogWarning("[RacingNavigationMap] checkpointMarkerPrefab not assigned. Using auto-generated marker.");
    }
}
