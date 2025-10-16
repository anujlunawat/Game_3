using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class XRCarTuningUI : MonoBehaviour
{
    [Header("Car Reference")]
    public XRWheelCarControllerXR car;

    //[Header("UI Controls")]
    //public Slider torqueSlider;
    //public Slider brakeSlider;
    //public Slider steerSlider;
    //public Slider antiRollSlider;

    [Header("Telemetry Display")]
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI accelText;
    public TextMeshProUGUI torqueText;
    public TextMeshProUGUI steerAngleText;
    public TextMeshProUGUI timerText;


    // Timer variables
    private float elapsedTime = 0f;
    public bool timerRunning = true; // set to false if you want to start later

    public static XRCarTuningUI Instance { get; private set; }

    private void Awake()
    {
        //// Enforce Singleton Pattern
        //if (Instance != null && Instance != this)
        //{
        //    Destroy(gameObject);
        //    return;
        //}
        Instance = this;
        //DontDestroyOnLoad(gameObject); // Optional: persists between scenes
    }
    void Start()
    {
        if (car == null)
        {
            var vehicle = GameObject.FindGameObjectWithTag("Vehicle");
            if (vehicle != null)
                car = vehicle.GetComponent<XRWheelCarControllerXR>();
        }

        if (car == null)
        {
            Debug.LogError("XRCarTuningUI: No car reference found!");
            return;
        }

        //// Initialize sliders with car's current tuning values
        //torqueSlider.value = car.maxMotorTorque;
        //brakeSlider.value = car.brakeTorque;
        //steerSlider.value = car.maxSteerAngle;
        //antiRollSlider.value = car.antiRoll;

        //// Hook up sliders to live tuning
        //torqueSlider.onValueChanged.AddListener(v => car.maxMotorTorque = v);
        //brakeSlider.onValueChanged.AddListener(v => car.brakeTorque = v);
        //steerSlider.onValueChanged.AddListener(v => car.maxSteerAngle = v);
        //antiRollSlider.onValueChanged.AddListener(v => car.antiRoll = v);
    }

    [Header("UI Update Settings")]
    public float updateInterval = 0.2f; // Update UI every 0.2 seconds (5 times/sec)
    private float updateTimer;

    void Update()
    {
        if (car == null) return;
        updateTimer += Time.deltaTime;

        // --- Timer update ---
        if (timerRunning)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerDisplay();
        }

        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;
            // Telemetry updates
            speedText.text = $"Speed: {car.currentSpeedKmh:F0} km/h";
            accelText.text = $"Accel: {car.currentAcceleration:F0} m/s²";
            torqueText.text = $"Torque: {car.currentTorque:F0} Nm";
            steerAngleText.text = $"Steer Angle: {car.currentSteeringWheelRotation:F0}";
        }
    }

    void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        float seconds = elapsedTime % 60f;
        timerText.text = $"Time: {minutes:00}:{seconds:00.0}";
    }

    // 👇 Optional controls
    public void ResetTimer()
    {
        elapsedTime = 0f;
        UpdateTimerDisplay();
    }
}
