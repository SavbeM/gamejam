using UnityEngine;

[CreateAssetMenu(menuName = "Game/MiniGames/Tap Truth Question")]
public class TapTruthQuestion : ScriptableObject
{
    [TextArea]
    public string questionText;

    public string[] answers = new string[4];

    [Range(0, 3)]
    public int correctAnswerIndex;
}