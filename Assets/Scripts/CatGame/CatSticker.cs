using UnityEngine;
using UnityEngine.UI;

public class CatSticker : MonoBehaviour
{
    private CatTapMiniGame game;
    private Image image;
    private RectTransform rect;

    public void Init(CatTapMiniGame g)
    {
        game = g;
        image = GetComponent<Image>();
        rect = GetComponent<RectTransform>();
    }

    public void OnClick()
    {
        game.OnCatClicked();
    }

    public void SetTired(Sprite tiredSprite)
    {
        image.sprite = tiredSprite;
    }

    public void Fall()
    {
        StartCoroutine(FallRoutine());
    }

    private System.Collections.IEnumerator FallRoutine()
    {
        float t = 0;
        Vector2 start = rect.anchoredPosition;

        while (t < 1f)
        {
            t += Time.deltaTime;
            rect.anchoredPosition = start + Vector2.down * t * 500f;
            yield return null;
        }

        Destroy(gameObject);
    }
}