using System;

public enum MiniGameResult
{
    None,
    Win,
    Fail
}

public interface IMiniGame
{
    void Setup(Action<MiniGameResult> onFinished);
    void Begin();
    void Cleanup();
}