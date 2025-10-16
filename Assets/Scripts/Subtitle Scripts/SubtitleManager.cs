using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using System.Text.RegularExpressions;

public class SubtitleManager : MonoBehaviour
{
    public static SubtitleManager Instance { get; private set; }
    public TextMeshProUGUI subtitleText;
    public CanvasGroup subtitleCanvasGroup;

    private Coroutine runningCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ShowSubtitles(string srtFileName)
    {
        string path = Path.Combine(Application.streamingAssetsPath, srtFileName);
        string[] lines = File.ReadAllLines(path);

        // Parse SRT file
        List<(float start, float end, string text)> subtitles = new();
        Regex timeRegex = new Regex(@"(\d{2}):(\d{2}):(\d{2}),(\d{3}) --> (\d{2}):(\d{2}):(\d{2}),(\d{3})");

        for (int i = 0; i < lines.Length; i++)
        {
            var match = timeRegex.Match(lines[i]);
            if (match.Success)
            {
                float start = TimeToSeconds(match, 1);
                float end = TimeToSeconds(match, 5);
                string text = lines[i + 1];
                subtitles.Add((start, end, text));
            }
        }

        if (runningCoroutine != null)
            StopCoroutine(runningCoroutine);
        runningCoroutine = StartCoroutine(PlaySubtitles(subtitles));
    }

    private float TimeToSeconds(Match m, int groupIndex)
    {
        int h = int.Parse(m.Groups[groupIndex].Value);
        int min = int.Parse(m.Groups[groupIndex + 1].Value);
        int s = int.Parse(m.Groups[groupIndex + 2].Value);
        int ms = int.Parse(m.Groups[groupIndex + 3].Value);
        return h * 3600 + min * 60 + s + ms / 1000f;
    }

    IEnumerator PlaySubtitles(List<(float start, float end, string text)> subs)
    {
        subtitleCanvasGroup.alpha = 0;
        subtitleText.text = "";

        float startTime = Time.realtimeSinceStartup;

        foreach (var sub in subs)
        {
            float waitStart = sub.start - (Time.realtimeSinceStartup - startTime);
            if (waitStart > 0)
                yield return new WaitForSecondsRealtime(waitStart);

            subtitleText.text = sub.text;
            subtitleCanvasGroup.alpha = 1;

            float displayTime = sub.end - sub.start;
            yield return new WaitForSecondsRealtime(displayTime);

            subtitleCanvasGroup.alpha = 0;
            subtitleText.text = "";
        }
    }
}
