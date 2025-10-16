//using Unity.AppUI.UI;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class XRCarCameraEffects : MonoBehaviour
{
    [Header("References")]
    private Rigidbody carRigidbody;    // assign your car's Rigidbody
    public Camera playerCamera;       // assign XR camera (usually Main Camera)

    [Header("FOV Effect Settings")]
    [Tooltip("default FOV when stationary")]
    public float baseFOV = 60f;          // default FOV when stationary
    [Tooltip("max FOV at full speed")]
    public float maxFOV = 90f;           // max FOV at full speed
    [Tooltip("km/h at which FOV reaches max")]
    public float maxSpeedForFOV = 100f;  // km/h at which FOV reaches max
    [Tooltip("how fast FOV transitions")]
    public float fovSmoothSpeed = 5f;    // how fast FOV transitions

    private float currentFOV;

    void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();

        if (playerCamera == null)
            playerCamera = Camera.main; // fallback to main camera

        if (playerCamera != null)
            currentFOV = playerCamera.fieldOfView;
    }

    void Update()
    {
        if (carRigidbody == null || playerCamera == null) return;

        // convert speed from m/s to km/h
        float speedKmh = carRigidbody.linearVelocity.magnitude * 3.6f;

        // map 0..maxSpeedForFOV to 0..1
        float t = Mathf.Clamp01(speedKmh / maxSpeedForFOV);

        // compute target FOV
        float targetFOV = Mathf.Lerp(baseFOV, maxFOV, t);

        // smoothly interpolate
        currentFOV = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime * fovSmoothSpeed);
        playerCamera.fieldOfView = currentFOV;
    }
}
