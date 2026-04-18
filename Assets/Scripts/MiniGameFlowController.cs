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

    private readonly Queue<MiniGameEntry> gameQueue = new();
    private GameObject currentInstance;
    private IMiniGame currentMiniGame;
    private TimedMiniGameBase currentTimedMiniGame;
    private MiniGameEntry currentEntry;
    private bool isRunning;
    private bool waitingForNextLevelInput;
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
    }

    private void Update()
    {
        if (!isRunning)
        {
            return;
        }

        if (hudController != null && currentTimedMiniGame != null)
        {
            hudController.SetMiniGameTimer(currentTimedMiniGame.TimeLeft);
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

        isRunning = true;
        Debug.Log("[MiniGameFlow] Flow started.");
        BuildQueue();

        if (gameQueue.Count == 0)
        {
            Debug.LogError("MiniGameFlowController: queue is empty.");
            isRunning = false;
            return;
        }

        SpawnNextGame();
    }

    public void StopFlow()
    {
        Debug.Log("[MiniGameFlow] Flow stopped.");
        isRunning = false;
        waitingForNextLevelInput = false;

        CleanupCurrentMiniGame();

        if (hudController != null)
        {
            hudController.HideLevelComplete();
            hudController.SetMiniGameTimer(0f);
        }
    }

    private void BuildQueue()
    {
        gameQueue.Clear();

        if (playlist == null || playlist.games == null || playlist.games.Count == 0)
        {
            Debug.LogError("MiniGameFlowController: playlist is empty or missing.");
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
            Debug.LogError("MiniGameFlowController: no games to spawn.");
            return;
        }

        currentEntry = gameQueue.Dequeue();
        Debug.Log($"[MiniGameFlow] Spawning mini-game '{currentEntry.displayName}'.");
        currentInstance = Instantiate(currentEntry.prefab, miniGameHost);
        currentMiniGame = currentInstance.GetComponent<IMiniGame>() ?? currentInstance.GetComponentInChildren<IMiniGame>();
        currentTimedMiniGame = currentInstance.GetComponent<TimedMiniGameBase>() ?? currentInstance.GetComponentInChildren<TimedMiniGameBase>();

        if (currentMiniGame == null)
        {
            Debug.LogError($"MiniGameFlowController: prefab '{currentEntry.prefab.name}' has no IMiniGame implementation.");
            CleanupCurrentMiniGame();
            return;
        }

        if (currentTimedMiniGame != null)
        {
            currentTimedMiniGame.SetDuration(currentEntry.timeLimit);
        }

        if (hudController != null)
        {
            hudController.ShowGameplay();
            hudController.HideLevelComplete();
            hudController.SetMiniGameTitle(currentEntry.displayName);
            hudController.SetMiniGameTimer(currentEntry.timeLimit);
        }

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
            waitingForNextLevelInput = true;
            hudController?.ShowLevelComplete("LEVEL CLEARED", "Swipe to next level");
            return;
        }

        Debug.Log($"[MiniGameFlow] Mini-game '{currentEntry?.displayName}' completed with result: {result}.");
        hudController?.PlayMiniGameResult(result);
        GoToNextGame();
    }

    private void HandleNextLevelInput()
    {
        if (!waitingForNextLevelInput)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            swipeStartPosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            Vector2 endPosition = Input.mousePosition;
            float deltaX = endPosition.x - swipeStartPosition.x;
            float deltaY = endPosition.y - swipeStartPosition.y;

            bool isSwipe = Mathf.Abs(deltaX) > 80f || Mathf.Abs(deltaY) > 80f;
            if (!isSwipe)
            {
                return;
            }

            Debug.Log($"[MiniGameFlow] Swipe detected. deltaX={deltaX:0.##}, deltaY={deltaY:0.##}. Moving to next level.");
            waitingForNextLevelInput = false;
            hudController?.HideLevelComplete();
            GoToNextGame();
        }
    }

    private void GoToNextGame()
    {
        if (swipeFeedController != null)
        {
            swipeFeedController.PlaySwipeDownTransition(SpawnNextGame);
        }
        else
        {
            SpawnNextGame();
        }
    }

    private void CleanupCurrentMiniGame()
    {
        if (currentMiniGame != null)
        {
            currentMiniGame.Cleanup();
            currentMiniGame = null;
        }

        currentTimedMiniGame = null;
        currentEntry = null;

        if (currentInstance != null)
        {
            Destroy(currentInstance);
            currentInstance = null;
        }
    }
}
