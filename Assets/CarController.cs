using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    [Header("Car Setup")]
    public float maxSpeed = 180f;
    public float acceleration = 300f;
    public float brakeForce = 800f;
    public float maxSteerAngle = 30f;
    public float steeringSmooth = 6f;

    [Header("Wheels")]
    public WheelCollider frontLeftCol;
    public WheelCollider frontRightCol;
    public WheelCollider rearLeftCol;
    public WheelCollider rearRightCol;

    public Transform frontLeftMesh;
    public Transform frontRightMesh;
    public Transform rearLeftMesh;
    public Transform rearRightMesh;

    [Header("Center Of Mass")]
    public Vector3 centerOfMass = new Vector3(0, -0.5f, 0);

    [Header("Gear System")]
    public int totalGears = 6;
    public int currentGear = 1;
    public float minRPM = 900f;
    public float maxRPM = 7000f;
    public float engineRPM;
    private float[] gearSpeed;

    [Header("Sound")]
    public AudioSource engineSound;

    [Header("Drift")]
    public bool driftEnabled = false;
    public KeyCode driftToggleKey = KeyCode.T;
    public float driftMultiplier = 2.5f;

    [Header("Drive Mode")]
    public DriveMode driveMode = DriveMode.Comfort;

    public enum DriveMode { Comfort, Sport, Drift }

    private Rigidbody rb;
    private float motorInput;
    private float steerInput;
    private float smoothSteer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass;

        gearSpeed = new float[totalGears + 1];
        float gearStep = maxSpeed / totalGears;
        for (int i = 1; i <= totalGears; i++)
            gearSpeed[i] = gearStep * i;

        ApplyDriveMode();
    }

    void Update()
    {
        motorInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");

        if (Input.GetKeyDown(driftToggleKey))
            driftEnabled = !driftEnabled;

        HandleGears();
        UpdateEngineSound();
        UpdateWheelMeshes();
    }

    void FixedUpdate()
    {
        HandleMotor();
        HandleSteering();
        HandleDrift();
    }

    // ================= MOTOR =================
    void HandleMotor()
    {
        float speed = rb.linearVelocity.magnitude * 3.6f;

        if (speed < maxSpeed)
        {
            float torque = motorInput * acceleration * currentGear;
            frontLeftCol.motorTorque = torque;
            frontRightCol.motorTorque = torque;
        }

        if (motorInput == 0)
            ApplyBrake(200f);
        else
            ApplyBrake(0f);
    }

    void ApplyBrake(float force)
    {
        frontLeftCol.brakeTorque = force;
        frontRightCol.brakeTorque = force;
        rearLeftCol.brakeTorque = force;
        rearRightCol.brakeTorque = force;
    }

    // ================= STEERING =================
    void HandleSteering()
    {
        smoothSteer = Mathf.Lerp(smoothSteer, steerInput, Time.fixedDeltaTime * steeringSmooth);
        float steerAngle = smoothSteer * maxSteerAngle;
        frontLeftCol.steerAngle = steerAngle;
        frontRightCol.steerAngle = steerAngle;
    }

    // ================= DRIFT =================
    void HandleDrift()
    {
        if (!driftEnabled) return;

        WheelFrictionCurve friction = rearLeftCol.sidewaysFriction;
        friction.stiffness = Input.GetKey(KeyCode.Space) ? driftMultiplier : 1f;
        rearLeftCol.sidewaysFriction = friction;
        rearRightCol.sidewaysFriction = friction;
    }

    // ================= GEARS =================
    void HandleGears()
    {
        float speed = rb.linearVelocity.magnitude * 3.6f;

        if (currentGear < totalGears && speed > gearSpeed[currentGear])
            currentGear++;

        if (currentGear > 1 && speed < gearSpeed[currentGear - 1] - 10f)
            currentGear--;

        float lowSpeed = currentGear > 1 ? gearSpeed[currentGear - 1] : 0f;
        float highSpeed = gearSpeed[currentGear];

        engineRPM = Mathf.Lerp(
            minRPM,
            maxRPM,
            Mathf.InverseLerp(lowSpeed, highSpeed, speed)
        );
    }

    // ================= SOUND =================
    void UpdateEngineSound()
    {
        if (!engineSound) return;

        float rpmNormalized = engineRPM / maxRPM;
        engineSound.pitch = Mathf.Lerp(0.9f, 2f, rpmNormalized);
        engineSound.volume = Mathf.Lerp(0.4f, 1f, rpmNormalized);
    }

    // ================= DRIVE MODES =================
    void ApplyDriveMode()
    {
        switch (driveMode)
        {
            case DriveMode.Comfort:
                acceleration = 250f;
                steeringSmooth = 5f;
                driftMultiplier = 1.5f;
                break;

            case DriveMode.Sport:
                acceleration = 350f;
                steeringSmooth = 7f;
                driftMultiplier = 2.5f;
                break;

            case DriveMode.Drift:
                acceleration = 400f;
                steeringSmooth = 9f;
                driftMultiplier = 4f;
                driftEnabled = true;
                break;
        }
    }

    // ================= WHEEL VISUALS =================
    void UpdateWheelMeshes()
    {
        UpdateWheel(frontLeftCol, frontLeftMesh);
        UpdateWheel(frontRightCol, frontRightMesh);
        UpdateWheel(rearLeftCol, rearLeftMesh);
        UpdateWheel(rearRightCol, rearRightMesh);
    }

    void UpdateWheel(WheelCollider col, Transform mesh)
    {
        col.GetWorldPose(out Vector3 pos, out Quaternion rot);
        mesh.position = pos;
        mesh.rotation = rot;
    }
}
