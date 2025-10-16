using UnityEngine;
using System.Collections;
using System.Linq;

public class VoiceoverController : MonoBehaviour
{
    public static VoiceoverController Instance { get; private set; }
    public AudioSource voiceoverSource; // Assign your voiceover AudioSource here
    [Tooltip("Assign Voiceover Source")]
    private AudioSource[] allAudioSources;

    private void Awake()
    {
        // Enforce Singleton Pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional: persists between scenes
    }

    void Start()
    {
        // Find all audio sources in the scene (excluding the voiceover)
        allAudioSources = GameObject.FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Where(a => a != voiceoverSource).ToArray();
    }

    public void PlayVoiceover(AudioClip clip)
    {
        StartCoroutine(PlayVoiceoverRoutine(clip));
    }

    private IEnumerator PlayVoiceoverRoutine(AudioClip clip)
    {
        // Pause gameplay
        Time.timeScale = 0f;

        // Pause all other sounds
        foreach (AudioSource a in allAudioSources)
            a.Pause();

        // Play voiceover
        voiceoverSource.clip = clip;
        voiceoverSource.Play();

        // Wait until voiceover finishes (real time)
        yield return new WaitForSecondsRealtime(clip.length);

        // Resume gameplay
        Time.timeScale = 1f;

        // Resume all other sounds
        foreach (AudioSource a in allAudioSources)
            a.UnPause();
    }
}
