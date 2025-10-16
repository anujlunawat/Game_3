using UnityEngine;
using System.Collections;

public class TunnelTeleporter : MonoBehaviour
{
    [Header("Teleport Settings")]
    public Transform destination;
    public bool keepVelocity = true;
    public float delayPhysicsResume = 0.05f; // small delay to let Unity settle colliders
    public float verticalOffset = 0.2f;      // slight lift to avoid ground clipping

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag);
        if (!other.CompareTag("Vehicle")) return;

        Debug.Log("Entered!");
        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;

        StartCoroutine(SmoothTeleport(rb));
    }

    private IEnumerator SmoothTeleport(Rigidbody rb)
    {
        // Save current velocity
        Vector3 storedVelocity = rb.linearVelocity;
        Vector3 storedAngularVelocity = rb.angularVelocity;

        // Temporarily disable physics interactions
        rb.isKinematic = true;

        // Slight offset up to avoid ground penetration
        Vector3 targetPos = destination.position + Vector3.up * verticalOffset;

        // Teleport
        rb.position = targetPos;
        rb.rotation = destination.rotation;

        yield return new WaitForFixedUpdate(); // wait one physics step

        // Re-enable physics
        rb.isKinematic = false;

        // Restore velocity
        if (keepVelocity)
        {
            rb.linearVelocity = destination.forward * storedVelocity.magnitude;
            rb.angularVelocity = storedAngularVelocity;
        }

        Debug.Log("Smooth teleported car without bounce.");
    }
}
