using UnityEngine;

public enum SortCategory
{
    Edible,
    Inedible
}

[CreateAssetMenu(menuName = "Game/Sort Garbage/Item Data")]
public class SortGarbageItemData : ScriptableObject
{
    [SerializeField] private string itemId;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;
    [SerializeField] private SortCategory correctCategory;

    public string ItemId => itemId;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public SortCategory CorrectCategory => correctCategory;
}