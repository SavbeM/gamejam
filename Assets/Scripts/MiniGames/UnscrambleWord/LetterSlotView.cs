using UnityEngine;
using UnityEngine.UI;

public class LetterSlotView : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Image outlineImage;

    private LetterTileView occupant;

    public bool HasLetter => occupant != null;
    public char Letter => occupant != null ? occupant.Letter : '\0';
    public LetterTileView Occupant => occupant;

    public void Accept(LetterTileView tile)
    {
        occupant = tile;
        SetHighlight(true);
    }

    public void Clear()
    {
        occupant = null;
        SetHighlight(false);
    }

    public void Reset()
    {
        occupant = null;
        SetHighlight(false);
    }

    private void SetHighlight(bool filled)
    {
        if (outlineImage == null) return;
        Color c = outlineImage.color;
        c.a = filled ? 0.9f : 0.35f;
        outlineImage.color = c;
    }
}