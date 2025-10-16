//using UnityEngine;

//[RequireComponent(typeof(Collider))]
//public class CheckpointTrigger : MonoBehaviour
//{
//    [HideInInspector] public CheckpointManager manager;
//    //[HideInInspector] public int checkpointIndex;

//    private void OnTriggerEnter(Collider other)
//    {
//        // only trigger when the car enters
//        if (manager != null && other.gameObject == manager.car)
//        {
//            manager.ReachCheckpoint();
//        }
//    }
//}
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CheckpointTrigger : MonoBehaviour
{
    [HideInInspector] public CheckpointManager manager;
    [HideInInspector] public int checkpointIndex;

    private void OnTriggerEnter(Collider other)
    {
        if (manager != null && other.gameObject == manager.car)
        {

            manager.ReachCheckpoint(checkpointIndex);
        }
    }
}
