using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Session")]
    [SerializeField] private float totalSessionTime = 45f;
    [SerializeField] private bool autoStartOnSceneLoad = true;

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
            StartSession();
        }
    }

    private void Update()
    {
        if (!isGameActive)
        {
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
            EndSession();
        }
    }

    public void StartSession()
    {
        if (isGameActive)
        {
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
            return;
        }

        isGameActive = false;
        flowController?.StopFlow();
        hudController?.ShowGameOver();
    }
}
