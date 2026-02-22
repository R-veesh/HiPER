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

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
            mainCamera = Camera.main;
            
        Debug.Log("[CameraFollow] Initialized. Camera: " + (mainCamera != null ? "Found" : "NOT FOUND"));

        // Try to find local player car immediately
        if (target == null)
            FindLocalPlayerCar();
            
        // Fallback: find any car after delay
        Invoke("FindAnyCar", 2f);
    }

    void Update()
    {
        // Keep trying to find local player car until found
        if (target == null)
            FindLocalPlayerCar();
    }
    
    void FindAnyCar()
    {
        if (target != null) return;
        
        // Find any GameObject with CarPlayer component as fallback
        CarPlayer[] cars = FindObjectsOfType<CarPlayer>();
        foreach (var car in cars)
        {
            if (car != null)
            {
                target = car.transform;
                targetSet = true;
                Debug.Log("[CameraFollow] Fallback: Found car: " + target.name);
                return;
            }
        }
        
        // If still no car, try finding CarController
        CarController[] controllers = FindObjectsOfType<CarController>();
        foreach (var carCtrl in controllers)
        {
            if (carCtrl != null)
            {
                target = carCtrl.transform;
                targetSet = true;
                Debug.Log("[CameraFollow] Fallback: Found car with CarController: " + target.name);
                return;
            }
        }
        
        Debug.LogWarning("[CameraFollow] No cars found in scene!");
    }

    void FindLocalPlayerCar()
    {
        if (NetworkClient.localPlayer != null)
        {
            CarPlayer carPlayer = NetworkClient.localPlayer.GetComponent<CarPlayer>();
            if (carPlayer != null)
            {
                target = carPlayer.transform;
                targetSet = true;
                Debug.Log("[CameraFollow] Auto-found local player car: " + target.name);
            }
            else
            {
                Debug.LogWarning("[CameraFollow] Local player found but no CarPlayer component!");
            }
        }
    }

    void LateUpdate()
    {
        if (!target)
        {
            if (targetSet)
                Debug.LogWarning("[CameraFollow] Target lost!");
            
            FindLocalPlayerCar();
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
