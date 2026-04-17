using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Session")]
    [SerializeField] private float totalSessionTime = 45f;

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
    }

    private void Start()
    {
        StartSession();
    }

    private void Update()
    {
        if (!isGameActive) return;

        remainingTime -= Time.deltaTime;
        remainingTime = Mathf.Max(remainingTime, 0f);

        hudController.SetProgress(remainingTime / totalSessionTime);

        if (remainingTime <= 0f)
        {
            EndSession();
        }
    }

    public void StartSession()
    {
        remainingTime = totalSessionTime;
        isGameActive = true;

        hudController.ShowGameplay();
        flowController.StartFlow();
    }

    public void EndSession()
    {
        if (!isGameActive) return;

        isGameActive = false;
        flowController.StopFlow();
        hudController.ShowGameOver();
    }
}