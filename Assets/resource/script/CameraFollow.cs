using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Vector3 offset = new Vector3(0, 6, -10);
    public float smoothSpeed = 8f;

    private Transform target;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * smoothSpeed);
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
        // immediate snap
        transform.position = target.position + offset;
        transform.LookAt(target.position + Vector3.up * 1.5f);
        Debug.Log("[CameraFollow] Target set: " + newTarget.name);
    }

    public void ClearTarget()
    {
        target = null;
        Debug.Log("[CameraFollow] Target cleared");
    }
}
