using System.Collections;
using UnityEngine;

public class CarZoneTransition : MonoBehaviour
{
    [Header("Transition Settings")]
    public float flyHeight = 3f;          // height of the jump arc
    public float transitionDuration = 2f; // seconds to fly over
    public string zoneTag = "TransitionZone";
    public Transform landing;

    [Header("Debug")]
    public bool showGizmos = true;        // show flight arc for visual debug

    private Rigidbody rb;
    private bool isFlying = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isFlying) return; // avoid multiple triggers

        if (other.CompareTag(zoneTag))
        {
            // Find landing point
            //Transform landing = GameObject.Find("SafeLandingPoint")?.transform;
            //Transform landing = GameObject.Find("SafeLandingPoint")?.transform;
            if (landing != null)
            {
                StartCoroutine(FlyOverSection(landing.position, transitionDuration));
            }
            else
            {
                Debug.LogWarning("No 'SafeLandingPoint' found in the scene!");
            }
        }
    }

    private IEnumerator FlyOverSection(Vector3 destination, float duration)
    {
        isFlying = true;
        Vector3 start = transform.position;
        float elapsed = 0f;

        rb.isKinematic = true; // disable physics

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Smooth arc movement
            Vector3 midPos = Vector3.Lerp(start, destination, t);
            midPos.y += Mathf.Sin(t * Mathf.PI) * flyHeight;
            transform.position = midPos;

            yield return null;
        }

        rb.isKinematic = false; // re-enable physics
        isFlying = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * flyHeight, 0.5f);
    }
}
