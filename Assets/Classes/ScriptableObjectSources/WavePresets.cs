using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu( fileName = "WavePreset", menuName = "Wave System/Wave Preset" )]
public class WavePreset : ScriptableObject
{
    public List<WaveGroup> FullWave;
}