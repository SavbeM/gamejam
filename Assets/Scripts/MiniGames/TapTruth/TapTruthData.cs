using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/MiniGames/Tap Truth Data")]
public class TapTruthData : ScriptableObject
{
    public List<TapTruthQuestion> questions = new();
}