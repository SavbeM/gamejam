using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("Main HUD")]
    [SerializeField] private CanvasGroup gameplayGroup;
    [SerializeField] private CanvasGroup gameOverGroup;

    [Header("Top")]
    [SerializeField] private TMP_Text miniGameTitleText;

    [Header("Bottom progress")]
    [SerializeField] private Image procrastinationFill;

    [Header("Optional result flash")]
    [SerializeField] private TMP_Text resultText;

    [Header("Level Complete Overlay")]
    [SerializeField] private CanvasGroup levelCompleteGroup;
    [SerializeField] private TMP_Text levelCompleteTitleText;
    [SerializeField] private TMP_Text levelCompleteHintText;

    private void Awake()
    {
        HideLevelComplete();
    }

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
        if (miniGameTitleText == null)
            return;

        miniGameTitleText.text = value ?? string.Empty;
    }

    public void SetProgress(float normalized)
    {
        if (procrastinationFill == null)
            return;

        procrastinationFill.fillAmount = Mathf.Clamp01(normalized);
    }

    public void PlayMiniGameResult(MiniGameResult result)
    {
        if (resultText == null)
            return;

        resultText.text = result == MiniGameResult.Win ? "DONE" : "MISSED";
    }

    public void ShowLevelComplete(string title = "LEVEL CLEARED", string hint = "Swipe to next level")
    {
        Debug.Log("ShowLevelComplete called");

        if (levelCompleteTitleText != null)
            levelCompleteTitleText.text = title;
        else
            Debug.LogError("levelCompleteTitleText is NULL");

        if (levelCompleteHintText != null)
            levelCompleteHintText.text = hint;
        else
            Debug.LogError("levelCompleteHintText is NULL");

        if (levelCompleteGroup == null)
        {
            Debug.LogError("levelCompleteGroup is NULL");
            return;
        }

        SetGroup(levelCompleteGroup, true);
    }

    public void HideLevelComplete()
    {
        SetGroup(levelCompleteGroup, false);
    }

    private void SetGroup(CanvasGroup group, bool visible)
    {
        if (group == null)
            return;

        group.alpha = visible ? 1f : 0f;
        group.interactable = visible;
        group.blocksRaycasts = visible;

        Debug.Log($"SetGroup: {(visible ? "Showing" : "Hiding")} {group.alpha} {group.name}");
    }
}