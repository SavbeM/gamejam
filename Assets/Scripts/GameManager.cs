using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Session")]
    [SerializeField] private bool autoStartOnSceneLoad = true;
    [SerializeField] private bool allowTapToRestart = true;

    [Header("Refs")]
    [SerializeField] private MiniGameFlowController flowController;
    [SerializeField] private HUDController hudController;
    [SerializeField] private GlobalProgressTimer globalProgressTimer;

    private bool isGameActive;

    public bool IsGameActive => isGameActive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (flowController == null)
        {
            flowController = FindFirstObjectByType<MiniGameFlowController>();
        }

        if (hudController == null)
        {
            hudController = FindFirstObjectByType<HUDController>();
        }

        if (globalProgressTimer == null)
        {
            globalProgressTimer = FindFirstObjectByType<GlobalProgressTimer>();
        }

        if (globalProgressTimer != null)
        {
            globalProgressTimer.TimerEmptied += HandleTimerEmptied;
        }

        Debug.Log("[GameManager] Awake completed.");
    }

    private void OnDestroy()
    {
        if (globalProgressTimer != null)
        {
            globalProgressTimer.TimerEmptied -= HandleTimerEmptied;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        if (autoStartOnSceneLoad)
        {
            StartSession();
        }
    }

    private void Update()
    {
        if (isGameActive)
        {
            return;
        }

        if (allowTapToRestart && Input.GetMouseButtonDown(0))
        {
            RestartSession();
        }
    }

    public void StartSession()
    {
        if (isGameActive)
        {
            Debug.LogWarning("[GameManager] StartSession called while already active.");
            return;
        }

        if (flowController == null)
        {
            flowController = FindFirstObjectByType<MiniGameFlowController>();
        }

        if (hudController == null)
        {
            hudController = FindFirstObjectByType<HUDController>();
        }

        if (globalProgressTimer == null)
        {
            globalProgressTimer = FindFirstObjectByType<GlobalProgressTimer>();
        }

        if (flowController == null || hudController == null || globalProgressTimer == null)
        {
            Debug.LogError("[GameManager] Missing required references.");
            return;
        }

        isGameActive = true;

        hudController.ShowGameplay();
        hudController.HideLevelComplete();

        globalProgressTimer.ResetTimer(startImmediately: true);
        flowController.StartFlow();

        Debug.Log("[GameManager] Session started.");
    }

    public void EndSessionAsFinished()
    {
        if (!isGameActive)
        {
            return;
        }

        isGameActive = false;

        flowController?.StopFlow();
        globalProgressTimer?.StopTimer();
        hudController?.ShowGameFinished();

        Debug.Log("[GameManager] Session finished.");
    }

    public void EndSessionAsGameOver()
    {
        if (!isGameActive)
        {
            return;
        }

        isGameActive = false;

        flowController?.StopFlow();
        globalProgressTimer?.StopTimer();
        hudController?.ShowGameOver();

        Debug.Log("[GameManager] Session ended with game over.");
    }

    public void RestartSession()
    {
        Debug.Log("[GameManager] Restarting session.");

        isGameActive = false;

        flowController?.StopFlow();
        globalProgressTimer?.StopTimer();

        StartSession();
    }

    private void HandleTimerEmptied()
    {
        if (!isGameActive)
        {
            return;
        }

        Debug.Log("[GameManager] Global timer reached zero.");
        EndSessionAsGameOver();
    }
}