using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CatTapMiniGame : MiniGameBase
{
    [Header("Setup")]
    public GameObject catPrefab;
    public Transform spawnArea;

    [Header("Sprites")]
    public Sprite[] catSprites;
    public Sprite tiredSprite;

    [Header("Game Settings")]
    public int totalCats = 5;
    public int needToWin = 4;
    public float catLifetime = 1f;

    private int clicked = 0;
    private bool gameEnded = false;

    private List<CatSticker> activeCats = new List<CatSticker>();

    public override void Begin()
    {
        base.Begin();
        StartCoroutine(GameLoop());
    }

    IEnumerator GameLoop()
    {
        yield return StartCoroutine(Countdown());

        for (int i = 0; i < totalCats; i++)
        {
            SpawnCat();
            yield return new WaitForSeconds(catLifetime);
        }

        EndGame();
    }

    IEnumerator Countdown()
    {
        Debug.Log("3");
        yield return new WaitForSeconds(1f);

        Debug.Log("2");
        yield return new WaitForSeconds(1f);

        Debug.Log("1");
        yield return new WaitForSeconds(1f);
    }

    void SpawnCat()
    {
        GameObject cat = Instantiate(catPrefab, spawnArea);

        RectTransform rt = cat.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(
            Random.Range(-300, 300),
            Random.Range(-400, 400)
        );

        CatSticker sticker = cat.GetComponent<CatSticker>();
        sticker.Init(this);

        Image img = cat.GetComponent<Image>();
        img.sprite = catSprites[Random.Range(0, catSprites.Length)];

        Button btn = cat.GetComponent<Button>();
        btn.onClick.AddListener(sticker.OnClick);

        activeCats.Add(sticker);

        StartCoroutine(RemoveAfterTime(sticker, catLifetime));
    }

    IEnumerator RemoveAfterTime(CatSticker cat, float time)
    {
        yield return new WaitForSeconds(time);

        if (cat != null)
        {
            activeCats.Remove(cat);
            Destroy(cat.gameObject);
        }
    }

    public void OnCatClicked()
    {
        if (gameEnded) return;

        clicked++;

        foreach (var cat in activeCats)
        {
            if (cat != null)
            {
                cat.SetTired(tiredSprite);
                cat.Fall();
            }
        }

        activeCats.Clear();
    }

    void EndGame()
    {
        gameEnded = true;

        bool win = clicked >= needToWin;

        if (win)
            Finish(MiniGameResult.Win);
        else
            Finish(MiniGameResult.Fail);
    }
}