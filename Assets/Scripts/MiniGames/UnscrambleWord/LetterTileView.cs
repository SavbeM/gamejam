using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class LetterTileView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Refs")]
    [SerializeField] private TMP_Text letterText;
    [SerializeField] private Image background;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas rootCanvas;

    private Vector2 originalAnchoredPosition;
    private Transform originalParent;
    private int originalSiblingIndex;
    public LetterSlotView currentSlot;

    public char Letter { get; private set; }
    public event Action OnPlaced;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        rootCanvas = GetComponentInParent<Canvas>();
    }

    public void SetLetter(char c)
    {
        Letter = c;
        if (letterText != null)
            letterText.text = c.ToString();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalAnchoredPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();

        if (currentSlot != null)
        {
            currentSlot.Clear();
            currentSlot = null;
        }

        transform.SetParent(rootCanvas.transform, true);
        // ВАЖНО: выключаем raycast чтобы слоты под нами были видны
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.85f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        float scaleFactor = rootCanvas != null ? rootCanvas.scaleFactor : 1f;
        rectTransform.anchoredPosition += eventData.delta / scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        // Ищем слот под курсором (наш raycast уже включён, ищем через позицию)
        LetterSlotView targetSlot = FindSlotAtPosition(eventData.position);

        if (targetSlot != null)
        {
            if (targetSlot.HasLetter)
            {
                // Swap
                LetterTileView other = targetSlot.Occupant;
                LetterSlotView myOldSlot = currentSlot;

                targetSlot.Clear();
                other.currentSlot = null;

                PlaceInSlot(targetSlot);

                if (myOldSlot != null)
                    other.PlaceInSlot(myOldSlot);
                else
                    other.ReturnToOrigin();
            }
            else
            {
                PlaceInSlot(targetSlot);
            }
        }
        else
        {
            ReturnToOrigin();
        }
    }

    public void PlaceInSlot(LetterSlotView slot)
    {
        currentSlot = slot;
        slot.Accept(this);
        transform.SetParent(slot.transform, false);
        rectTransform.anchoredPosition = Vector2.zero;
        OnPlaced?.Invoke();
    }

    public void ReturnToOrigin()
    {
        currentSlot = null;
        transform.SetParent(originalParent, false);
        transform.SetSiblingIndex(originalSiblingIndex);
        rectTransform.anchoredPosition = originalAnchoredPosition;
    }

    // Ищем слот по позиции мыши через OverlapPoint
    private LetterSlotView FindSlotAtPosition(Vector2 screenPos)
    {
        // Ищем все слоты на сцене
        LetterSlotView[] allSlots = FindObjectsByType<LetterSlotView>(FindObjectsSortMode.None);

        foreach (var slot in allSlots)
        {
            RectTransform slotRect = slot.GetComponent<RectTransform>();
            if (slotRect == null) continue;

            if (RectTransformUtility.RectangleContainsScreenPoint(slotRect, screenPos, rootCanvas.worldCamera))
                return slot;
        }

        return null;
    }
}