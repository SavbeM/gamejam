using System.Collections.Generic;
using UnityEngine;

public class MiniGameFlowController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private MiniGamePlaylist playlist;
    [SerializeField] private float rewardTimeOnWin = 5f;
    [SerializeField] private float penaltyTimeOnFail = 0f;

    [Header("Scene refs")]
    [SerializeField] private Transform miniGameHost;
    [SerializeField] private SwipeFeedController swipeFeedController;
    [SerializeField] private HUDController hudController;
    [SerializeField] private GlobalProgressTimer globalProgressTimer;

    [Header("Next Level Input")]
    [SerializeField] private float nextLevelSwipeThresholdPixels = 160f;
    [SerializeField] private float nextLevelSwipeMaxHorizontalRatio = 0.75f;

    private readonly Queue<MiniGameEntry> gameQueue = new();

    private GameObject currentInstance;
    private IMiniGame currentMiniGame;
    private MiniGameEntry currentEntry;

    private bool isRunning;
    private bool waitingForNextLevelInput;
    private bool isTransitionRequested;
    private bool nextLevelSwipeStarted;

    private Vector2 swipeStartPosition;

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

        if (globalProgressTimer == null)
        {
            globalProgressTimer = FindFirstObjectByType<GlobalProgressTimer>();
        }
    }

    private void Update()
    {
        if (!isRunning)
        {
            return;
        }

        HandleNextLevelInput();
    }

    public void StartFlow()
    {
        if (isRunning)
        {
            Debug.LogWarning("[MiniGameFlow] StartFlow called while already running.");
            return;
        }

        BuildQueue();

        if (gameQueue.Count == 0)
        {
            Debug.LogError("[MiniGameFlow] Queue is empty.");
            return;
        }

        isRunning = true;
        waitingForNextLevelInput = false;
        isTransitionRequested = false;
        nextLevelSwipeStarted = false;

        SpawnNextGame();
        Debug.Log("[MiniGameFlow] Flow started.");
    }

    public void StopFlow()
    {
        isRunning = false;
        waitingForNextLevelInput = false;
        isTransitionRequested = false;
        nextLevelSwipeStarted = false;

        CleanupCurrentMiniGame();
        hudController?.HideLevelComplete();

        Debug.Log("[MiniGameFlow] Flow stopped.");
    }

    private void BuildQueue()
    {
        gameQueue.Clear();

        if (playlist == null || playlist.games == null || playlist.games.Count == 0)
        {
            Debug.LogError("[MiniGameFlow] Playlist is empty or missing.");
            return;
        }

        foreach (MiniGameEntry entry in playlist.games)
        {
            if (entry != null && entry.prefab != null)
            {
                gameQueue.Enqueue(entry);
            }
        }

        Debug.Log($"[MiniGameFlow] Queue rebuilt. Entries: {gameQueue.Count}.");
    }

    private void SpawnNextGame()
    {
        waitingForNextLevelInput = false;
        isTransitionRequested = false;
        nextLevelSwipeStarted = false;

        hudController?.HideLevelComplete();

        if (!isRunning || miniGameHost == null)
        {
            return;
        }

        CleanupCurrentMiniGame();

        if (gameQueue.Count == 0)
        {
            BuildQueue();
        }

        if (gameQueue.Count == 0)
        {
            Debug.LogError("[MiniGameFlow] No games to spawn.");
            return;
        }

        currentEntry = gameQueue.Dequeue();
        currentInstance = Instantiate(currentEntry.prefab, miniGameHost);
        currentMiniGame = currentInstance.GetComponent<IMiniGame>() ?? currentInstance.GetComponentInChildren<IMiniGame>();

        if (currentMiniGame == null)
        {
            Debug.LogError($"[MiniGameFlow] Prefab '{currentEntry.prefab.name}' has no IMiniGame implementation.");
            CleanupCurrentMiniGame();
            return;
        }

        hudController?.ShowGameplay();
        hudController?.HideLevelComplete();
        hudController?.SetMiniGameTitle(currentEntry.displayName);
        hudController?.SetMiniGameRules(currentEntry.GameRules);

        currentMiniGame.Setup(HandleMiniGameFinished);
        currentMiniGame.Begin();

        Debug.Log($"[MiniGameFlow] Mini-game '{currentEntry.displayName}' started.");
    }

    private void HandleMiniGameFinished(MiniGameResult result)
    {
        if (!isRunning)
        {
            return;
        }

        if (result == MiniGameResult.Win)
        {
            Debug.Log($"[MiniGameFlow] Mini-game '{currentEntry?.displayName}' completed with WIN.");

            globalProgressTimer?.AddTime(rewardTimeOnWin);

            waitingForNextLevelInput = true;
            hudController?.ShowLevelComplete("LEVEL CLEARED", "Swipe ↑/↓ or press ↑/↓ to next level");
            return;
        }

        Debug.Log($"[MiniGameFlow] Mini-game '{currentEntry?.displayName}' completed with result: {result}.");

        if (result == MiniGameResult.Fail && penaltyTimeOnFail > 0f)
        {
            globalProgressTimer?.RemoveTime(penaltyTimeOnFail);
        }

        hudController?.PlayMiniGameResult(result);
        GoToNextGame();
    }

    private void HandleNextLevelInput()
    {
        if (!waitingForNextLevelInput)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            RequestNextLevelTransition("keyboard arrow");
        }

        if (TryConsumeNextLevelSwipe(out string swipeSource))
        {
            RequestNextLevelTransition(swipeSource);
        }
    }

    private bool TryConsumeNextLevelSwipe(out string inputSource)
    {
        inputSource = null;

        if (isTransitionRequested)
        {
            nextLevelSwipeStarted = false;
            return false;
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                nextLevelSwipeStarted = true;
                swipeStartPosition = touch.position;
                return false;
            }

            if (!nextLevelSwipeStarted)
            {
                return false;
            }

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                nextLevelSwipeStarted = false;
                Vector2 delta = touch.position - swipeStartPosition;
                return EvaluateSwipeDelta(delta, out inputSource);
            }

            return false;
        }

        if (Input.GetMouseButtonDown(0))
        {
            nextLevelSwipeStarted = true;
            swipeStartPosition = Input.mousePosition;
            return false;
        }

        if (nextLevelSwipeStarted && Input.GetMouseButtonUp(0))
        {
            nextLevelSwipeStarted = false;
            Vector2 delta = (Vector2)Input.mousePosition - swipeStartPosition;
            return EvaluateSwipeDelta(delta, out inputSource);
        }

        return false;
    }

    private bool EvaluateSwipeDelta(Vector2 delta, out string inputSource)
    {
        inputSource = null;

        float absY = Mathf.Abs(delta.y);
        float absX = Mathf.Abs(delta.x);

        if (absY < Mathf.Max(1f, nextLevelSwipeThresholdPixels))
        {
            return false;
        }

        if (absX > absY * Mathf.Max(0f, nextLevelSwipeMaxHorizontalRatio))
        {
            return false;
        }

        inputSource = delta.y > 0f ? "swipe up" : "swipe down";
        return true;
    }

    public void RequestNextLevelFromUpButton()
    {
        RequestNextLevelTransition("up button");
    }

    public void RequestNextLevelFromDownButton()
    {
        RequestNextLevelTransition("down button");
    }

    private void RequestNextLevelTransition(string inputSource)
    {
        if (!waitingForNextLevelInput || isTransitionRequested)
        {
            return;
        }

        isTransitionRequested = true;
        waitingForNextLevelInput = false;

        Debug.Log($"[MiniGameFlow] Next level requested via {inputSource}.");
        hudController?.HideLevelComplete();

        GoToNextGame();
    }

    private void GoToNextGame()
    {
        if (swipeFeedController != null)
        {
            swipeFeedController.PlaySwipeDownTransition(() =>
            {
                isTransitionRequested = false;
                SpawnNextGame();
            });

            return;
        }

        isTransitionRequested = false;
        SpawnNextGame();
    }

    private void CleanupCurrentMiniGame()
    {
        if (currentMiniGame != null)
        {
            currentMiniGame.Cleanup();
            currentMiniGame = null;
        }

        currentEntry = null;

        if (currentInstance != null)
        {
            Destroy(currentInstance);
            currentInstance = null;
        }
    }
}