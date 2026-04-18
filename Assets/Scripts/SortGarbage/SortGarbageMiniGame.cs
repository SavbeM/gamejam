using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SortGarbageMiniGame : MiniGameBase
{
    [Header("Data")]
    [SerializeField] private List<SortGarbageItemData> items = new();

    [Header("Refs")]
    [SerializeField] private SortGarbageItemView itemView;
    [SerializeField] private TMP_Text leftLabelText;
    [SerializeField] private TMP_Text rightLabelText;
    [SerializeField] private Image leftZoneHighlight;
    [SerializeField] private Image rightZoneHighlight;

    [Header("Visual")]
    [SerializeField] private string leftLabel = "Inedible";
    [SerializeField] private string rightLabel = "Edible";

    private SortGarbageItemData currentItem;

    private int rightCount;

    public override void Setup(System.Action<MiniGameResult> onFinished)
    {
        base.Setup(onFinished);
        Debug.Log("[SortGarbage] Setup started.");

        if (items == null || items.Count == 0)
        {
            Debug.LogError("SortGarbageMiniGame: items list is empty.");
            return;
        }

        if (leftLabelText != null)
            leftLabelText.text = leftLabel;

        if (rightLabelText != null)
            rightLabelText.text = rightLabel;

        SetZoneAlpha(leftZoneHighlight, 0.18f);
        SetZoneAlpha(rightZoneHighlight, 0.18f);

        SpawnNewItem();
    }

    public override void Cleanup()
    {
        if (itemView != null)
            itemView.Swiped -= HandleSwiped;

        base.Cleanup();
    }

    private void SpawnNewItem()
    {
        if (items == null || items.Count == 0)
            return;

        currentItem = items[Random.Range(0, items.Count)];

        if (itemView != null)
        {
            itemView.ResetView();
            itemView.SetData(currentItem.Icon, currentItem.DisplayName);
            itemView.Swiped -= HandleSwiped;
            itemView.Swiped += HandleSwiped;
        }

        Debug.Log($"[SortGarbage] New item spawned: {currentItem.DisplayName}.");
    }

    private void HandleSwiped(SortCategory selectedCategory)
    {
        if (!IsRunning || IsFinished || currentItem == null)
            return;

        Debug.Log($"[SortGarbage] Swipe input received. Selected={selectedCategory}, Expected={currentItem.CorrectCategory}.");
        bool isCorrect = selectedCategory == currentItem.CorrectCategory;

        if (selectedCategory == SortCategory.Inedible)
        {
            SetZoneAlpha(leftZoneHighlight, 0.45f);
        }
        else
        {
            SetZoneAlpha(rightZoneHighlight, 0.45f);
        }

        if (!isCorrect)
        {
            RestartGame();
            return;
        }else
        {
            rightCount++;
            if (rightCount >= 5)
            {
                Debug.Log("Player sorted 5 items correctly, finishing mini game...");
                Finish(MiniGameResult.Win);
            }
        }

        SpawnNewItem();
    }

    private static void SetZoneAlpha(Image image, float alpha)
    {
        if (image == null)
            return;

        Color color = image.color;
        color.a = Mathf.Clamp01(alpha);
        image.color = color;
    }

    private void RestartGame()
    {
        if (!IsRunning || IsFinished)
            return;

        Debug.Log("Restarting mini game...");

        SetZoneAlpha(leftZoneHighlight, 0.18f);
        SetZoneAlpha(rightZoneHighlight, 0.18f);

        SpawnNewItem();
    }
}
