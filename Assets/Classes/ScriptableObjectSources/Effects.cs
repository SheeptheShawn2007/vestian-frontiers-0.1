using UnityEngine;

[CreateAssetMenu(fileName = "Effects", menuName = "Effects/Effect")]
public class Effects : ScriptableObject
{
    public string EffectName;
    public float Damage;
    public float Duration;
    public float DamageRate;
    public float Shred;
    public float Penetration;
    public float Delay;
    public float Slow;
    public float Cripple;
    public float Marked;
    public GameObject ParticlePrefab;
}
