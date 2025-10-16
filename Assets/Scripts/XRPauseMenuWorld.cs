using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class XRPauseMenuWorld : MonoBehaviour
{
    [Header("Input")]
    public InputActionProperty pauseAction; // assign Pause action here

    [Header("Menu UI (World Space)")]
    public GameObject pauseMenuRoot;        // PauseMenuCanvas_WS (root canvas or PausePanel)
    public CanvasGroup pauseCanvasGroup;    // CanvasGroup on canvas or panel
    public GameObject controlsPanel;        // the controls subpanel (hidden by default)

    [Header("Audio / Volume")]
    public Slider volumeSlider;             // volume slider UI
    public float defaultVolume = 1f;
    public AudioClip uiOpenSound;           // optional click/chime
    AudioSource uiAudioSource;

    [Header("XR / Positioning")]
    public Transform xrCameraTransform;     // main camera of XR rig (assign)
    public float menuDistance = 1.5f;       // metres in front of camera
    public Vector3 menuOffset = Vector3.zero;
    public bool faceCamera = true;

    [Header("VoiceOverPoints parent (optional)")]
    public GameObject voiceOverPointsParent;

    [Header("Fade")]
    public float fadeTime = 0.35f;

    bool isPaused = false;
    Coroutine fadeCoroutine;

    void Awake()
    {
        // ensure menu hidden initially
        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(false);
        if (pauseCanvasGroup != null)
            pauseCanvasGroup.alpha = 0f;

        // ui audio source that ignores listener pause
        uiAudioSource = gameObject.AddComponent<AudioSource>();
        uiAudioSource.playOnAwake = false;
        uiAudioSource.spatialBlend = 0f;
        uiAudioSource.ignoreListenerPause = true;

        // volume initialization
        float saved = PlayerPrefs.GetFloat("MasterVolume", defaultVolume);
        AudioListener.volume = saved;
        if (volumeSlider != null)
        {
            volumeSlider.value = saved;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        // find camera if not assigned
        if (xrCameraTransform == null && Camera.main != null)
            xrCameraTransform = Camera.main.transform;
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

    public void TogglePause()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        if (isPaused) return;
        isPaused = true;

        // position menu in front of the player
        PositionMenuInFrontOfCamera();

        if (pauseMenuRoot != null) pauseMenuRoot.SetActive(true);
        if (controlsPanel != null) controlsPanel.SetActive(false);

        // fade in (unscaled time)
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        if (pauseCanvasGroup != null)
        {
            pauseCanvasGroup.alpha = 0f;
            fadeCoroutine = StartCoroutine(FadeCanvasGroup(pauseCanvasGroup, 0f, 1f, fadeTime));
        }

        // pause time & audio
        Time.timeScale = 0f;
        //AudioListener.pause = true;

        // optional sound
        if (uiOpenSound != null) uiAudioSource.PlayOneShot(uiOpenSound);
    }

    public void ResumeGame()
    {
        if (!isPaused) return;
        isPaused = false;

        // fade out then disable
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        if (pauseCanvasGroup != null)
            fadeCoroutine = StartCoroutine(FadeOutAndDeactivate(pauseCanvasGroup, fadeTime));
        else
        {
            if (pauseMenuRoot != null) pauseMenuRoot.SetActive(false);
        }

        // unpause
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    public void RestartGame()
    {
        // disable all children under voiceOverPointsParent
        if (voiceOverPointsParent != null)
        {
            foreach (Transform t in voiceOverPointsParent.transform)
            {
                if (t != null && t.gameObject != null)
                    t.gameObject.SetActive(false);
            }
        }

        // unpause and reload
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ShowControls()
    {
        if (controlsPanel != null) controlsPanel.SetActive(true);
    }

    public void HideControls()
    {
        if (controlsPanel != null) controlsPanel.SetActive(false);
    }

    public void OnVolumeChanged(float v)
    {
        float val = Mathf.Clamp01(v);
        AudioListener.volume = val;
        PlayerPrefs.SetFloat("MasterVolume", val);
        PlayerPrefs.Save();
    }

    void PositionMenuInFrontOfCamera()
    {
        if (pauseMenuRoot == null || xrCameraTransform == null) return;

        Vector3 forward = xrCameraTransform.forward;
        Vector3 pos = xrCameraTransform.position + forward * menuDistance + xrCameraTransform.TransformDirection(menuOffset);
        pauseMenuRoot.transform.position = pos;

        if (faceCamera)
        {
            // face toward camera, keep upright
            Vector3 lookDir = (pauseMenuRoot.transform.position - xrCameraTransform.position).normalized;
            Quaternion rot = Quaternion.LookRotation(lookDir, Vector3.up);
            pauseMenuRoot.transform.rotation = rot;
        }
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
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
    }

    IEnumerator FadeOutAndDeactivate(CanvasGroup cg, float duration)
    {
        yield return FadeCanvasGroup(cg, cg.alpha, 0f, duration);
        if (pauseMenuRoot != null) pauseMenuRoot.SetActive(false);
    }
}
