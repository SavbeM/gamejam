using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnscrambleWordMiniGame : TimedMiniGameBase
{
    [Header("Data")]
    [SerializeField] private List<UnscrambleWordData> words = new();

    [Header("Refs")]
    [SerializeField] private Transform scrambledContainer;
    [SerializeField] private Transform answerContainer;
    [SerializeField] private LetterTileView letterTilePrefab;
    [SerializeField] private LetterSlotView slotPrefab; // префаб слота

    [Header("Layout")]
    [SerializeField] private float tileSize = 60f;
    [SerializeField] private float tileSpacing = 10f;
    [SerializeField] private float rowSpacing = 20f; // расстояние между рядами

    private string currentWord;
    private List<LetterTileView> spawnedTiles = new();
    private List<LetterSlotView> answerSlots = new();

    public override void Setup(System.Action<MiniGameResult> onFinished)
    {
        base.Setup(onFinished);

        if (words == null || words.Count == 0)
        {
            Debug.LogError("UnscrambleWordMiniGame: words list is empty.");
            return;
        }

        currentWord = words[Random.Range(0, words.Count)].Word;
        SpawnLetters();
    }

    public override void Cleanup()
    {
        foreach (var tile in spawnedTiles)
            if (tile != null) Destroy(tile.gameObject);
        spawnedTiles.Clear();

        foreach (var slot in answerSlots)
            if (slot != null) Destroy(slot.gameObject);
        answerSlots.Clear();

        base.Cleanup();
    }

    protected override void Update()
    {
        if (!IsRunning || IsFinished) return;
        if (AllSlotsFilled()) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            OnTimeExpired();
        }
    }

    private void SpawnLetters()
    {
        // Очищаем старое
        foreach (var tile in spawnedTiles)
            if (tile != null) Destroy(tile.gameObject);
        spawnedTiles.Clear();

        foreach (var slot in answerSlots)
            if (slot != null) Destroy(slot.gameObject);
        answerSlots.Clear();

        float step = tileSize + tileSpacing;
        float totalWidth = currentWord.Length * tileSize + (currentWord.Length - 1) * tileSpacing;
        float startX = -totalWidth / 2f + tileSize / 2f;

        // Спавним слоты в answerContainer
        for (int i = 0; i < currentWord.Length; i++)
        {
            LetterSlotView slot = Instantiate(slotPrefab, answerContainer);
            RectTransform slotRect = slot.GetComponent<RectTransform>();
            slotRect.anchorMin = new Vector2(0.5f, 0.5f);
            slotRect.anchorMax = new Vector2(0.5f, 0.5f);
            slotRect.pivot = new Vector2(0.5f, 0.5f);
            slotRect.sizeDelta = new Vector2(tileSize, tileSize);
            slotRect.anchoredPosition = new Vector2(startX + i * step, 0);
            slot.Reset();
            answerSlots.Add(slot);
        }

        // Перемешиваем буквы
        char[] letters = currentWord.ToCharArray();
        Shuffle(letters);

        // Спавним буквы в scrambledContainer
        for (int i = 0; i < letters.Length; i++)
        {
            LetterTileView tile = Instantiate(letterTilePrefab, scrambledContainer);
            RectTransform tileRect = tile.GetComponent<RectTransform>();
            tileRect.anchorMin = new Vector2(0.5f, 0.5f);
            tileRect.anchorMax = new Vector2(0.5f, 0.5f);
            tileRect.pivot = new Vector2(0.5f, 0.5f);
            tileRect.sizeDelta = new Vector2(tileSize, tileSize);
            tileRect.anchoredPosition = new Vector2(startX + i * step, 0);
            tile.SetLetter(letters[i]);
            tile.OnPlaced += CheckWin;
            spawnedTiles.Add(tile);
        }
    }

    private void CheckWin()
    {
        if (!IsRunning || IsFinished) return;
        if (!AllSlotsFilled()) return;

        string assembled = "";
        foreach (var slot in answerSlots)
            assembled += slot.Letter;

        if (assembled == currentWord)
            Finish(MiniGameResult.Win);
        else
            Finish(MiniGameResult.Fail);
    }

    private bool AllSlotsFilled()
    {
        foreach (var slot in answerSlots)
            if (!slot.HasLetter) return false;
        return true;
    }

    protected override void OnTimeExpired()
    {
        Finish(MiniGameResult.Fail);
    }

    private static void Shuffle(char[] arr)
    {
        for (int i = arr.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }
    }
}