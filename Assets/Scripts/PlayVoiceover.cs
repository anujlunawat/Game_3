using UnityEngine;

public class PlayVoiceover : MonoBehaviour
{
    string vehicleTag = "Vehicle";
    public AudioClip clip;
    public string subtitleFile;
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
        if (clip == null) return;
        if (other.CompareTag(vehicleTag))
        {
            SubtitleManager.Instance.ShowSubtitles(subtitleFile);
            VoiceoverController.Instance.PlayVoiceover(clip);
        }
    }

}
