using UnityEngine;

[CreateAssetMenu(menuName = "Game/Mini Game Entry")]
public class MiniGameEntry : ScriptableObject
{
    public string gameId;
    public string displayName;
    public string gameRules;
    public GameObject prefab;

    [Tooltip("If true, global timer will pause during this mini-game")]
    public bool pauseTimer;
}