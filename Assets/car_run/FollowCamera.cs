//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class FollowCamera : MonoBehaviour
//{
//    public Transform target;
//    public float smoothSpeed = 0.125f;
//    public Vector3 offset = new Vector3(0, 5, -10);

//    void LateUpdate()
//    {
//        if (target == null) return;

//        Vector3 desiredPos = target.position + offset;
//        Vector3 smoothPos = Vector3.Lerp(transform.position, desiredPos, smoothSpeed);

//        transform.position = smoothPos;
//        transform.LookAt(target);
//    }
//}
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;       // Car Body
    public float distance = 6f;    // Camera distance behind the car
    public float height = 2.5f;    // Camera height
    public float rotationDamping = 3f;
    public float heightDamping = 2f;

    void LateUpdate()
    {
        if (!target) return;

        // Desired rotation angle (car’s Y axis)
        float wantedRotationAngle = target.eulerAngles.y;
        float wantedHeight = target.position.y + height;

        // Current camera rotation angle
        float currentRotationAngle = transform.eulerAngles.y;
        float currentHeight = transform.position.y;

        // Smooth rotation
        currentRotationAngle = Mathf.LerpAngle(
            currentRotationAngle,
            wantedRotationAngle,
            rotationDamping * Time.deltaTime
        );

        // Smooth height
        currentHeight = Mathf.Lerp(
            currentHeight,
            wantedHeight,
            heightDamping * Time.deltaTime
        );

        // Convert to rotation
        Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

        // Set camera position behind car
        Vector3 newPos = target.position;
        newPos -= currentRotation * Vector3.forward * distance;
        newPos.y = currentHeight;

        transform.position = newPos;

        // Look at the car
        transform.LookAt(target);
    }
}
