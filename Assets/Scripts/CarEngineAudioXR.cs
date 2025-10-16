using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CarEngineAudioXR : MonoBehaviour
{
    [Header("References")]
    public XRWheelCarControllerXR carController; // assign in inspector
    private AudioSource engineAudio;

    [Header("Engine Sound Settings")]
    public float minPitch = 0.8f;       // sound when idle
    public float maxPitch = 2.0f;       // sound at full throttle
    public float minVolume = 0.4f;      // idle volume
    public float maxVolume = 1.0f;      // loudest volume
    public float smoothTime = 0.1f;     // smooth transition speed

    [Header("Throttle Response")]
    public float throttleLerpSpeed = 5f; // how fast it reacts to throttle

    [Header("Engine Rumble (Natural Randomness)")]
    public bool enableRumble = true;      // toggle randomization
    public float rumbleIntensity = 0.03f; // how much to vary pitch (±%)
    public float rumbleSpeed = 10f;       // how fast rumble changes

    private float basePitch;
    private float targetPitch;
    private float targetVolume;
    private float pitchVelocity;
    private float volumeVelocity;
    private float rumbleTimer;

    void Start()
    {
        engineAudio = GetComponent<AudioSource>();

        if (carController == null)
            carController = GetComponent<XRWheelCarControllerXR>();

        if (engineAudio == null)
        {
            Debug.LogError("No AudioSource found! Add one to the car.");
            return;
        }

        engineAudio.loop = true;
        if (!engineAudio.isPlaying)
            engineAudio.Play();
    }

    void Update()
    {
        if (carController == null || engineAudio == null) return;

        // read throttle (moveInput.y = forward/back)
        float throttle = Mathf.Clamp01(Mathf.Abs(carController.leftStickAction.action.ReadValue<Vector2>().y));

        // calculate base pitch and volume
        basePitch = Mathf.Lerp(minPitch, maxPitch, throttle);
        targetVolume = Mathf.Lerp(minVolume, maxVolume, throttle);

        // --- ADD ENGINE RUMBLE ---
        float rumble = 0f;
        if (enableRumble)
        {
            // a soft oscillating random offset
            rumbleTimer += Time.deltaTime * rumbleSpeed;
            rumble = Mathf.PerlinNoise(rumbleTimer, 0.0f) * 2f - 1f;
            rumble *= rumbleIntensity;
        }

        // apply the rumble to the pitch
        targetPitch = basePitch + rumble;

        // smooth transitions
        engineAudio.pitch = Mathf.SmoothDamp(engineAudio.pitch, targetPitch, ref pitchVelocity, smoothTime);
        engineAudio.volume = Mathf.SmoothDamp(engineAudio.volume, targetVolume, ref volumeVelocity, smoothTime);
    }
}
