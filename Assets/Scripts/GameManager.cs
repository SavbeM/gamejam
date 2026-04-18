using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Session")]
    [SerializeField] private float totalSessionTime = 45f;
    [SerializeField] private bool autoStartOnSceneLoad = true;
    [SerializeField] private bool allowTapToRestart = true;

    [Header("Refs")]
    [SerializeField] private MiniGameFlowController flowController;
    [SerializeField] private HUDController hudController;

    private float remainingTime;
    private bool isGameActive;

    public float RemainingTime => remainingTime;
    public float TotalSessionTime => totalSessionTime;
    public bool IsGameActive => isGameActive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Debug.Log("[GameManager] Awake completed.");

        if (flowController == null)
        {
            flowController = FindFirstObjectByType<MiniGameFlowController>();
        }

        if (hudController == null)
        {
            hudController = FindFirstObjectByType<HUDController>();
        }
    }

    private void Start()
    {
        if (autoStartOnSceneLoad)
        {
            Debug.Log("[GameManager] Auto-start is enabled. Starting session.");
            StartSession();
        }
    }

    private void Update()
    {
        if (!isGameActive)
        {
            if (allowTapToRestart && Input.GetMouseButtonDown(0))
            {
                Debug.Log("[GameManager] Restart input received.");
                RestartSession();
            }

            return;
        }

        remainingTime -= Time.deltaTime;
        remainingTime = Mathf.Max(remainingTime, 0f);

        if (hudController != null && totalSessionTime > 0f)
        {
            hudController.SetProgress(remainingTime / totalSessionTime);
        }

        if (remainingTime <= 0f)
        {
            Debug.Log("[GameManager] Session timer reached zero.");
            EndSession();
        }
    }

    public void StartSession()
    {
        if (isGameActive)
        {
            Debug.LogWarning("[GameManager] StartSession was called while session is already active.");
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

        remainingTime = totalSessionTime;
        isGameActive = true;
        Debug.Log($"[GameManager] Session started. Duration: {totalSessionTime:0.##}s.");

        hudController?.ShowGameplay();
        hudController?.SetProgress(1f);

        if (flowController != null)
        {
            flowController.StartFlow();
        }
        else
        {
            Debug.LogError("GameManager: MiniGameFlowController is missing.");
        }
    }

    public void EndSession()
    {
        if (!isGameActive)
        {
            Debug.LogWarning("[GameManager] EndSession was called while session is not active.");
            return;
        }

        isGameActive = false;
        Debug.Log("[GameManager] Session ended.");
        flowController?.StopFlow();
        hudController?.ShowGameOver();
    }

    public void RestartSession()
    {
        Debug.Log("[GameManager] Restarting session.");
        flowController?.StopFlow();
        remainingTime = totalSessionTime;
        isGameActive = false;
        StartSession();
    }
}
