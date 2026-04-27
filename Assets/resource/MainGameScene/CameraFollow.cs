using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Vector3 offset = new Vector3(0, 6, -10);
    public float smoothSpeed = 8f;
    public float rotationSmooth = 5f;

    private Transform target;

    void Awake()
    {
        AudioListenerEnforcer.KeepOnly(GetComponent<AudioListener>());
    }

    void LateUpdate()
    {
        if (target == null) return;

        // offset relative to the car's facing direction (behind + above)
        Vector3 desiredPos = target.position
                           + target.forward * offset.z
                           + target.up * offset.y
                           + target.right * offset.x;

        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * smoothSpeed);

        // smoothly rotate to look at a point slightly above the car
        Vector3 lookTarget = target.position + Vector3.up * 1.5f;
        Quaternion desiredRot = Quaternion.LookRotation(lookTarget - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, Time.deltaTime * rotationSmooth);
    }

    public void SetTarget(Transform newTarget)
    {
        if (newTarget == null)
        {
            Debug.LogError("[CameraFollow] Attempted to set null target!");
            return;
        }

        target = newTarget;
        // immediate snap behind the car
        transform.position = target.position
                           + target.forward * offset.z
                           + target.up * offset.y
                           + target.right * offset.x;
        transform.LookAt(target.position + Vector3.up * 1.5f);
        Debug.Log("[CameraFollow] Target set: " + newTarget.name);
    }

    public void ClearTarget()
    {
        target = null;
        Debug.Log("[CameraFollow] Target cleared");
    }
}
