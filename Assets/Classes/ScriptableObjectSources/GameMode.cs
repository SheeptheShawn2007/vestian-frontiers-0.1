using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu( menuName = "Game Modes/New Game Mode" )]
public class GameMode : ScriptableObject
{
    public string modeName;
    public List<WavePreset> WavePresets;
}