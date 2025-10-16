using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class CarTunnelTeleporter : MonoBehaviour
{
    [Header("Teleport Settings")]
    [Tooltip("Tag of trigger zones that cause teleport (example: TunnelTrigger)")]
    public string teleportTriggerTag = "TunnelTrigger";

    [Tooltip("Assign the destination transform (where car will appear)")]
    public Transform teleportDestination;

    [Tooltip("Keep previous speed and direction after teleport")]
    public bool keepVelocity = true;

    [Tooltip("Slight lift upward after teleport to prevent ground clipping")]
    public float verticalOffset = 0.25f;

    [Tooltip("Delay before physics is resumed (to prevent bounce)")]
    public float physicsResumeDelay = 0.05f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only trigger if we enter a teleport zone
        if (other.CompareTag(teleportTriggerTag) && teleportDestination != null)
        {
            Debug.Log("Entered tunnel teleport trigger.");
            StartCoroutine(TeleportCarSmoothly());
        }
    }

    private IEnumerator TeleportCarSmoothly()
    {
        if (rb == null) yield break;

        // Save speed and angular momentum
        Vector3 storedVelocity = rb.linearVelocity;
        Vector3 storedAngularVelocity = rb.angularVelocity;

        // Disable physics temporarily
        rb.isKinematic = true;

        // Slightly offset up to prevent collision with floor
        Vector3 newPos = teleportDestination.position + Vector3.up * verticalOffset;

        // Move using physics-safe method
        rb.position = newPos;
        rb.rotation = teleportDestination.rotation;

        // Wait a physics frame
        yield return new WaitForFixedUpdate();

        // Re-enable physics
        rb.isKinematic = false;

        // Restore velocity
        if (keepVelocity)
        {
            rb.linearVelocity = teleportDestination.forward * storedVelocity.magnitude;
            rb.angularVelocity = storedAngularVelocity;
        }

        Debug.Log("Teleported car smoothly with preserved velocity.");
    }
}
