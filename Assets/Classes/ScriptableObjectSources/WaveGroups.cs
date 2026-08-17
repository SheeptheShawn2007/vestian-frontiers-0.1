using UnityEngine;

[CreateAssetMenu( fileName = "New WaveGroup", menuName = "Waves/Wave Group" )]
public class WaveGroup : ScriptableObject
{
    public int EnemyID;
    public int Path;
    public int EnemyCount;
    public float Spacing;
    public float SpawnDelay;
}