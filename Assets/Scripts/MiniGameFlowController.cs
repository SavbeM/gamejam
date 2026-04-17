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

    public void StartFlow()
    {
        isRunning = true;
        BuildQueue();
        SpawnNextGame();
    }

    public void StopFlow()
    {
        isRunning = false;

        if (currentMiniGame != null)
        {
            currentMiniGame.Cleanup();
        }

        if (currentInstance != null)
        {
            Destroy(currentInstance);
        }
    }

    private void BuildQueue()
    {
        gameQueue.Clear();

        foreach (var entry in playlist.games)
        {
            gameQueue.Enqueue(entry);
        }
    }

    private void SpawnNextGame()
    {
        if (!isRunning) return;

        if (currentInstance != null)
        {
            Destroy(currentInstance);
        }

        if (gameQueue.Count == 0)
        {
            BuildQueue();
        }

        currentEntry = gameQueue.Dequeue();

        currentInstance = Instantiate(currentEntry.prefab, miniGameHost);
        currentMiniGame = currentInstance.GetComponent<IMiniGame>();

        if (currentMiniGame == null)
        {
            Debug.LogError($"Prefab {currentEntry.name} has no IMiniGame implementation.");
            return;
        }

        hudController.SetMiniGameTitle(currentEntry.displayName);
        hudController.SetMiniGameTimer(currentEntry.timeLimit);

        currentMiniGame.Setup(HandleMiniGameFinished);
        currentMiniGame.Begin();
    }

    private void HandleMiniGameFinished(MiniGameResult result)
    {
        if (!isRunning) return;

        hudController.PlayMiniGameResult(result);
        swipeFeedController.PlaySwipeDownTransition(() =>
        {
            SpawnNextGame();
        });
    }
}