using TMPro;
using MagicPigGames;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private HorizontalProgressBar procrastinationProgressBar;
    [SerializeField] private RectTransform progressBarParent;

    [Header("Optional result flash")]
    [SerializeField] private TMP_Text resultText;

    [Header("Level Complete Overlay")]
    [SerializeField] private CanvasGroup levelCompleteGroup;
    [SerializeField] private TMP_Text levelCompleteTitleText;
    [SerializeField] private TMP_Text levelCompleteHintText;

    private void Awake()
    {
        EnsureGameplayGroupFromOverlayUI();
        EnsureProgressBar();
        HideLevelComplete();
    }

    public void ShowGameplay()
    {
        SetGroup(gameplayGroup, true);
        SetGroup(gameOverGroup, false);
    }

    public void ShowGameOver()
    {
        if (gameOverGroup != null)
        {
            SetGroup(gameplayGroup, false);
            SetGroup(gameOverGroup, true);
            ShowLevelComplete("GAME OVER", "Tap anywhere to restart");
            return;
        }

        ShowLevelComplete("GAME OVER", "Tap anywhere to restart");
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
        if (levelCompleteTitleText != null)
        {
            levelCompleteTitleText.text = title;
        }

        if (levelCompleteHintText != null)
        {
            levelCompleteHintText.text = hint;
        }

        SetGroup(levelCompleteGroup, true);
    }

    public void HideLevelComplete()
    {
        SetGroup(levelCompleteGroup, false);
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

    private void EnsureProgressBar()
    {
        if (procrastinationProgressBar != null)
        {
            return;
        }

        RectTransform parent = progressBarParent;
        if (parent == null)
        {
            Transform topBar = transform.Find("GameplayGroup/TopBar") ?? transform.Find("TopBar");
            if (topBar != null)
            {
                parent = topBar as RectTransform;
            }
        }

        if (parent == null)
        {
            return;
        }

        GameObject root = new("ProcrastinationProgressBar", typeof(RectTransform), typeof(Image), typeof(HorizontalProgressBar));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.SetParent(parent, false);
        rootRect.anchorMin = new Vector2(0.5f, 1f);
        rootRect.anchorMax = new Vector2(0.5f, 1f);
        rootRect.pivot = new Vector2(0.5f, 1f);
        rootRect.anchoredPosition = new Vector2(0f, -84f);
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
