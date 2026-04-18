using System;
using System.Collections;
using UnityEngine;

namespace MagicPigGames
{
    [Serializable]
    public class ProgressBar : MonoBehaviour
    {
        [Header("Overlay Bar")]
        [Tooltip("This is the bar that moves to show progress, covering the fill bar.")]
        public RectTransform overlayBar;

        [Tooltip("Width as a percent of the Progress Bar Rect Transform.")]
        [Range(0f, 1f)] public float sizeMin = 0f;

        [Tooltip("Width as a percent of the Progress Bar Rect Transform.")]
        [Range(0f, 1f)] public float sizeMax = 1f;

        [Header("Options")]
        [Tooltip("When true, the progress bar width will grow as the progress increases. When false, the progress bar width will shrink as the progress increases.")]
        public bool invertProgress = false;

        [Tooltip("When 0, the progress bar will update immediately. When greater than 0, the progress bar will take this many seconds to update.")]
        [Min(0f)]
        public float transitionTime = 0.1f;

        [Header("Plumbing")]
        public RectTransform rectTransform;

        private float _progress = 1f;
        protected float _elapsedTime = 0f;
        protected Vector2 _lastParentSize;
        protected Coroutine _transitionCoroutine;

        protected virtual float SizeAtCurrentProgress => Mathf.Lerp(SizeMin, SizeMax, _progress);

        public virtual float Progress => _progress;
        public virtual float ProgressPercent => _progress * 100f;

        protected virtual float SizeMin => rectTransform.rect.width * sizeMin;
        protected virtual float SizeMax => rectTransform.rect.width * sizeMax;
        protected virtual float CurrentOverlaySize => overlayBar.rect.width;

        public virtual void SetProgress(float progress)
        {
            if (progress is > 1f or < 0f)
            {
                Debug.LogError($"Progress value must be between 0 and 1. Value was {progress}. Will clamp.");
                progress = Mathf.Clamp01(progress);
            }

            _progress = ValueBasedOnInvert(progress);

            if (transitionTime <= 0f)
            {
                SetBarValue(SizeAtCurrentProgress);
                return;
            }

            StartTheCoroutine();
        }

        protected float ValueBasedOnInvert(float value) => invertProgress ? 1f - value : value;

        protected virtual void Start()
        {
            CheckOverlayBarRectTransform();
            SetProgress(1f);
        }

        protected virtual void LateUpdate() => HandleParentSizeChange();

        private void StartTheCoroutine()
        {
            if (_transitionCoroutine != null)
            {
                _elapsedTime = transitionTime - _elapsedTime;
                StopCoroutine(_transitionCoroutine);
            }
            else
            {
                _elapsedTime = 0f;
            }

            _transitionCoroutine = StartCoroutine(TransitionProgress(CurrentOverlaySize));
        }

        protected virtual void CheckOverlayBarRectTransform()
        {
            if (overlayBar == null) return;

            overlayBar.anchorMin = new Vector2(0.5f, 0f);
            overlayBar.anchorMax = new Vector2(0.5f, 1f);
            overlayBar.pivot = new Vector2(0.5f, 0.5f);

            Vector2 anchoredPos = overlayBar.anchoredPosition;
            anchoredPos.x = 0f;
            overlayBar.anchoredPosition = anchoredPos;
        }

        protected virtual void HandleParentSizeChange()
        {
            if (rectTransform == null) return;

            if (_lastParentSize == rectTransform.rect.size)
                return;

            _lastParentSize = rectTransform.rect.size;

            float progress = ValueBasedOnInvert(_progress);
            SetProgress(progress);
        }

        protected virtual IEnumerator TransitionProgress(float startWidth)
        {
            while (_elapsedTime < transitionTime)
            {
                float t = _elapsedTime / transitionTime;
                float newWidth = Mathf.Lerp(startWidth, SizeAtCurrentProgress, t);
                SetBarValue(newWidth);

                _elapsedTime += Time.deltaTime;
                yield return null;
            }

            SetBarValue(SizeAtCurrentProgress);
            _transitionCoroutine = null;
        }

        protected virtual void SetBarValue(float value)
        {
            if (overlayBar == null) return;

            Vector2 sizeDelta = overlayBar.sizeDelta;
            sizeDelta.x = value;
            overlayBar.sizeDelta = sizeDelta;

            Vector2 anchoredPos = overlayBar.anchoredPosition;
            anchoredPos.x = 0f;
            overlayBar.anchoredPosition = anchoredPos;
        }

        protected virtual void OnValidate()
        {
            sizeMin = Mathf.Clamp(sizeMin, 0f, sizeMax - 0.01f);
            sizeMax = Mathf.Clamp(sizeMax, sizeMin + 0.01f, 1f);

            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();

            if (overlayBar == null)
                Debug.Log("Please assign the Overlay Bar RectTransform.");

            CheckOverlayBarRectTransform();

            if (rectTransform != null)
                _lastParentSize = rectTransform.rect.size;
        }
    }
}