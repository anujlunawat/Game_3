using UnityEngine;

public class FacePlayer : MonoBehaviour
{
    public Transform playerCamera;
    public float distance = 2f;

    void Start()
    {
        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main.transform;
    }

    void Update()
    {
        if (playerCamera == null) return;

        // Make the menu always face the camera
        transform.LookAt(playerCamera);
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + 180f, 0);

        // Keep menu a fixed distance from the player
        transform.position = playerCamera.position + playerCamera.forward * distance;
    }
}
