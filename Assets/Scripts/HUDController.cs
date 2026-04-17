using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("Main HUD")]
    [SerializeField] private CanvasGroup gameplayGroup;
    [SerializeField] private CanvasGroup gameOverGroup;

    [Header("Top")]
    [SerializeField] private TMP_Text miniGameTitleText;
    [SerializeField] private TMP_Text miniGameTimerText;

    [Header("Bottom progress")]
    [SerializeField] private Image procrastinationFill;

    [Header("Optional result flash")]
    [SerializeField] private TMP_Text resultText;

    public void ShowGameplay()
    {
        SetGroup(gameplayGroup, true);
        SetGroup(gameOverGroup, false);
    }

    public void ShowGameOver()
    {
        SetGroup(gameplayGroup, false);
        SetGroup(gameOverGroup, true);
    }

    public void SetMiniGameTitle(string value)
    {
        miniGameTitleText.text = value;
    }

    public void SetMiniGameTimer(float value)
    {
        miniGameTimerText.text = $"{value:0}s";
    }

    public void SetProgress(float normalized)
    {
        procrastinationFill.fillAmount = Mathf.Clamp01(normalized);
    }

    public void PlayMiniGameResult(MiniGameResult result)
    {
        if (resultText == null) return;

        resultText.text = result == MiniGameResult.Win ? "DONE" : "MISSED";
    }

    private void SetGroup(CanvasGroup group, bool visible)
    {
        group.alpha = visible ? 1f : 0f;
        group.interactable = visible;
        group.blocksRaycasts = visible;
    }
}