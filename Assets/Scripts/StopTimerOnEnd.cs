using UnityEngine;

public class StopTimerOnEnd : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Reached End!");
        if (other.CompareTag("Vehicle"))
        {
            //Debug.Log("Reached End Vehicle!");
            XRCarTuningUI.Instance.timerRunning = false;
        }
    }
}
