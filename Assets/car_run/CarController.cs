using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    //    public float motorForce = 1500f;
    //    public float brakeForce = 3000f;
    //    public float steeringAngle = 30f;

    //    public WheelCollider frontLeft;
    //    public WheelCollider frontRight;
    //    public WheelCollider rearLeft;
    //    public WheelCollider rearRight;



    //    void FixedUpdate()
    //    {
    //        float horizontal = Input.GetAxis("Horizontal");
    //        float vertical = Input.GetAxis("Vertical");

    //        // Motor
    //        rearLeft.motorTorque = vertical * motorForce * Time.deltaTime;
    //        rearRight.motorTorque = vertical * motorForce * Time.deltaTime;

    //        // Steering
    //        float steer = steeringAngle * horizontal;
    //        frontLeft.steerAngle = steer;
    //        frontRight.steerAngle = steer;

    //        // Brake
    //        if (Input.GetKey(KeyCode.Space))
    //        {
    //            frontLeft.brakeTorque = brakeForce;
    //            frontRight.brakeTorque = brakeForce;
    //            rearLeft.brakeTorque = brakeForce;
    //            rearRight.brakeTorque = brakeForce;
    //        }
    //        else
    //        {
    //            frontLeft.brakeTorque = 0;
    //            frontRight.brakeTorque = 0;
    //            rearLeft.brakeTorque = 0;
    //            rearRight.brakeTorque = 0;
    //        }
    //    }

    [Header("Rigidbody & COM")]
    public Rigidbody rb;
    public Transform centerOfMass;

    [Header("Wheel Colliders")]
    public WheelCollider frontLeftWheel;
    public WheelCollider frontRightWheel;
    public WheelCollider backLeftWheel;
    public WheelCollider backRightWheel;

    [Header("Wheel Transforms (visual)")]
    public Transform frontLeftTransform;
    public Transform frontRightTransform;
    public Transform backLeftTransform;
    public Transform backRightTransform;

    [Header("Car Settings")]
    public float maxMotorTorque = 1500f;
    public float maxSteerAngle = 30f;
    public float brakeTorque = 3000f;

    [Header("Camera")]
    public Transform cameraTarget;
    public Vector3 cameraOffset = new Vector3(0, 5, -10);
    public float cameraSmooth = 0.1f;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();

        // Set center of mass for stability
        if (centerOfMass != null)
            rb.centerOfMass = centerOfMass.localPosition;

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.maxAngularVelocity = 10f;
    }

    void FixedUpdate()
    {
        HandleMovement();
        UpdateWheels();
    }

    void LateUpdate()
    {
        HandleCamera();
    }

    void HandleMovement()
    {
        float motor = maxMotorTorque * Input.GetAxis("Vertical");
        float steering = maxSteerAngle * Input.GetAxis("Horizontal");

        // Apply motor torque to back wheels
        backLeftWheel.motorTorque = motor;
        backRightWheel.motorTorque = motor;

        // Apply steering to front wheels
        frontLeftWheel.steerAngle = steering;
        frontRightWheel.steerAngle = steering;

        // Optional braking
        if (Input.GetKey(KeyCode.Space))
        {
            backLeftWheel.brakeTorque = brakeTorque;
            backRightWheel.brakeTorque = brakeTorque;
        }
        else
        {
            backLeftWheel.brakeTorque = 0;
            backRightWheel.brakeTorque = 0;
        }
    }

    void UpdateWheels()
    {
        UpdateWheel(frontLeftWheel, frontLeftTransform);
        UpdateWheel(frontRightWheel, frontRightTransform);
        UpdateWheel(backLeftWheel, backLeftTransform);
        UpdateWheel(backRightWheel, backRightTransform);
    }

    void UpdateWheel(WheelCollider wc, Transform wt)
    {
        Vector3 pos;
        Quaternion rot;
        wc.GetWorldPose(out pos, out rot);
        if (wt != null)
        {
            wt.position = pos;
            wt.rotation = rot;
        }
    }

    void HandleCamera()
    {
        if (cameraTarget != null)
        {
            Vector3 desiredPos = cameraTarget.position + cameraOffset;
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, desiredPos, cameraSmooth);
            Camera.main.transform.LookAt(cameraTarget);
        }
    }
}
