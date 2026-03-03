// using UnityEngine;
//
// public class CameraFollow : MonoBehaviour
// {
//     public Transform target;
//     public Vector3 offset = new Vector3(0, 6, -10);
//     public float smoothSpeed = 8f;
//
//     void LateUpdate()
//     {
//         if (target == null) return;
//
//         Vector3 desiredPosition = target.position + offset;
//         transform.position = Vector3.Lerp(
//             transform.position,
//             desiredPosition,
//             smoothSpeed * Time.deltaTime
//         );
//
//         transform.LookAt(target);
//     }
//
//     // 🔥 THIS FIXES YOUR ERROR
//     public void SetTarget(Transform newTarget)
//     {
//         target = newTarget;
//     }
// }

using UnityEngine;
using Mirror;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 6, -10);
    public float smoothSpeed = 8f;
    
    private Camera mainCamera;
    private bool targetSet = false;
    private float searchTimer = 0.5f; // Start searching immediately
    private float totalSearchTime = 0f;
    private const float MAX_SEARCH_TIME = 30f;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
            mainCamera = Camera.main;
            
        Debug.Log("[CameraFollow] Initialized. Camera: " + (mainCamera != null ? "Found" : "NOT FOUND"));
    }

    void Update()
    {
        // Keep trying to find local player car until found
        if (target == null)
        {
            totalSearchTime += Time.deltaTime;
            searchTimer += Time.deltaTime;
            
            // Search every 0.5 seconds
            if (searchTimer >= 0.5f)
            {
                FindLocalPlayerCar();
                searchTimer = 0f;
            }
            
            // Stop searching after timeout
            if (totalSearchTime > MAX_SEARCH_TIME)
            {
                Debug.LogError("[CameraFollow] Failed to find local player car after " + MAX_SEARCH_TIME + "s! Check that GameSpawnManager is spawning cars with authority.");
                totalSearchTime = MAX_SEARCH_TIME; // Prevent spam
            }
        }
    }
    
    void FindLocalPlayerCar()
    {
        // Find the car that belongs to the local player
        // Cars have CarPlayer component, and local player's car has isLocalPlayer = true
        CarPlayer[] allCars = FindObjectsOfType<CarPlayer>();
        
        foreach (CarPlayer car in allCars)
        {
            // SAFETY: Check if car exists and has valid network setup before accessing isLocalPlayer
            if (car == null) continue;
            
            // Try-catch to handle any network initialization issues
            try
            {
                if (car.isLocalPlayer)
                {
                    target = car.transform;
                    targetSet = true;
                    Debug.Log("[CameraFollow] FOUND local player car: " + car.name);
                    return;
                }
            }
            catch (System.NullReferenceException)
            {
                // CarPlayer exists but network not initialized yet - skip it
                continue;
            }
        }
        
        // If no local player car found yet, log occasionally
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log("[CameraFollow] Waiting for local player car... Found " + allCars.Length + " cars total");
        }
    }

    void LateUpdate()
    {
        if (!target)
        {
            return;
        }

        // Calculate desired position relative to car (world space offset, not rotated with car)
        Vector3 desiredPos = target.position + offset;
        
        // Smooth follow
        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * smoothSpeed);
        
        // Look at target
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }

    public void SetTarget(Transform newTarget)
    {
        if (newTarget == null)
        {
            Debug.LogError("[CameraFollow] Attempted to set null target!");
            return;
        }

        target = newTarget;
        targetSet = true;
        Debug.Log("[CameraFollow] Target set to: " + newTarget.name);
    }
}
