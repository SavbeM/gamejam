using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Session")]
    [SerializeField] private bool autoStartOnSceneLoad = true;
    [SerializeField] private bool allowTapToRestart = true;

    [Header("Ending Scenes")]
    [SerializeField] private string goodEndingSceneName = "GoodEnding";
    [SerializeField] private string badEndingSceneName = "BadEnding";

    [Header("Refs")]
    [SerializeField] private MiniGameFlowController flowController;
    [SerializeField] private HUDController hudController;
    [SerializeField] private GlobalProgressTimer globalProgressTimer;

    private bool isGameActive;
    private bool isEndingTriggered;

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
            return;
        }

        isEndingTriggered = false;

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
    }

    public void CompleteGameSuccessfully()
    {
        if (!isGameActive || isEndingTriggered)
        {
            return;
        }

        isEndingTriggered = true;
        isGameActive = false;

        flowController?.StopFlow();
        globalProgressTimer?.StopTimer();

        if (!string.IsNullOrWhiteSpace(goodEndingSceneName))
        {
            SceneManager.LoadScene(goodEndingSceneName);
        }
        else
        {
            Debug.LogError("[GameManager] Good ending scene name is empty.");
        }
    }

    public void EndSessionAsGameOver()
    {
        if (!isGameActive || isEndingTriggered)
        {
            return;
        }

        isEndingTriggered = true;
        isGameActive = false;

        flowController?.StopFlow();
        globalProgressTimer?.StopTimer();

        if (!string.IsNullOrWhiteSpace(badEndingSceneName))
        {
            SceneManager.LoadScene(badEndingSceneName);
        }
        else
        {
            Debug.LogError("[GameManager] Bad ending scene name is empty.");
        }
    }

    public void RestartSession()
    {
        flowController?.StopFlow();
        globalProgressTimer?.StopTimer();

        isGameActive = false;
        isEndingTriggered = false;

        StartSession();
    }

    private void HandleTimerEmptied()
    {
        EndSessionAsGameOver();
    }
}