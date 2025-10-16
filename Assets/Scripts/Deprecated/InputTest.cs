using UnityEngine;

public class InputTest : MonoBehaviour
{
    private PlayerInputActions inputActions;

    void Awake()
    {
        // Create new input actions instance
        inputActions = new PlayerInputActions();
    }

    void OnEnable()
    {
        inputActions.Enable();
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

    void Update()
    {
        // Read joystick or WASD input
        Vector2 moveValue = inputActions.Player.Move.ReadValue<Vector2>();

        Debug.Log($"Joystick: {moveValue}");

    }
}
