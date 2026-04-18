using TMPro;
using MagicPigGames;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    private enum OverlayStage
    {
        Hidden,
        LevelComplete,
        GameOver,
        GameFinished
    }

    [Header("Main HUD")]
    [SerializeField] private CanvasGroup gameplayGroup;
    [SerializeField] private CanvasGroup gameOverGroup;
    [SerializeField] private CanvasGroup gameplayGroupPrefab;

    [Header("Top")]
    [SerializeField] private TMP_Text miniGameTitleText;
    [SerializeField] private TMP_Text miniGameTimerText;

    [Header("Bottom progress")]
    [SerializeField] private Image procrastinationFill;
    [SerializeField] private HorizontalProgressBar procrastinationProgressBar;
    [SerializeField] private HorizontalProgressBar procrastinationProgressBarPrefab;
    [SerializeField] private RectTransform bottomMetaGroup;
    [SerializeField] private RectTransform progressBarParent;

    [Header("Optional result flash")]
    [SerializeField] private TMP_Text resultText;

    [Header("Level Complete Overlay")]
    [SerializeField] private CanvasGroup levelCompleteGroup;
    [SerializeField] private TMP_Text levelCompleteTitleText;
    [SerializeField] private TMP_Text levelCompleteHintText;
    [SerializeField] private Image levelCompleteBackground;

    private void Awake()
    {
        EnsureGameplayGroupReference();
        EnsureOverlayDependencies();
        EnsureProgressBar();
        HideLevelComplete();
    }

    public void ShowGameplay()
    {
        SetGroup(gameplayGroup, true);
        ApplyOverlayStage(OverlayStage.Hidden);
    }

    public void ShowGameOver()
    {
        if (gameOverGroup != null)
        {
            SetGroup(gameplayGroup, false);
            SetGroup(gameOverGroup, true);
        }

        ApplyOverlayStage(OverlayStage.GameOver);
    }

    public void ShowGameFinished()
    {
        SetGroup(gameplayGroup, false);
        ApplyOverlayStage(OverlayStage.GameFinished);
    }

    public void SetMiniGameTitle(string value)
    {
        if (miniGameTitleText == null)
        {
            return;
        }

        miniGameTitleText.text = value ?? string.Empty;
    }

    public void SetMiniGameTimer(float value)
    {
        if (miniGameTimerText == null)
        {
            return;
        }

        miniGameTimerText.text = $"{Mathf.Max(0f, value):0}s";
    }

    public void SetProgress(float normalized)
    {
        if (procrastinationProgressBar != null)
        {
            procrastinationProgressBar.SetProgress(Mathf.Clamp01(normalized));
        }

        if (procrastinationFill == null)
        {
            return;
        }

        procrastinationFill.fillAmount = Mathf.Clamp01(normalized);
    }

    public void PlayMiniGameResult(MiniGameResult result)
    {
        if (resultText == null)
        {
            return;
        }

        resultText.text = result == MiniGameResult.Win ? "DONE" : "MISSED";
    }

    public void ShowLevelComplete(string title = "LEVEL CLEARED", string hint = "Press ↑ or ↓ to next level")
    {
        ApplyOverlayStage(OverlayStage.LevelComplete, title, hint);
    }

    public void HideLevelComplete()
    {
        ApplyOverlayStage(OverlayStage.Hidden);
    }

    private void EnsureGameplayGroupReference()
    {
        if (gameplayGroup != null)
        {
            return;
        }

        RectTransform overlayTransform = transform as RectTransform;
        if (overlayTransform == null)
        {
            return;
        }

        Transform existingGroupTransform = overlayTransform.Find("GameplayGroup");
        if (existingGroupTransform != null)
        {
            gameplayGroup = existingGroupTransform.GetComponent<CanvasGroup>();
        }

        if (gameplayGroup != null)
        {
            return;
        }

        if (gameplayGroupPrefab != null)
        {
            CanvasGroup instantiatedGroup = Instantiate(gameplayGroupPrefab, overlayTransform);
            instantiatedGroup.name = "GameplayGroup";
            RectTransform gameplayGroupRect = instantiatedGroup.transform as RectTransform;
            if (gameplayGroupRect != null)
            {
                gameplayGroupRect.anchorMin = Vector2.zero;
                gameplayGroupRect.anchorMax = Vector2.one;
                gameplayGroupRect.offsetMin = Vector2.zero;
                gameplayGroupRect.offsetMax = Vector2.zero;
                gameplayGroupRect.SetSiblingIndex(0);
            }

            gameplayGroup = instantiatedGroup;
            return;
        }

        Debug.LogWarning("HUDController could not resolve GameplayGroup. Assign gameplayGroup or gameplayGroupPrefab in the inspector.", this);
    }

    private void EnsureOverlayDependencies()
    {
        RectTransform overlayTransform = transform as RectTransform;
        if (overlayTransform == null)
        {
            return;
        }

        if (levelCompleteGroup == null)
        {
            Transform levelOverlay = overlayTransform.Find("LevelCompleteOverlay");
            if (levelOverlay != null)
            {
                levelCompleteGroup = levelOverlay.GetComponent<CanvasGroup>();
            }
        }

        if (levelCompleteGroup == null)
        {
            return;
        }

        if (gameOverGroup == null)
        {
            gameOverGroup = levelCompleteGroup;
        }

        if (levelCompleteBackground == null)
        {
            levelCompleteBackground = levelCompleteGroup.GetComponent<Image>();
        }

        if (levelCompleteTitleText == null)
        {
            levelCompleteTitleText = levelCompleteGroup.GetComponentInChildren<TMP_Text>(true);
        }

        if (levelCompleteHintText == null)
        {
            TMP_Text[] allTexts = levelCompleteGroup.GetComponentsInChildren<TMP_Text>(true);
            foreach (TMP_Text textComponent in allTexts)
            {
                if (textComponent == levelCompleteTitleText)
                {
                    continue;
                }

                levelCompleteHintText = textComponent;
                break;
            }
        }
    }

    private void EnsureProgressBar()
    {
        if (procrastinationProgressBar != null)
        {
            return;
        }

        RectTransform resolvedBottomMeta = ResolveBottomMetaGroup();
        RectTransform parent = progressBarParent != null ? progressBarParent : resolvedBottomMeta;
        if (parent == null)
        {
            parent = transform as RectTransform;
        }

        if (parent == null)
        {
            return;
        }

        HorizontalProgressBar existingBar = parent.GetComponentInChildren<HorizontalProgressBar>(true);
        if (existingBar != null)
        {
            procrastinationProgressBar = existingBar;
            return;
        }

        if (procrastinationProgressBarPrefab == null)
        {
            Debug.LogWarning("HUDController could not resolve ProcrastinationProgressBar. Assign procrastinationProgressBar or procrastinationProgressBarPrefab in the inspector.", this);
            return;
        }

        procrastinationProgressBar = Instantiate(procrastinationProgressBarPrefab, parent);
        procrastinationProgressBar.name = "ProcrastinationProgressBar";

        RectTransform rootRect = procrastinationProgressBar.transform as RectTransform;
        if (rootRect != null)
        {
            rootRect.SetAsLastSibling();
        }
    }

    private RectTransform ResolveBottomMetaGroup()
    {
        if (bottomMetaGroup != null)
        {
            return bottomMetaGroup;
        }

        Transform resolved = transform.Find("BottomMeta");
        if (resolved == null)
        {
            resolved = transform.Find("GameplayGroup/BottomMeta");
        }

        bottomMetaGroup = resolved as RectTransform;
        return bottomMetaGroup;
    }

    private void ApplyOverlayStage(OverlayStage stage, string title = null, string hint = null)
    {
        if (stage == OverlayStage.Hidden)
        {
            SetGroup(levelCompleteGroup, false);
            return;
        }

        EnsureOverlayDependencies();
        SetGroup(levelCompleteGroup, true);

        string resolvedTitle = title;
        string resolvedHint = hint;
        Color backgroundColor = new Color(0f, 0f, 0f, 0.68f);

        switch (stage)
        {
            case OverlayStage.LevelComplete:
                resolvedTitle ??= "LEVEL CLEARED";
                resolvedHint ??= "Press ↑ or ↓ to next level";
                backgroundColor = new Color(0.04f, 0.12f, 0.2f, 0.72f);
                break;
            case OverlayStage.GameOver:
                resolvedTitle ??= "GAME OVER";
                resolvedHint ??= "Tap anywhere to restart";
                backgroundColor = new Color(0.2f, 0.05f, 0.07f, 0.75f);
                break;
            case OverlayStage.GameFinished:
                resolvedTitle ??= "GAME FINISHED";
                resolvedHint ??= "Tap anywhere to play again";
                backgroundColor = new Color(0.05f, 0.15f, 0.08f, 0.75f);
                break;
        }

        if (levelCompleteTitleText != null)
        {
            levelCompleteTitleText.text = resolvedTitle;
        }

        if (levelCompleteHintText != null)
        {
            levelCompleteHintText.text = resolvedHint;
        }

        if (levelCompleteBackground != null)
        {
            levelCompleteBackground.color = backgroundColor;
        }
    }

    private static void SetGroup(CanvasGroup group, bool visible)
    {
        if (group == null)
        {
            return;
        }

        group.alpha = visible ? 1f : 0f;
        group.interactable = visible;
        group.blocksRaycasts = visible;
    }
}
