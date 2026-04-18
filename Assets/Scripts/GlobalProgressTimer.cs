using System;
using UnityEngine;

public class GlobalProgressTimer : MonoBehaviour
{
    [Header("Time Settings")]
    [SerializeField] private float maxTime = 30f;
    [SerializeField] private float startTime = 20f;
    [SerializeField] private float drainPerSecond = 1f;

    [Header("Refs")]
    [SerializeField] private HUDController hudController;

    private float currentTime;
    private bool isRunning;
    private bool isFinished;

    public float MaxTime => maxTime;
    public float StartTime => startTime;
    public float CurrentTime => currentTime;
    public bool IsRunning => isRunning;
    public bool IsFinished => isFinished;
    public float NormalizedTime => maxTime <= 0f ? 0f : Mathf.Clamp01(currentTime / maxTime);

    public event Action TimerEmptied;

    private void Awake()
    {
        maxTime = Mathf.Max(0.01f, maxTime);
        startTime = Mathf.Clamp(startTime, 0f, maxTime);
        currentTime = startTime;

        if (hudController == null)
        {
            hudController = FindFirstObjectByType<HUDController>();
        }

        UpdateHud();
    }

    private void Update()
    {
        if (!isRunning || isFinished)
        {
            return;
        }

        float newTime = currentTime - drainPerSecond * Time.deltaTime;
        SetTimeInternal(newTime);
    }

    public void StartTimer()
    {
        if (isFinished && currentTime <= 0f)
        {
            return;
        }

        isRunning = true;
        UpdateHud();
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResetTimer(bool startImmediately = false)
    {
        currentTime = startTime;
        isFinished = false;
        isRunning = startImmediately;
        UpdateHud();
    }

    public void SetToMax(bool startImmediately = false)
    {
        currentTime = maxTime;
        isFinished = false;
        isRunning = startImmediately;
        UpdateHud();
    }

    public void AddTime(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        currentTime = Mathf.Min(currentTime + amount, maxTime);
        UpdateHud();
    }

    public void RemoveTime(float amount)
    {
        if (amount <= 0f || isFinished)
        {
            return;
        }

        SetTimeInternal(currentTime - amount);
    }

    private void SetTimeInternal(float newTime)
    {
        currentTime = Mathf.Clamp(newTime, 0f, maxTime);
        UpdateHud();

        if (currentTime > 0f)
        {
            return;
        }

        if (isFinished)
        {
            return;
        }

        isFinished = true;
        isRunning = false;
        TimerEmptied?.Invoke();
    }

    private void UpdateHud()
    {
        if (hudController != null)
        {
            hudController.SetProgress(NormalizedTime);
        }
    }
}