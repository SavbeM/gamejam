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

    [Header("Top")]
    [SerializeField] private TMP_Text miniGameTitleText;
    [SerializeField] private TMP_Text miniGameTimerText;

    [Header("Bottom progress")]
    [SerializeField] private Image procrastinationFill;
    [SerializeField] private HorizontalProgressBar procrastinationProgressBar;
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
        EnsureGameplayGroupFromOverlayUI();
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

    public void ShowLevelComplete(string title = "LEVEL CLEARED", string hint = "Swipe to next level")
    {
        ApplyOverlayStage(OverlayStage.LevelComplete, title, hint);
    }

    public void HideLevelComplete()
    {
        ApplyOverlayStage(OverlayStage.Hidden);
    }

    private void EnsureGameplayGroupFromOverlayUI()
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

        CanvasGroup discoveredLevelCompleteGroup = levelCompleteGroup;
        if (discoveredLevelCompleteGroup == null)
        {
            Transform levelCompleteTransform = overlayTransform.Find("LevelCompleteOverlay");
            if (levelCompleteTransform != null)
            {
                discoveredLevelCompleteGroup = levelCompleteTransform.GetComponent<CanvasGroup>();
            }
        }

        GameObject gameplayGroupObject = new("GameplayGroup", typeof(RectTransform), typeof(CanvasGroup));
        RectTransform gameplayGroupTransform = gameplayGroupObject.GetComponent<RectTransform>();
        gameplayGroupTransform.SetParent(overlayTransform, false);
        gameplayGroupTransform.anchorMin = Vector2.zero;
        gameplayGroupTransform.anchorMax = Vector2.one;
        gameplayGroupTransform.offsetMin = Vector2.zero;
        gameplayGroupTransform.offsetMax = Vector2.zero;

        for (int childIndex = overlayTransform.childCount - 1; childIndex >= 0; childIndex--)
        {
            Transform child = overlayTransform.GetChild(childIndex);
            if (child == gameplayGroupTransform)
            {
                continue;
            }

            if (discoveredLevelCompleteGroup != null && child == discoveredLevelCompleteGroup.transform)
            {
                continue;
            }

            child.SetParent(gameplayGroupTransform, true);
        }

        gameplayGroup = gameplayGroupObject.GetComponent<CanvasGroup>();
        if (gameOverGroup == null)
        {
            gameOverGroup = discoveredLevelCompleteGroup;
        }

        if (levelCompleteGroup == null)
        {
            levelCompleteGroup = discoveredLevelCompleteGroup;
        }
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

        RectTransform parent = progressBarParent;
        if (parent == null)
        {
            Transform fallbackParent = transform.Find("GameplayGroup") ?? transform;
            parent = fallbackParent as RectTransform;
        }

        if (parent == null)
        {
            return;
        }

        RectTransform topBarRect = (transform.Find("GameplayGroup/TopBar") ?? transform.Find("TopBar")) as RectTransform;

        GameObject root = new("ProcrastinationProgressBar", typeof(RectTransform), typeof(Image), typeof(HorizontalProgressBar));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.SetParent(parent, false);
        rootRect.anchorMin = new Vector2(0.5f, 1f);
        rootRect.anchorMax = new Vector2(0.5f, 1f);
        rootRect.pivot = new Vector2(0.5f, 1f);
        rootRect.anchoredPosition = new Vector2(0f, CalculateProgressBarYOffset(parent, topBarRect));
        rootRect.sizeDelta = new Vector2(480f, 22f);

        Image rootImage = root.GetComponent<Image>();
        rootImage.color = new Color(0f, 0f, 0f, 0.55f);
        rootImage.raycastTarget = false;

        GameObject overlay = new("Overlay", typeof(RectTransform), typeof(Image));
        RectTransform overlayRect = overlay.GetComponent<RectTransform>();
        overlayRect.SetParent(rootRect, false);
        overlayRect.anchorMin = new Vector2(1f, 0f);
        overlayRect.anchorMax = new Vector2(1f, 1f);
        overlayRect.pivot = new Vector2(1f, 0.5f);
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        Image overlayImage = overlay.GetComponent<Image>();
        overlayImage.color = new Color(0.15f, 0.95f, 0.45f, 0.95f);
        overlayImage.raycastTarget = false;

        procrastinationProgressBar = root.GetComponent<HorizontalProgressBar>();
        procrastinationProgressBar.rectTransform = rootRect;
        procrastinationProgressBar.overlayBar = overlayRect;
        procrastinationProgressBar.sizeMin = 0f;
        procrastinationProgressBar.sizeMax = 1f;
        procrastinationProgressBar.invertProgress = true;
        procrastinationProgressBar.transitionTime = 0.15f;
    }

    private float CalculateProgressBarYOffset(RectTransform parent, RectTransform topBarRect)
    {
        const float defaultOffset = -112f;
        const float extraSpacing = 18f;

        if (topBarRect == null || parent == null)
        {
            return defaultOffset;
        }

        Vector3[] corners = new Vector3[4];
        topBarRect.GetWorldCorners(corners);
        Vector3 bottomCenter = (corners[0] + corners[3]) * 0.5f;
        Vector3 localBottomCenter = parent.InverseTransformPoint(bottomCenter);
        return localBottomCenter.y - extraSpacing;
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
                resolvedHint ??= "Swipe to next level";
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
