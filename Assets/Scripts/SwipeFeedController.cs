using System;
using System.Collections;
using UnityEngine;

public enum SwipeTransitionDirection
{
    Up,
    Down
}

public class SwipeFeedController : MonoBehaviour
{
    [SerializeField] private RectTransform feedRoot;
    [SerializeField] private float swipeDistance = 1400f;
    [SerializeField] private float swipeDuration = 0.3f;
    [SerializeField] private AnimationCurve swipeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Vector2 initialPosition;
    private Coroutine swipeCoroutine;
    private bool isAnimating;
    public bool IsAnimating => isAnimating;
    public float SwipeDistance => swipeDistance;

    private void Awake()
    {
        if (feedRoot != null)
        {
            initialPosition = feedRoot.anchoredPosition;
        }
    }

    public void PlaySwipeTransition(SwipeTransitionDirection direction, Action onComplete)
    {
        if (feedRoot == null)
        {
            Debug.LogWarning("[SwipeFeed] Feed root is missing, transition is skipped.");
            onComplete?.Invoke();
            return;
        }

        if (swipeCoroutine != null)
        {
            StopCoroutine(swipeCoroutine);
        }

        Debug.Log($"[SwipeFeed] Swipe transition started. Direction: {direction}.");
        swipeCoroutine = StartCoroutine(AnimateSwipe(direction, onComplete));
    }

    private IEnumerator AnimateSwipe(SwipeTransitionDirection direction, Action onComplete)
    {
        isAnimating = true;
        initialPosition = feedRoot.anchoredPosition;

        Vector2 start = initialPosition;
        Vector2 offset = direction == SwipeTransitionDirection.Up ? Vector2.up : Vector2.down;
        Vector2 end = initialPosition + offset * swipeDistance;

        float time = 0f;
        while (time < swipeDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / swipeDuration);
            t = swipeCurve.Evaluate(t);
            feedRoot.anchoredPosition = Vector2.LerpUnclamped(start, end, t);
            yield return null;
        }

        feedRoot.anchoredPosition = initialPosition;
        isAnimating = false;
        swipeCoroutine = null;
        Debug.Log("[SwipeFeed] Swipe transition finished.");
        onComplete?.Invoke();
    }
}
