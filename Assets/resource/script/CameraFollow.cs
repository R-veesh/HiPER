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

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 6, -10);
    public float smoothSpeed = 8f;

    void LateUpdate()
    {
        if (!target) return;

        Vector3 desiredPos = target.position + target.TransformDirection(offset);
        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * smoothSpeed);
        transform.LookAt(target);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
