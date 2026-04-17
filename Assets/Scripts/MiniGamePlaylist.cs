using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Mini Game Playlist")]
public class MiniGamePlaylist : ScriptableObject
{
    public List<MiniGameEntry> games = new();
}