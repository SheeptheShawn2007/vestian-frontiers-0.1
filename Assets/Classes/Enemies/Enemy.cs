using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Enemy : MonoBehaviour
{
    public float MaxHealth;
    public float MaxArmor;
    public float Health;
    public float Armor;
    public float MaxSpeed;
    public float Speed;
    public float MaxDodgeRate;
    public float DodgeRate;
    public float Resist;
    public int ID;
    public int NodeIndex;
    public int Bounty;
    public int Path;
    public int LeakValue;
    public bool IsDead = false;
    public bool IsPooled = false;
    public List<Effect> ActiveEffects;
    private float SlowDebuffStack;
    private float TotalCripple;
    private float TotalMark;

    void Awake()
    {
        if (ActiveEffects == null)
            ActiveEffects = new List<Effect>();
    }

    public void Init()
    {
        ActiveEffects = new List<Effect>();

        NodeIndex = 0;
        Health = MaxHealth;
        Armor = MaxArmor;
        transform.position = GameLoopManager.ListOfNodePositions[Path][0];
        IsDead = false;
        IsPooled = false;
        Speed = MaxSpeed;
        DodgeRate = MaxDodgeRate;
    }

    public void SetID(int newID)
    {
        ID = newID;
    }

    public void Tick()
    {
        DodgeRate = MaxDodgeRate;
        TotalCripple = 0;
        TotalMark = 0;
        for (int i = 0; i < ActiveEffects.Count; i++)
        {
            var CurrEffect = ActiveEffects[i];
            SlowDebuffStack += CurrEffect.Slow;
            TotalCripple += CurrEffect.Cripple;
            TotalMark += CurrEffect.Marked;
            if (CurrEffect.Particles != null && CurrEffect.SpawnedParticle == null)
            {
                CurrEffect.SpawnedParticle = GameObject.Instantiate(CurrEffect.Particles, transform);
                CurrEffect.SpawnedParticle.transform.localPosition = Vector3.up * 1f;
            }

            if (CurrEffect.Duration > 0f && CurrEffect != null)
            {
                if (CurrEffect.Delay > 0f)
                {
                    CurrEffect.Delay -= Time.deltaTime;
                }
                else
                {
                    GameLoopManager.EnqueueDamageData(new EnemyDamageData(this, CurrEffect.Damage, CurrEffect.Shred, CurrEffect.Penetration));
                    CurrEffect.Delay = 1f / CurrEffect.DamageRate;
                }
            }

            if (Resist == 0)
            {
                CurrEffect.Duration -= Time.deltaTime;
            }
            else
            {
                CurrEffect.Duration -= Time.deltaTime*(1/Resist);
            }
        }

        Speed = MaxSpeed * ( 1 - SlowDebuffStack );
        if (Speed < 0)
        {
            Speed = 0;
        }
        if (TotalCripple > DodgeRate)
            {
                TotalCripple = DodgeRate;
            }
        DodgeRate -= TotalCripple;
        DodgeRate -= TotalMark;
        Debug.Log("Current Speed " + Speed);
        SlowDebuffStack = 0;

        for (int i = ActiveEffects.Count - 1; i >= 0; i--)
        {
            if (ActiveEffects[i].Duration <= 0f)
            {
                if (ActiveEffects[i].SpawnedParticle != null)
                {
                    Destroy(ActiveEffects[i].SpawnedParticle);
                }
                ActiveEffects.RemoveAt(i);
            }
        }
    }

    public void RemoveEffects()
    {
        if (ActiveEffects == null) return;
        for (int i = ActiveEffects.Count - 1; i >= 0; i--)
        {
            if (ActiveEffects[i].SpawnedParticle != null)
            {
                Destroy(ActiveEffects[i].SpawnedParticle);
            }
        }
        ActiveEffects.Clear();
        SlowDebuffStack = 0;
    }
}
