using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class WatchHUD : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text timerText;

    [Header("Progress")]
    [SerializeField] private int totalToEscape = 8;
    [SerializeField] private int progress = 0;

    [Header("Timer")]
    [SerializeField] private float startSeconds = 300f; // 5 minutes
    [SerializeField] private bool timerRuns = true;

    [Header("Events")]
    [SerializeField] private UnityEvent onLose; // hook up "player loses" here

    private float remaining;
    private bool lost;

    private void Awake()
    {
        remaining = startSeconds;
        RefreshUI();
    }

    private void Update()
    {
        if (!timerRuns || lost) return;

        remaining -= Time.deltaTime;
        if (remaining <= 0f)
        {
            remaining = 0f;
            lost = true;
            RefreshUI();
            onLose?.Invoke();
            return;
        }

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (progressText != null)
            progressText.text = $"Progress: {progress}/{totalToEscape}";

        if (timerText != null)
            timerText.text = $"Time: {FormatTime(remaining)}";
    }

    private string FormatTime(float seconds)
    {
        int s = Mathf.CeilToInt(seconds);
        int m = s / 60;
        int r = s % 60;
        return $"{m:00}:{r:00}";
    }

    // Call this when player unlocks progress (e.g., puzzle solved)
    public void AddProgress(int amount = 1)
    {
        if (lost) return;
        progress = Mathf.Clamp(progress + amount, 0, totalToEscape);
        RefreshUI();
    }

    // Optional: reset/restart
    public void ResetTimer()
    {
        lost = false;
        remaining = startSeconds;
        RefreshUI();
    }

    public void StopTimer() => timerRuns = false;
    public void StartTimer() => timerRuns = true;
}
