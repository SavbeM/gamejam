using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class SortGarbageItemView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Refs")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;

    [Header("Swipe")]
    [SerializeField] private float swipeThreshold = 220f;
    [SerializeField] private float maxRotation = 18f;
    [SerializeField] private float returnSpeed = 14f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas rootCanvas;

    private Vector2 initialAnchoredPosition;
    private Quaternion initialRotation;

    private bool isDragging;
    private bool isLocked;

    public event Action<SortCategory> Swiped;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        rootCanvas = GetComponentInParent<Canvas>();

        initialAnchoredPosition = rectTransform.anchoredPosition;
        initialRotation = rectTransform.localRotation;
    }

    private void Update()
    {
        if (isDragging || isLocked)
            return;

        rectTransform.anchoredPosition = Vector2.Lerp(
            rectTransform.anchoredPosition,
            initialAnchoredPosition,
            Time.deltaTime * returnSpeed
        );

        rectTransform.localRotation = Quaternion.Lerp(
            rectTransform.localRotation,
            initialRotation,
            Time.deltaTime * returnSpeed
        );
    }

    public void SetData(Sprite icon, string displayName)
    {
        if (iconImage != null)
            iconImage.sprite = icon;

        if (nameText != null)
            nameText.text = displayName;
    }

    public void ResetView()
    {
        isLocked = false;
        isDragging = false;
        rectTransform.anchoredPosition = initialAnchoredPosition;
        rectTransform.localRotation = initialRotation;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
    }

    public void Lock()
    {
        isLocked = true;
        isDragging = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isLocked)
            return;

        isDragging = true;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isLocked || !isDragging)
            return;

        float scaleFactor = rootCanvas != null ? rootCanvas.scaleFactor : 1f;
        rectTransform.anchoredPosition += eventData.delta / scaleFactor;

        float normalized = Mathf.Clamp(rectTransform.anchoredPosition.x / swipeThreshold, -1f, 1f);
        float zRotation = -normalized * maxRotation;
        rectTransform.localRotation = Quaternion.Euler(0f, 0f, zRotation);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isLocked)
            return;

        isDragging = false;
        canvasGroup.blocksRaycasts = true;

        float x = rectTransform.anchoredPosition.x;

        if (x >= swipeThreshold)
        {
            Lock();
            Swiped?.Invoke(SortCategory.Edible);
            return;
        }

        if (x <= -swipeThreshold)
        {
            Lock();
            Swiped?.Invoke(SortCategory.Inedible);
            return;
        }
    }
}