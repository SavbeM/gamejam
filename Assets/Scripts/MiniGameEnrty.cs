using UnityEngine;

[CreateAssetMenu(menuName = "Game/Mini Game Entry")]
public class MiniGameEntry : ScriptableObject
{
    public string gameId;
    public string displayName;
    public GameObject prefab;
    public float timeLimit = 5f;
}