using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CheckpointManagerWithComparison : MonoBehaviour
{
    [Header("Setup")]
    public Transform checkpointsParent;
    public TextMeshProUGUI checkpointText;
    public TextMeshProUGUI comparisonText;
    public AudioSource checkpointChime;

    [Header("Animation Settings")]
    public float textPopDuration = 0.3f;
    public float comparisonDisplayTime = 1.5f;

    private List<Transform> checkpoints = new List<Transform>();
    private int currentCheckpoint = 0;

    private List<float> currentLapTimes = new List<float>();
    private List<float> previousLapTimes = new List<float>();

    private float lapStartTime;
    private bool comparisonActive = false;

    void Start()
    {
        foreach (Transform child in checkpointsParent)
            checkpoints.Add(child);

        lapStartTime = Time.time;
        UpdateCheckpointText();
    }

    void UpdateCheckpointText()
    {
        checkpointText.text = $"Checkpoint: {currentCheckpoint}/{checkpoints.Count}";
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if we hit a checkpoint
        int index = checkpoints.IndexOf(other.transform);
        if (index == -1 || index != currentCheckpoint) return;

        float checkpointTime = Time.time - lapStartTime;
        currentLapTimes.Add(checkpointTime);

        // Compare with previous lap
        if (previousLapTimes.Count > index)
        {
            float diff = checkpointTime - previousLapTimes[index];
            ShowComparison(diff);
        }

        // Play sound
        checkpointChime?.Play();

        // Animate checkpoint text (pop)
        StopAllCoroutines();
        StartCoroutine(AnimateCheckpointText());

        // Move to next checkpoint
        currentCheckpoint++;
        UpdateCheckpointText();

        // If lap complete
        if (currentCheckpoint >= checkpoints.Count)
        {
            Debug.Log("Lap Complete!");
            previousLapTimes = new List<float>(currentLapTimes);
            currentLapTimes.Clear();
            currentCheckpoint = 0;
            lapStartTime = Time.time;
        }
    }

    void ShowComparison(float diff)
    {
        comparisonText.color = diff < 0 ? Color.green : Color.red;
        comparisonText.text = diff < 0 ?
            $"-{Mathf.Abs(diff):F2}s faster!" :
            $"+{diff:F2}s slower!";

        if (!comparisonActive)
            StartCoroutine(ShowComparisonCoroutine());
    }

    System.Collections.IEnumerator ShowComparisonCoroutine()
    {
        comparisonActive = true;
        comparisonText.alpha = 1;
        yield return new WaitForSeconds(comparisonDisplayTime);

        // Fade out
        for (float t = 1; t >= 0; t -= Time.deltaTime * 2)
        {
            comparisonText.alpha = t;
            yield return null;
        }

        comparisonText.alpha = 0;
        comparisonActive = false;
    }

    System.Collections.IEnumerator AnimateCheckpointText()
    {
        Vector3 originalScale = checkpointText.rectTransform.localScale;
        Vector3 bigScale = originalScale * 1.2f;

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / textPopDuration;
            checkpointText.rectTransform.localScale = Vector3.Lerp(originalScale, bigScale, Mathf.Sin(t * Mathf.PI));
            yield return null;
        }

        checkpointText.rectTransform.localScale = originalScale;
    }
}
