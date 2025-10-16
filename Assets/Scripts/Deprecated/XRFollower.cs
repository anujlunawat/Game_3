using UnityEngine;

public class XRFollower : MonoBehaviour
{
    //[Tooltip("Assign the SeatAnchor transform inside the car (driver position)")]
    //public Transform target;
    //[Tooltip("Higher = snappier")]
    //public float positionSmooth = 10f;
    //public float rotationSmooth = 10f;

    //void LateUpdate()
    //{
    //    if (target == null) return;

    //    // exponential smoothing (frame-rate independent)
    //    float tPos = 1f - Mathf.Exp(-positionSmooth * Time.deltaTime);
    //    float tRot = 1f - Mathf.Exp(-rotationSmooth * Time.deltaTime);

    //    transform.position = Vector3.Lerp(transform.position, target.position, tPos);
    //    transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, tRot);
    //}

    [Tooltip("Assign the SeatAnchor transform inside the car (driver position)")]
    public Transform target;       // assign your car here
    public float smoothTime = 0.2f; // how quickly camera follows
    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        // Smoothly follow car position (optional offset)
        transform.position = Vector3.SmoothDamp(transform.position, target.position, ref velocity, smoothTime);

        // Smoothly align rotation (yaw only)
        Quaternion targetRot = Quaternion.Euler(0, target.eulerAngles.y, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 2f);
    }
}

