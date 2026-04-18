using System.Collections.Generic;
using UnityEngine;

public class UnscrambleWordMiniGame : MiniGameBase
{
    [Header("Data")]
    [SerializeField] private List<UnscrambleWordData> words = new();

    [Header("Refs")]
    [SerializeField] private Transform scrambledContainer;
    [SerializeField] private Transform answerContainer;
    [SerializeField] private LetterTileView letterTilePrefab;
    [SerializeField] private LetterSlotView slotPrefab;

    [Header("Layout")]
    [SerializeField] private float tileSize = 60f;
    [SerializeField] private float tileSpacing = 10f;

    [Header("Win Condition")]
    [SerializeField] private int wordsToWin = 5;

    private string currentWord;
    private List<LetterTileView> spawnedTiles = new();
    private List<LetterSlotView> answerSlots = new();
    private List<UnscrambleWordData> remainingWords = new();
    private int solvedCount = 0;

    public override void Setup(System.Action<MiniGameResult> onFinished)
    {
        base.Setup(onFinished);

        if (words == null || words.Count == 0)
        {
            Debug.LogError("UnscrambleWordMiniGame: words list is empty.");
            return;
        }

        solvedCount = 0;
        remainingWords = new List<UnscrambleWordData>(words);
        ShuffleWords(remainingWords);
        SpawnNextWord();
    }

    public override void Cleanup()
    {
        ClearTiles();
        ClearSlots();
        base.Cleanup();
    }

    private void SpawnLetters()
    {
        ClearTiles();
        ClearSlots();

        if (remainingWords.Count == 0)
        {
            remainingWords = new List<UnscrambleWordData>(words);
            ShuffleWords(remainingWords);
        }

        int index = Random.Range(0, remainingWords.Count);
        currentWord = remainingWords[index].Word;
        remainingWords.RemoveAt(index);

        timeLeft = duration;

        float step = tileSize + tileSpacing;
        float totalWidth = currentWord.Length * tileSize + (currentWord.Length - 1) * tileSpacing;
        float startX = -totalWidth / 2f + tileSize / 2f;

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

        char[] letters = currentWord.ToCharArray();
        Shuffle(letters);

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
            tile.OnPlaced += CheckWord;
            spawnedTiles.Add(tile);
        }
    }

    private void CheckWord()
    {
        if (!IsRunning || IsFinished) return;
        if (!AllSlotsFilled()) return;

        string assembled = "";
        foreach (var slot in answerSlots)
            assembled += slot.Letter;

        if (assembled == currentWord)
        {
            solvedCount++;
            Debug.Log($"Solved {solvedCount}/{wordsToWin}");

            if (solvedCount >= wordsToWin)
                Finish(MiniGameResult.Win);
            else
                SpawnNextWord();
        }
    }

    private bool AllSlotsFilled()
    {
        foreach (var slot in answerSlots)
            if (!slot.HasLetter) return false;
        return true;
    }

    private static void Shuffle(char[] arr)
    {
        for (int i = arr.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }
    }

    private static void ShuffleWords(List<UnscrambleWordData> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}