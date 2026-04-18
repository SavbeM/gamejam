using System;
using System.Collections.Generic;
using UnityEngine;

public class MiniGameFlowController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private MiniGamePlaylist playlist;

    [Header("Scene refs")]
    [SerializeField] private Transform miniGameHost;
    [SerializeField] private SwipeFeedController swipeFeedController;
    [SerializeField] private HUDController hudController;

    [Header("Swipe Input")]
    [SerializeField] private float swipeThresholdPixels = 140f;
    [SerializeField] private float swipeMaxHorizontalRatio = 0.75f;

    private readonly List<MiniGameEntry> gameEntries = new();
    private int currentIndex;

    private GameObject previousInstance;
    private GameObject currentInstance;
    private GameObject nextInstance;
    private IMiniGame currentMiniGame;
    private TimedMiniGameBase currentTimedMiniGame;
    private MiniGameEntry currentEntry;
    private bool isRunning;
    private bool isTransitionRequested;
    private bool swipeStarted;
    private Vector2 swipeStartScreenPosition;

    private void Awake()
    {
        if (hudController == null)
        {
            hudController = FindFirstObjectByType<HUDController>();
        }

        if (swipeFeedController == null)
        {
            swipeFeedController = FindFirstObjectByType<SwipeFeedController>();
        }
    }

    private void Update()
    {
        if (!isRunning)
        {
            return;
        }

        HandleSwipeInput();
    }

    public void StartFlow()
    {
        if (isRunning)
        {
            Debug.LogWarning("[MiniGameFlow] StartFlow called while already running.");
            return;
        }

        isRunning = true;
        currentIndex = 0;
        Debug.Log("[MiniGameFlow] Flow started.");
        BuildGameEntryList();

        if (gameEntries.Count == 0)
        {
            Debug.LogError("MiniGameFlowController: valid game list is empty.");
            isRunning = false;
            return;
        }

        RebuildVisibleGames();
    }

    public void StopFlow()
    {
        Debug.Log("[MiniGameFlow] Flow stopped.");
        isRunning = false;
        isTransitionRequested = false;
        swipeStarted = false;

        CleanupAllVisibleGames();

        if (hudController != null)
        {
            hudController.HideLevelComplete();
        }
    }

    private void BuildGameEntryList()
    {
        gameEntries.Clear();

        if (playlist == null || playlist.games == null || playlist.games.Count == 0)
        {
            Debug.LogError("MiniGameFlowController: playlist is empty or missing.");
            return;
        }

        foreach (MiniGameEntry entry in playlist.games)
        {
            if (entry != null && entry.prefab != null)
            {
                gameEntries.Add(entry);
            }
        }

        Debug.Log($"[MiniGameFlow] Entry list rebuilt. Entries: {gameEntries.Count}.");
    }

    private void RebuildVisibleGames()
    {
        isTransitionRequested = false;
        swipeStarted = false;

        if (hudController != null)
        {
            hudController.HideLevelComplete();
        }

        if (!isRunning || miniGameHost == null || gameEntries.Count == 0)
        {
            return;
        }

        CleanupAllVisibleGames();
        SpawnSlotInstances();
    }

    private void SpawnSlotInstances()
    {
        int previousIndex = WrapIndex(currentIndex - 1);
        int nextIndex = WrapIndex(currentIndex + 1);

        float slotOffset = swipeFeedController != null ? swipeFeedController.SwipeDistance : 1400f;
        previousInstance = SpawnGameInstance(previousIndex, slotOffset);
        currentInstance = SpawnGameInstance(currentIndex, 0f);
        nextInstance = SpawnGameInstance(nextIndex, -slotOffset);

        if (currentInstance == null)
        {
            Debug.LogError("[MiniGameFlow] Failed to spawn current game instance.");
            return;
        }

        currentEntry = gameEntries[currentIndex];
        currentMiniGame = currentInstance.GetComponent<IMiniGame>() ?? currentInstance.GetComponentInChildren<IMiniGame>();
        currentTimedMiniGame = currentInstance.GetComponent<TimedMiniGameBase>() ?? currentInstance.GetComponentInChildren<TimedMiniGameBase>();

        if (currentMiniGame == null)
        {
            Debug.LogError($"MiniGameFlowController: prefab '{currentEntry.prefab.name}' has no IMiniGame implementation.");
            CleanupAllVisibleGames();
            return;
        }

        if (currentTimedMiniGame != null)
        {
            currentTimedMiniGame.SetDuration(currentEntry.timeLimit);
        }

        currentMiniGame.Setup(HandleMiniGameFinished);
        currentMiniGame.Begin();

        hudController?.ShowGameplay();
        hudController?.SetMiniGameTitle(currentEntry.displayName);
        Debug.Log($"[MiniGameFlow] Active mini-game is '{currentEntry.displayName}'. Prev and next are pre-rendered.");
    }

    private GameObject SpawnGameInstance(int gameIndex, float anchoredY)
    {
        MiniGameEntry entry = gameEntries[gameIndex];
        Debug.Log($"[MiniGameFlow] Spawning slot instance '{entry.displayName}' at y={anchoredY:0.##}.");
        GameObject instance = Instantiate(entry.prefab, miniGameHost);

        RectTransform rectTransform = instance.GetComponent<RectTransform>() ?? instance.GetComponentInChildren<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, anchoredY);
        }
        else
        {
            Vector3 localPosition = instance.transform.localPosition;
            instance.transform.localPosition = new Vector3(localPosition.x, anchoredY, localPosition.z);
        }

        return instance;
    }

    private void HandleMiniGameFinished(MiniGameResult result)
    {
        if (!isRunning || isTransitionRequested)
        {
            return;
        }

        Debug.Log($"[MiniGameFlow] Mini-game '{currentEntry?.displayName}' completed with result: {result}.");
        hudController?.PlayMiniGameResult(result);
        RequestSwipeTransition(+1, "mini-game finished");
    }

    private void HandleSwipeInput()
    {
        if (isTransitionRequested)
        {
            swipeStarted = false;
            return;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            RequestSwipeTransition(+1, "up arrow");
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            RequestSwipeTransition(-1, "down arrow");
        }

        if (TryConsumeSwipe(out int swipeStep, out string swipeSource))
        {
            RequestSwipeTransition(swipeStep, swipeSource);
        }
    }

    private bool TryConsumeSwipe(out int swipeStep, out string inputSource)
    {
        swipeStep = 0;
        inputSource = null;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                swipeStarted = true;
                swipeStartScreenPosition = touch.position;
                return false;
            }

            if (!swipeStarted)
            {
                return false;
            }

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                swipeStarted = false;
                Vector2 delta = touch.position - swipeStartScreenPosition;
                return EvaluateSwipeDelta(delta, out swipeStep, out inputSource);
            }

            return false;
        }

        if (Input.GetMouseButtonDown(0))
        {
            swipeStarted = true;
            swipeStartScreenPosition = Input.mousePosition;
            return false;
        }

        if (swipeStarted && Input.GetMouseButtonUp(0))
        {
            swipeStarted = false;
            Vector2 delta = (Vector2)Input.mousePosition - swipeStartScreenPosition;
            return EvaluateSwipeDelta(delta, out swipeStep, out inputSource);
        }

        return false;
    }

    private bool EvaluateSwipeDelta(Vector2 delta, out int swipeStep, out string inputSource)
    {
        swipeStep = 0;
        inputSource = null;

        float absY = Mathf.Abs(delta.y);
        float absX = Mathf.Abs(delta.x);

        if (absY < Mathf.Max(1f, swipeThresholdPixels))
        {
            return false;
        }

        if (absX > absY * Mathf.Max(0f, swipeMaxHorizontalRatio))
        {
            return false;
        }

        bool isSwipeUp = delta.y > 0f;
        swipeStep = isSwipeUp ? +1 : -1;
        inputSource = isSwipeUp ? "swipe up" : "swipe down";
        return true;
    }

    public void RequestNextLevelFromUpButton()
    {
        RequestSwipeTransition(+1, "up button");
    }

    public void RequestNextLevelFromDownButton()
    {
        RequestSwipeTransition(-1, "down button");
    }

    private void RequestSwipeTransition(int step, string inputSource)
    {
        if (!isRunning || gameEntries.Count == 0 || isTransitionRequested || step == 0)
        {
            return;
        }

        isTransitionRequested = true;
        Debug.Log($"[MiniGameFlow] Swipe transition requested via {inputSource}. Step: {step}.");
        hudController?.HideLevelComplete();
        PlayTransition(step);
    }

    private void PlayTransition(int step)
    {
        SwipeTransitionDirection direction = step > 0
            ? SwipeTransitionDirection.Up
            : SwipeTransitionDirection.Down;

        if (swipeFeedController != null)
        {
            swipeFeedController.PlaySwipeTransition(direction, () =>
            {
                CompleteTransition(step);
            });
        }
        else
        {
            CompleteTransition(step);
        }
    }

    private void CompleteTransition(int step)
    {
        currentIndex = WrapIndex(currentIndex + Math.Sign(step));
        RebuildVisibleGames();
    }

    private int WrapIndex(int index)
    {
        if (gameEntries.Count == 0)
        {
            return 0;
        }

        int wrapped = index % gameEntries.Count;
        return wrapped < 0 ? wrapped + gameEntries.Count : wrapped;
    }

    private void CleanupAllVisibleGames()
    {
        if (currentMiniGame != null)
        {
            currentMiniGame.Cleanup();
            currentMiniGame = null;
        }

        currentTimedMiniGame = null;
        currentEntry = null;

        DestroySlotInstance(ref previousInstance);
        DestroySlotInstance(ref currentInstance);
        DestroySlotInstance(ref nextInstance);
    }

    private static void DestroySlotInstance(ref GameObject instance)
    {
        if (instance == null)
        {
            return;
        }

        Destroy(instance);
        instance = null;
    }
}
