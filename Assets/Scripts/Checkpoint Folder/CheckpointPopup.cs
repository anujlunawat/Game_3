using UnityEngine;
using TMPro;
using System.Collections;

public class CheckpointPopup : MonoBehaviour
{
    public TextMeshProUGUI popupText;
    public float fadeDuration = 0.5f;
    public float displayDuration = 1.5f;

    CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0;
    }

    public void ShowPopup(string message)
    {
        popupText.text = message;
        StopAllCoroutines();
        StartCoroutine(ShowPopupCoroutine());
    }

    IEnumerator ShowPopupCoroutine()
    {
        // Fade In
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;
        }

        // Hold
        yield return new WaitForSeconds(displayDuration);

        // Fade Out
        t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0;
    }
}
