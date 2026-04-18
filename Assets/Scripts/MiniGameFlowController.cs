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
    private MiniGameEntry currentEntry;
    private bool isRunning;
    private bool waitingForNextLevelInput;
    private bool nextLevelSwipeStarted;
    private Vector2 swipeStartPosition;

    private void Start()
    {
        StartFlow();
    }

    private void Update()
    {
        HandleNextLevelInput();
    }

    public void StartFlow()
    {
        if (isRunning)
            return;

        isRunning = true;
        BuildQueue();

        if (gameQueue.Count == 0)
        {
            Debug.LogError("MiniGameFlowController: queue is empty.");
            return;
        }

        SpawnNextGame();
    }

    public void StopFlow()
    {
        isRunning = false;
        waitingForNextLevelInput = false;
        nextLevelSwipeStarted = false;

        if (currentMiniGame != null)
        {
            currentMiniGame.Cleanup();
            currentMiniGame = null;
        }

        if (currentInstance != null)
        {
            Destroy(currentInstance);
            currentInstance = null;
        }

        if (hudController != null)
        {
            hudController.HideLevelComplete();
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
    }

    private void SpawnNextGame()
    {
        waitingForNextLevelInput = false;
        nextLevelSwipeStarted = false;

        if (hudController != null)
        {
            hudController.HideLevelComplete();
        }

        if (!isRunning || miniGameHost == null)
            return;

        if (currentMiniGame != null)
        {
            currentMiniGame.Cleanup();
            currentMiniGame = null;
        }

        if (currentInstance != null)
        {
            Destroy(currentInstance);
            currentInstance = null;
        }

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
        currentInstance = Instantiate(currentEntry.prefab, miniGameHost);
        currentMiniGame = currentInstance.GetComponent<IMiniGame>();

        if (currentMiniGame == null)
        {
            currentMiniGame = currentInstance.GetComponentInChildren<IMiniGame>();
        }

        if (currentMiniGame == null)
        {
            Debug.LogError($"MiniGameFlowController: prefab '{currentEntry.prefab.name}' has no IMiniGame implementation.");
            return;
        }

        if (hudController != null)
        {
            hudController.ShowGameplay();
            hudController.SetMiniGameTitle(currentEntry.displayName);
            hudController.SetMiniGameTimer(currentEntry.timeLimit);
        }

        currentMiniGame.Setup(HandleMiniGameFinished);
        currentMiniGame.Begin();
    }

    private void HandleMiniGameFinished(MiniGameResult result)
    {
        if (!isRunning)
            return;

        if (result == MiniGameResult.Win)
        {
            waitingForNextLevelInput = true;
            nextLevelSwipeStarted = false;

            if (hudController != null)
            {
                hudController.ShowLevelComplete("LEVEL CLEARED", "Swipe to next level");
            }

            return;
        }
    }

    private void HandleNextLevelInput()
    {
        if (!waitingForNextLevelInput)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            nextLevelSwipeStarted = true;
            swipeStartPosition = Input.mousePosition;
        }

        if (!nextLevelSwipeStarted)
            return;

        if (Input.GetMouseButtonUp(0))
        {
            Vector2 endPosition = Input.mousePosition;
            float deltaX = endPosition.x - swipeStartPosition.x;
            float deltaY = endPosition.y - swipeStartPosition.y;

            bool isSwipe = Mathf.Abs(deltaX) > 80f || Mathf.Abs(deltaY) > 80f;

            if (!isSwipe)
                return;

            waitingForNextLevelInput = false;
            nextLevelSwipeStarted = false;

            if (hudController != null)
            {
                hudController.HideLevelComplete();
            }

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
}