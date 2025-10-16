using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class XRWheelCarControllerXR : MonoBehaviour
{
    [Header("XR Input (assign from Input Actions asset)")]
    public InputActionProperty leftStickAction;       // Vector2 (thumbstick)
    public InputActionProperty rightTriggerAction;    // Float (trigger)
    public InputActionProperty handbrakeAction;       // Button/Float (thumbstick click or button)

    [Header("Wheel Colliders (assign)")]
    public WheelCollider frontLeft;
    public WheelCollider frontRight;
    public WheelCollider rearLeft;
    public WheelCollider rearRight;

    [Header("Wheel Visuals (optional)")]
    public Transform frontLeftMesh;
    public Transform frontRightMesh;
    public Transform rearLeftMesh;
    public Transform rearRightMesh;

    [Header("Drive Settings")]
    [Tooltip("Rotational force on the wheels to make them spin")]
    public float maxMotorTorque = 1500f;   // Nm applied to driven wheels
    public float maxSteerAngle = 30f;      // degrees
    [Tooltip("Controls the car's braking power")]
    public float brakeTorque = 3000f;      // Nm for normal brake
    [Tooltip("Controls the car's handbrake power")]
    public float handbrakeTorque = 6000f;  // Nm for handbrake

    [Tooltip("Resists car roll by applying forces on left/right wheels")]
    [Header("Anti-roll")]
    public float antiRoll = 5000f;

    [Header("Tuning")]
    public Transform centerOfMass; // child transform to lower CoM

    [Header("Headlights")]
    public Light headlightLeft;
    public Light headlightRight;
    public InputActionProperty headlightToggleAction;  // e.g., trigger, button, or thumbstick press
    private bool headlightsOn = false;

    // internal
    Rigidbody rb;

    private void Update()
    {
        // rb.rigidbody.magnitude gives the speed in m/s
        // multiply it by 3.6 to get the speed in kmph
        //float speed = rb.linearVelocity.magnitude * 3.6f;
        //Debug.Log("Speed:" + speed + " km/h");
        // check if brake is being pressed
        //if (handbrakeAction != null && handbrakeAction.action != null)
        //{
        //    float temp = handbrakeAction.action.ReadValue<float>();
        //    if(temp != 0)
        //        Debug.Log("Pressing hand brakes: " + temp);
        //}

        // --- Toggle headlights ---
        if (headlightToggleAction != null && headlightToggleAction.action != null)
        {
            if (headlightToggleAction.action.WasPressedThisFrame())
            {
                headlightsOn = !headlightsOn;
                SetHeadlights(headlightsOn);
                Debug.Log("Headlights: " + (headlightsOn ? "ON" : "OFF"));
            }
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (centerOfMass != null) rb.centerOfMass = centerOfMass.localPosition;
    }

    void OnEnable()
    {
        // Enable actions safely
        if (leftStickAction != null && leftStickAction.action != null) leftStickAction.action.Enable();
        if (rightTriggerAction != null && rightTriggerAction.action != null) rightTriggerAction.action.Enable();
        if (handbrakeAction != null && handbrakeAction.action != null) handbrakeAction.action.Enable();
        if (headlightToggleAction != null && headlightToggleAction.action != null) headlightToggleAction.action.Enable();
    }

    void OnDisable()
    {
        if (leftStickAction != null && leftStickAction.action != null) leftStickAction.action.Disable();
        if (rightTriggerAction != null && rightTriggerAction.action != null) rightTriggerAction.action.Disable();
        if (handbrakeAction != null && handbrakeAction.action != null) handbrakeAction.action.Disable();
        if (headlightToggleAction != null && headlightToggleAction.action != null) headlightToggleAction.action.Disable();
    }

    private void Start()
    {
        TweakFriction();
    }

    void FixedUpdate()
    {
        // --- Read XR inputs (left stick: x = steer, y = throttle) ---
        Vector2 left = Vector2.zero;
        if (leftStickAction != null && leftStickAction.action != null)
            left = leftStickAction.action.ReadValue<Vector2>();

        // defensive NaN / Infinity checks
        if (!IsFinite(left.x)) left.x = 0f;
        if (!IsFinite(left.y)) left.y = 0f;

        float steerInput = Mathf.Clamp(left.x, -1f, 1f);
        float throttleInput = Mathf.Clamp(left.y, -1f, 1f); // forward = positive
        float motor = throttleInput * maxMotorTorque;
        float steer = steerInput * maxSteerAngle;

        // --- Brake (trigger) and handbrake (button) ---
        float brakeInput = 0f;

        if (rightTriggerAction != null && rightTriggerAction.action != null)
        {
            brakeInput = rightTriggerAction.action.ReadValue<float>(); // expected 0..1
            if (!IsFinite(brakeInput)) brakeInput = 0f;
            brakeInput = Mathf.Clamp01(brakeInput);
        }

        bool handbrake = false;
        if (handbrakeAction != null && handbrakeAction.action != null)
        {
            // Read as float or button (works with either)
            float hb = handbrakeAction.action.ReadValue<float>();
            if (!IsFinite(hb)) hb = 0f;
            handbrake = hb > 0.5f;
        }

        // --- Apply steering ---
        if (frontLeft != null) frontLeft.steerAngle = steer;
        if (frontRight != null) frontRight.steerAngle = steer;

        // --- Apply motor torque (rear-wheel drive default) ---
        if (rearLeft != null) rearLeft.motorTorque = motor;
        if (rearRight != null) rearRight.motorTorque = motor;

        // --- Apply brakes ---
        float appliedBrakeTorque = brakeInput * brakeTorque;

        // If handbrake, override rear brakes with stronger torque
        if (handbrake)
        {
            if (rearLeft != null) rearLeft.brakeTorque = handbrakeTorque;
            if (rearRight != null) rearRight.brakeTorque = handbrakeTorque;
            // front wheels still get normal brake off (so it can rotate if desired)
            if (frontLeft != null) frontLeft.brakeTorque = 0f;
            if (frontRight != null) frontRight.brakeTorque = 0f;
        }
        else
        {
            // Normal braking
            if (frontLeft != null) frontLeft.brakeTorque = appliedBrakeTorque;
            if (frontRight != null) frontRight.brakeTorque = appliedBrakeTorque;
            if (rearLeft != null) rearLeft.brakeTorque = appliedBrakeTorque;
            if (rearRight != null) rearRight.brakeTorque = appliedBrakeTorque;
        }

        // --- Anti-roll bar (left-right pairs) ---
        ApplyAntiRoll(frontLeft, frontRight);
        ApplyAntiRoll(rearLeft, rearRight);

        //AdjustFrictionForDrift();
        HandleDynamicDrift(throttleInput, steerInput);

        // Steering Wheel Update
        UpdateSteeringWheel(steer);

        // --- Update wheel visuals ---
        //UpdateWheelPose(frontLeft, frontLeftMesh);
        //UpdateWheelPose(frontRight, frontRightMesh);
        //UpdateWheelPose(rearLeft, rearLeftMesh);
        //UpdateWheelPose(rearRight, rearRightMesh);

        // --- Telemetry calculations ---
        if (rb != null)
        {
            float newSpeed = rb.linearVelocity.magnitude * 3.6f;
            currentAcceleration = (newSpeed - currentSpeedKmh) / Time.fixedDeltaTime;
            currentSpeedKmh = newSpeed;

            if (steeringWheel != null)
                currentSteeringWheelRotation = steeringWheel.localRotation.z * maxWheelRotation;

            //Estimate torque based on angular acceleration of the wheels
            if (rearLeft != null)
            {
                float angularVelRad = rearLeft.rpm * Mathf.Deg2Rad * 6f; // convert RPM to rad/s
                float wheelInertia = 1.5f; // approximate inertia of a car wheel (tunable)
                currentTorque = Mathf.Clamp(angularVelRad * wheelInertia, -maxMotorTorque, maxMotorTorque);
            }

            //float topSpeed = 40f; // m/s ≈ 144 km/h — adjust to match your car
            //float speedFactor = Mathf.Clamp01(newSpeed / topSpeed);

            //// Approximate torque inversely proportional to speed
            //currentTorque = maxMotorTorque * (1f - speedFactor);

        }

    }

    // safe anti-roll calculation: resists body roll by applying forces on left/right wheels
    void ApplyAntiRoll(WheelCollider wheelL, WheelCollider wheelR)
    {
        if (wheelL == null || wheelR == null || rb == null) return;

        WheelHit hit;
        float travelL = 1f;
        float travelR = 1f;
        bool groundedL = wheelL.GetGroundHit(out hit);
        if (groundedL && wheelL.suspensionDistance > 0.0001f)
        {
            travelL = (-wheelL.transform.InverseTransformPoint(hit.point).y - wheelL.radius) / wheelL.suspensionDistance;
        }
        bool groundedR = wheelR.GetGroundHit(out hit);
        if (groundedR && wheelR.suspensionDistance > 0.0001f)
        {
            travelR = (-wheelR.transform.InverseTransformPoint(hit.point).y - wheelR.radius) / wheelR.suspensionDistance;
        }

        travelL = Mathf.Clamp(travelL, -1f, 1f);
        travelR = Mathf.Clamp(travelR, -1f, 1f);

        float antiRollForce = (travelL - travelR) * antiRoll;

        if (float.IsNaN(antiRollForce) || float.IsInfinity(antiRollForce)) return;

        if (groundedL) rb.AddForceAtPosition(wheelL.transform.up * -antiRollForce, wheelL.transform.position);
        if (groundedR) rb.AddForceAtPosition(wheelR.transform.up * antiRollForce, wheelR.transform.position);
    }

    // copies wheel collider pose into the visual mesh transform
    //void UpdateWheelPose(WheelCollider col, Transform mesh)
    //{
    //    if (col == null || mesh == null) return;
    //    Vector3 pos;
    //    Quaternion rot;
    //    col.GetWorldPose(out pos, out rot);
    //    mesh.position = pos;
    //    mesh.rotation = rot;
    //}

    bool IsFinite(float f) => !(float.IsNaN(f) || float.IsInfinity(f));

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        WheelCollider[] wheels = { frontLeft, frontRight, rearLeft, rearRight };
        foreach (var w in wheels)
        {
            if (w != null)
            {
                Gizmos.DrawWireSphere(w.transform.position - Vector3.up * w.suspensionDistance * 0.5f, w.radius);
            }
        }
    }
#endif

    void AdjustFrictionForDrift()
    {
        if (rearLeft == null || rearRight == null) return;

        WheelFrictionCurve sidewaysL = rearLeft.sidewaysFriction;
        WheelFrictionCurve sidewaysR = rearRight.sidewaysFriction;

        // Lower stiffness for easier sliding
        sidewaysL.stiffness = 0.6f;
        sidewaysR.stiffness = 0.6f;

        // Slightly increase slip values for smoother drift
        sidewaysL.extremumSlip = 0.5f;
        sidewaysL.asymptoteSlip = 1.0f;

        sidewaysR.extremumSlip = 0.5f;
        sidewaysR.asymptoteSlip = 1.0f;

        rearLeft.sidewaysFriction = sidewaysL;
        rearRight.sidewaysFriction = sidewaysR;
    }

    void HandleDynamicDrift(float throttle, float steer)
    {
        if (rearLeft == null || rearRight == null) return;

        float driftFactor = Mathf.Abs(throttle * steer);

        WheelFrictionCurve leftFriction = rearLeft.sidewaysFriction;
        WheelFrictionCurve rightFriction = rearRight.sidewaysFriction;

        // Reduce grip dynamically based on driftFactor
        leftFriction.stiffness = Mathf.Lerp(1.0f, 0.5f, driftFactor);
        rightFriction.stiffness = Mathf.Lerp(1.0f, 0.5f, driftFactor);

        rearLeft.sidewaysFriction = leftFriction;
        rearRight.sidewaysFriction = rightFriction;
    }

    [Header("Wheel Tuning")]
    public float forwardStiffness = 0.8f;
    public float sidewaysStiffness = 0.9f;
    void TweakFriction()
    {
        WheelCollider[] wheels = { frontLeft, frontRight, rearLeft, rearRight };
        foreach (var w in wheels)
        {
            if (w == null) continue;

            var f = w.forwardFriction;
            f.stiffness = forwardStiffness;  // default is 1.0 — reduces grip, allows higher wheel RPM
            w.forwardFriction = f;

            var s = w.sidewaysFriction;
            s.stiffness = sidewaysStiffness;
            w.sidewaysFriction = s;
        }
    }

    [Header("Steering Wheel Settings")]
    [Tooltip("Degrees left-right (like real cars)")]
    public float maxWheelRotation = 450f;  // Degrees left-right (like real cars)
    [Tooltip("How fast it returns visually")]
    public float wheelReturnSpeed = 5f;    // How fast it returns visually
    private float currentVisualRotation = 0f;
    public Transform steeringWheel;        // 3D model of steering wheel
    void SetHeadlights(bool on)
    {
        if (headlightLeft != null) headlightLeft.enabled = on;
        if (headlightRight != null) headlightRight.enabled = on;
    }

    void UpdateSteeringWheel(float steerAngle)
    {
        if (steeringWheel == null) return;

        // Map the steering angle to wheel rotation (like animation)
        float targetRotation = Mathf.Lerp(-maxWheelRotation, maxWheelRotation, (steerAngle + maxSteerAngle) / (2f * maxSteerAngle));
        // Smoothly rotate wheel
        currentVisualRotation = Mathf.Lerp(currentVisualRotation, targetRotation, Time.deltaTime * wheelReturnSpeed);
        steeringWheel.localRotation = Quaternion.Euler(0f, 0f, -currentVisualRotation); // Adjust axis if needed
    }


    // --- Telemetry ---
    [HideInInspector] public float currentSpeedKmh;
    [HideInInspector] public float currentAcceleration;
    [HideInInspector] public float currentTorque;
    [HideInInspector] public float currentSteeringWheelRotation;


}

