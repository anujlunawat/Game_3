using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class XRPauseMenu : MonoBehaviour
{
    [Header("Input (assign InputActionProperty Pause action)")]
    public InputActionProperty pauseAction;

    [Header("Menu Panels (assign in inspector)")]
    public GameObject pauseMenuPanel;   // root panel (Canvas root or child)
    public GameObject controlsPanel;    // subpanel showing controls (hidden by default)
    public CanvasGroup pauseCanvasGroup; // for fade in/out (must be on the pause menu root)

    [Header("XR / Camera")]
    public Transform xrCameraTransform; // assign your XR camera (Main Camera under XROrigin). fallback to Camera.main

    [Header("Menu placement")]
    public float menuDistance = 2.0f;    // meters in front of camera
    public Vector3 menuOffset = Vector3.zero; // extra offset in local camera space
    public bool faceCamera = true;       // rotate menu to face camera

    [Header("Volume")]
    public Slider volumeSlider;          // assign UI Slider
    public float defaultVolume = 1.0f;   // default global volume

    [Header("VoiceOverPoints parent (optional)")]
    public GameObject voiceOverPointsParent; // parent object that contains voiceover items

    [Header("Fade settings")]
    public float fadeTime = 0.35f;

    // internal state
    private bool isPaused = false;
    private Coroutine fadeCoroutine;
    private AudioSource tempAudioSource; // for one-shot sound if needed

    void Awake()
    {
        // ensure references
        if (xrCameraTransform == null && Camera.main != null)
            xrCameraTransform = Camera.main.transform;

        // Ensure pause panel inactive by default
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (controlsPanel != null)
            controlsPanel.SetActive(false);

        // Setup volume slider: load saved value if available
        if (volumeSlider != null)
        {
            float saved = PlayerPrefs.GetFloat("MasterVolume", defaultVolume);
            AudioListener.volume = saved;
            volumeSlider.value = saved;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
        else
        {
            // If no slider, ensure default volume is set
            AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume", defaultVolume);
        }

        // Add a temporary AudioSource if you want to play UI sounds (optional)
        tempAudioSource = gameObject.AddComponent<AudioSource>();
        tempAudioSource.playOnAwake = false;
        tempAudioSource.spatialBlend = 0f; // UI (2D)
    }

    void OnEnable()
    {
        if (pauseAction != null && pauseAction.action != null)
        {
            pauseAction.action.Enable();
            pauseAction.action.performed += OnPausePerformed;
        }
    }

    void OnDisable()
    {
        if (pauseAction != null && pauseAction.action != null)
        {
            pauseAction.action.performed -= OnPausePerformed;
            pauseAction.action.Disable();
        }
    }

    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        TogglePause();
    }

    /// <summary>Toggle pause state</summary>
    public void TogglePause()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    /// <summary>Pause the game and show the menu in front of the camera</summary>
    public void PauseGame()
    {
        if (isPaused) return;
        isPaused = true;

        // Place menu in front of camera
        PositionMenuInFrontOfCamera();

        // Activate panel and fade in using unscaled time
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        if (pauseCanvasGroup != null)
        {
            pauseCanvasGroup.alpha = 0f;
            fadeCoroutine = StartCoroutine(FadeCanvasGroup(pauseCanvasGroup, 0f, 1f, fadeTime));
        }

        // Pause game: stop time and pause audio
        Time.timeScale = 0f;
        AudioListener.pause = true;

        // Make sure controls panel is hidden on pause entry
        if (controlsPanel != null) controlsPanel.SetActive(false);

        // If you want to unlock cursor on desktop:
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>Resume gameplay and hide the menu</summary>
    public void ResumeGame()
    {
        if (!isPaused) return;
        isPaused = false;

        // Fade out and then deactivate
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        if (pauseCanvasGroup != null)
            fadeCoroutine = StartCoroutine(FadeOutAndDeactivate(pauseCanvasGroup, fadeTime));
        else
        {
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        }

        // Unpause time and audio
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // Hide controls panel
        if (controlsPanel != null) controlsPanel.SetActive(false);

        // Relock cursor (optional)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>Restart: disable voiceoverpoints then reload scene</summary>
    public void RestartGame()
    {
        // Turn off all voiceover points (children of provided parent)
        if (voiceOverPointsParent != null)
        {
            foreach (Transform t in voiceOverPointsParent.transform)
            {
                if (t != null && t.gameObject != null)
                    t.gameObject.SetActive(false);
            }
        }

        // Make sure we unpause before reloading
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>Show the controls panel (subpanel)</summary>
    public void ShowControlsPanel()
    {
        if (controlsPanel != null) controlsPanel.SetActive(true);
    }

    /// <summary>Hide the controls panel</summary>
    public void HideControlsPanel()
    {
        if (controlsPanel != null) controlsPanel.SetActive(false);
    }

    /// <summary>Called when volume slider changes</summary>
    public void OnVolumeChanged(float value)
    {
        // Value expected 0..1
        AudioListener.volume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat("MasterVolume", AudioListener.volume);
        PlayerPrefs.Save();
    }

    private void PositionMenuInFrontOfCamera()
    {
        if (pauseMenuPanel == null || xrCameraTransform == null) return;

        // set position in front of camera
        Vector3 forward = xrCameraTransform.forward;
        Vector3 up = xrCameraTransform.up;
        Vector3 targetPos = xrCameraTransform.position + forward * menuDistance + xrCameraTransform.TransformDirection(menuOffset);
        pauseMenuPanel.transform.position = targetPos;

        if (faceCamera)
        {
            // rotate so it faces the camera (only yaw)
            Vector3 lookDir = (pauseMenuPanel.transform.position - xrCameraTransform.position).normalized;
            // compute rotation but keep it upright (avoid roll)
            Quaternion rot = Quaternion.LookRotation(lookDir, Vector3.up);
            pauseMenuPanel.transform.rotation = rot;
        }
    }

    // Fade using unscaled time
    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        float elapsed = 0f;
        cg.alpha = from;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(from, to, elapsed / Mathf.Max(0.0001f, duration));
            yield return null;
        }
        cg.alpha = to;
        fadeCoroutine = null;
    }

    // Fade out then deactivate panel
    private IEnumerator FadeOutAndDeactivate(CanvasGroup cg, float duration)
    {
        yield return FadeCanvasGroup(cg, cg.alpha, 0f, duration);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        fadeCoroutine = null;
    }
}
