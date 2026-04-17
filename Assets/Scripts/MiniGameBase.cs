using System;
using UnityEngine;

public abstract class MiniGameBase : MonoBehaviour, IMiniGame
{
    protected Action<MiniGameResult> OnFinished;
    protected bool IsRunning;
    protected bool IsFinished;

    public virtual void Setup(Action<MiniGameResult> onFinished)
    {
        OnFinished = onFinished;
        IsRunning = false;
        IsFinished = false;
    }

    public virtual void Begin()
    {
        IsRunning = true;
    }

    public virtual void Cleanup()
    {
        IsRunning = false;
    }

    protected void Finish(MiniGameResult result)
    {
        if (IsFinished) return;

        IsFinished = true;
        IsRunning = false;
        OnFinished?.Invoke(result);
    }
}