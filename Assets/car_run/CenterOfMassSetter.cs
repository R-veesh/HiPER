using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CenterOfMassSetter : MonoBehaviour
{
    public Rigidbody rb;           // Your car's Rigidbody
    public Transform centerOfMass; // Empty object slightly below the car

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();

        if (centerOfMass != null)
        {
            rb.centerOfMass = centerOfMass.localPosition;
            Debug.Log("Center of Mass set to: " + centerOfMass.localPosition);
        }

        // Optional: stabilize car physics
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.maxAngularVelocity = 10f;
    }


}
