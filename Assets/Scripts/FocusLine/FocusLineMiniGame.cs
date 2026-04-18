using UnityEngine;

public class FocusLineMiniGame : TimedMiniGameBase
{
    [Header("UI")]
    public RectTransform indicator;
    public RectTransform bar;
    public GameObject[] hearts;

    [Header("Zones")]
    public RectTransform greenZone;
    public RectTransform orangeLeft;
    public RectTransform orangeRight;
    public RectTransform redLeft;
    public RectTransform redRight;

    [Header("Settings")]
    public float speed = 250f;
    public int maxAttempts = 3;

    private int attempts;
    private bool movingRight = true;
    private bool gameEnded = false;
    private float minWorldX, maxWorldX;
    private float worldSpeed;

    public override void Begin()
    {
        base.Begin();

        attempts = maxAttempts;
        gameEnded = false;
        movingRight = true;

        UpdateHeartsUI();

        Vector3[] barCorners = new Vector3[4];
        bar.GetWorldCorners(barCorners);
        minWorldX = barCorners[0].x;
        maxWorldX = barCorners[2].x;

        float scale = (maxWorldX - minWorldX) / bar.rect.width;
        worldSpeed = speed * scale;

        Vector3 pos = indicator.position;
        pos.x = (minWorldX + maxWorldX) * 0.5f;
        indicator.position = pos;
    }

    protected override void Update()
    {
        base.Update();
        
        if (!IsRunning || IsFinished || gameEnded) return;

        MoveIndicator();

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            CheckHit();
        }
    }

    void MoveIndicator()
    {
        float dir = movingRight ? 1f : -1f;

        Vector3 pos = indicator.position;
        pos.x += dir * worldSpeed * Time.deltaTime;

        if (pos.x >= maxWorldX)
        {
            pos.x = maxWorldX;
            movingRight = false;
        }
        else if (pos.x <= minWorldX)
        {
            pos.x = minWorldX;
            movingRight = true;
        }

        indicator.position = pos;
    }

    void CheckHit()
    {
        float worldX = indicator.position.x;

        if (IsInsideWorld(greenZone, worldX))
        {
            Win();
        }
        else
        {
            attempts--;
            UpdateHeartsUI();
            Debug.Log("Miss! Attempts left: " + attempts);

            if (attempts <= 0)
                Lose();
        }
    }

    void UpdateHeartsUI()
    {
        if (hearts == null) return;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] != null)
                hearts[i].SetActive(i < attempts);
        }
    }

    bool IsInsideWorld(RectTransform zone, float worldX)
    {
        Vector3[] corners = new Vector3[4];
        zone.GetWorldCorners(corners);
        return worldX >= corners[0].x && worldX <= corners[2].x;
    }

    void Win()
    {
        gameEnded = true;
        Debug.Log("WIN");
        Finish(MiniGameResult.Win);
    }

    void Lose()
    {
        gameEnded = true;
        Debug.Log("LOSE");
        Finish(MiniGameResult.Fail);
    }
}