using UnityEngine;

public abstract class TimedMiniGameBase : MiniGameBase
{
    [SerializeField] protected float duration = 10f;
    protected float timeLeft;

    public float Duration => duration;
    public float TimeLeft => timeLeft;

    public void SetDuration(float value)
    {
        duration = Mathf.Max(0.1f, value);
    }

    public override void Begin()
    {
        base.Begin();
        timeLeft = duration;
    }

    protected virtual void Update()
    {
        if (!IsRunning || IsFinished)
        {
            return;
        }

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            OnTimeExpired();
        }
    }

    protected virtual void OnTimeExpired()
    {
        Finish(MiniGameResult.Fail);
    }

    public float GetNormalizedTime()
    {
        if (duration <= 0f)
        {
            return 0f;
        }

        return timeLeft / duration;
    }
}
