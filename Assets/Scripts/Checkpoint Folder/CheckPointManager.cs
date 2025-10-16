using UnityEngine;
using TMPro;
using System.Collections;

public class CheckpointManager : MonoBehaviour
{
    [Header("References")]
    public GameObject car;                     // Your XR vehicle
    public TextMeshProUGUI checkpointText;     // Top-right text (Checkpoint: X / Y)
    public CheckpointPopup popupEffect;      // Optional: small popup prefab (UI animation)
    public ParticleSystem checkpointFlarePrefab; // Optional: flare particle prefab

    [Header("Checkpoints")]
    private Transform[] checkpoints;
    private int currentCheckpoint = 0;
    private int totalCheckpoints = 0;

    // Add these new fields:
    public AudioClip checkpointChime;   // assign your chime clip
    private AudioSource audioSource;

    void Start()
    {
        // Get all checkpoints
        totalCheckpoints = transform.childCount;
        checkpoints = new Transform[totalCheckpoints];

        for (int i = 0; i < totalCheckpoints; i++)
        {
            checkpoints[i] = transform.GetChild(i);
            // Auto-add trigger script
            CheckpointTrigger trigger = checkpoints[i].gameObject.AddComponent<CheckpointTrigger>();
            trigger.manager = this;
            trigger.checkpointIndex = i;

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.clip = checkpointChime;
        }

        UpdateCheckpointText();
    }

    public void ReachCheckpoint(int index)
    {
        if (index == currentCheckpoint)
        {
            currentCheckpoint++;
            UpdateCheckpointText();

            // play chime sound
            if (audioSource != null && checkpointChime != null)
                audioSource.PlayOneShot(checkpointChime);

            // Show popup
            if (popupEffect != null)
                popupEffect.ShowPopup($"Checkpoint {currentCheckpoint} Reached!");

            // Spawn flare effect
            if (checkpointFlarePrefab != null && checkpoints[index] != null)
                Instantiate(checkpointFlarePrefab, checkpoints[index].position, Quaternion.identity);

            if (currentCheckpoint >= totalCheckpoints)
            {
                popupEffect.ShowPopup("All Checkpoints Cleared!");
                Debug.Log("All checkpoints cleared!");
            }

            // animate the checkpointText scale
            StartCoroutine(AnimateCheckpointText());
        }
    }

    void UpdateCheckpointText()
    {
        if (checkpointText != null)
            checkpointText.text = $"Checkpoint: {currentCheckpoint} / {totalCheckpoints}";
    }

    IEnumerator AnimateCheckpointText()
    {
        if (checkpointText == null)
            yield break;

        Vector3 originalScale = checkpointText.transform.localScale;
        Vector3 targetScale = originalScale * 1.3f; // scale up by 30%
        float duration = 0.2f;

        // scale up
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            checkpointText.transform.localScale = Vector3.Lerp(originalScale, targetScale, t / duration);
            yield return null;
        }

        // scale back down
        t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            checkpointText.transform.localScale = Vector3.Lerp(targetScale, originalScale, t / duration);
            yield return null;
        }

        checkpointText.transform.localScale = originalScale;
    }
}
