using System;
using System.Collections;
using UnityEngine;

public class SwipeFeedController : MonoBehaviour
{
    [SerializeField] private RectTransform feedRoot;
    [SerializeField] private float swipeDistance = 1400f;
    [SerializeField] private float swipeDuration = 0.25f;

    private Vector2 initialPosition;
    private bool isAnimating;

    private void Awake()
    {
        initialPosition = feedRoot.anchoredPosition;
    }

    public void PlaySwipeDownTransition(Action onComplete)
    {
        if (isAnimating)
        {
            Debug.LogWarning("[SwipeFeed] Swipe transition request ignored because animation is in progress.");
            return;
        }

        Debug.Log("[SwipeFeed] Swipe transition started.");
        StartCoroutine(AnimateSwipe(onComplete));
    }

    private IEnumerator AnimateSwipe(Action onComplete)
    {
        isAnimating = true;

        Vector2 start = initialPosition;
        Vector2 end = initialPosition + Vector2.down * swipeDistance;

        float time = 0f;
        while (time < swipeDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / swipeDuration);
            t = EaseInOutCubic(t);
            feedRoot.anchoredPosition = Vector2.LerpUnclamped(start, end, t);
            yield return null;
        }

        feedRoot.anchoredPosition = initialPosition;
        isAnimating = false;
        Debug.Log("[SwipeFeed] Swipe transition finished.");
        onComplete?.Invoke();
    }

    private float EaseInOutCubic(float t)
    {
        return t < 0.5f
            ? 4f * t * t * t
            : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
    }
}
