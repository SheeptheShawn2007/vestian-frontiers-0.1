using System.Collections;
using UnityEngine;

public class StandardEffectDamage : MonoBehaviour, IDamageMethod
{
    [SerializeField] private GameObject[] Barrels;
    [SerializeField] private Effects Effect;
    [SerializeField] private GameObject Particle;
    private Effect CurrEffect;
    private float Damage;
    private float Firerate;
    private float Delay;
    private float Accuracy;
    private float Shred;
    private float Penetration;
    private int BarrelIndex = 0;
    private float MissSize = 2;
    private float Spread = 0;

    private TowerBehavior Tower;
    private LineRenderer Laser;
    private TowerGunSound TowerSound;

    public void Init(float damage, float firerate, float accuracy, float shred, float penetration)
    {
        Damage = damage;
        Firerate = firerate;
        Delay = 1 / Firerate;
        Accuracy = accuracy;
        Shred = shred;
        Penetration = penetration;
        Tower = GetComponent<TowerBehavior>();
        Laser = Tower.GetComponent<TowerLaserRender>().GetComponent<LineRenderer>();
        CurrEffect = new Effect(Effect.EffectName, Effect.DamageRate, Effect.Damage, Effect.Duration, Effect.Shred, Effect.Penetration, Effect.Delay, Effect.Slow, Effect.Cripple, Effect.Marked, Particle);
        TowerSound = Tower.GetComponent<TowerGunSound>();

        Debug.Log(CurrEffect.EffectName);
    }

    public void DamageTick(Enemy Target)
    {
        if (Target)
        {
            if (Delay > 0f)
            {
                Delay -= Time.deltaTime;
                return;
            }

            float HitCheck = Random.Range(0, 100); //Check for Accuracy
            if (Tower.Accuracy - Target.DodgeRate >= HitCheck) //Handles Accuracy
            {
                GameLoopManager.EnqueueDamageData(new EnemyDamageData(Target, Damage, Shred, Penetration));
                GameLoopManager.EnqueueEffectsToApply(new ApplyEffectData(Target, CurrEffect.Clone()));
                Spread = 0;
            }
            else
            {
                Spread = MissSize;
            }

            Delay = 1f / Firerate;
            Tower.RotateTowardsTarget();

            Laser.enabled = true;
            StartCoroutine(TurnOffLaser());
            Laser.SetPosition(0, Barrels[BarrelIndex].transform.position);
            Laser.SetPosition(1, Target.transform.position + Vector3.up * (2 + Spread));

            TowerSound.ShootSound();

            BarrelIndex++;
            if (Barrels.Length <= BarrelIndex)
                BarrelIndex = 0;

            if (Tower.GetComponent<TowerAnimation>() != null)
                Tower.GetComponent<TowerAnimation>().Fire();
        }
    }

    private IEnumerator TurnOffLaser()
    {
        yield return new WaitForSeconds(0.1f);
        Laser.enabled = false;
    }
}
