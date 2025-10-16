using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ControlsImg : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionProperty pauseAction;

    [Header("UI References")]
    public GameObject controlsImg;

    private bool isPaused = false;

    void OnEnable()
    {
        controlsImg.SetActive(false);
        pauseAction.action.performed += OnPausePressed;
        pauseAction.action.Enable();
    }

    void OnDisable()
    {
        pauseAction.action.performed -= OnPausePressed;
        pauseAction.action.Disable();
    }

    private void OnPausePressed(InputAction.CallbackContext ctx)
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    void PauseGame()
    {
        if (controlsImg != null)
            controlsImg.SetActive(true);

        Time.timeScale = 0f;
        isPaused = true;

        // Optional for Desktop (ignore for XR)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Game Paused");
    }

    void ResumeGame()
    {
        if (controlsImg != null)
            controlsImg.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;

        // Optional for Desktop
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("Game Resumed");
    }
}
