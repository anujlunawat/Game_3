using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarAutoDrive : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 10f;
    public float reachDistance = 2f;
    public float steeringForce = 10f;
    public bool loop = false;

    private int currentWaypoint = 0;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (waypoints.Length == 0)
        {
            Debug.LogError("Assign waypoints in the inspector!");
        }
    }

    void FixedUpdate()
    {
        if (waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypoint];
        Vector3 direction = (target.position - transform.position).normalized;

        // Smoothly rotate toward target
        Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, lookRotation, steeringForce * Time.fixedDeltaTime));

        // Apply forward movement (uses physics)
        rb.MovePosition(rb.position + transform.forward * speed * Time.fixedDeltaTime);

        // Check if close enough to target
        float distance = Vector3.Distance(transform.position, target.position);
        if (distance < reachDistance)
        {
            currentWaypoint++;
            if (currentWaypoint >= waypoints.Length)
            {
                if (loop)
                    currentWaypoint = 0;
                else
                    enabled = false; // stop when done
            }
        }
    }

    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }
    }
}
