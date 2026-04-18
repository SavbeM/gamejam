using UnityEngine;

[CreateAssetMenu(menuName = "Game/Unscramble Word/Word Data")]
public class UnscrambleWordData : ScriptableObject
{
    [SerializeField] private string word;

    // Слово в верхнем регистре, без пробелов
    public string Word => word.ToUpper().Trim();
}
