using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainDamage : MonoBehaviour, IDamageMethod
{
    [SerializeField] private GameObject[] Barrels;
    [SerializeField] private int MaxChain;
    [SerializeField] private float ChainRange;
    [SerializeField] private float ChainRatio;
    [SerializeField] private LineRenderer ChainRender;
    [SerializeField] private Material ChainMaterial;
    [SerializeField] private AudioClip ChainSound;
    private float Damage;
    private float Firerate;
    private float Delay;
    private float Accuracy;
    private float Shred;
    private float Penetration;
    private int BarrelIndex = 0;
    private float MissSize = 3;
    private float Spread = 0;

    private TowerBehavior Tower;
    private LineRenderer Laser;
    private TowerGunSound TowerSound;
    private List<Enemy> AlreadyHit;

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
        TowerSound = Tower.GetComponent<TowerGunSound>();

        ChainRender.startWidth = 0.2f;
        ChainRender.endWidth = 0.2f;
        ChainRender.material = ChainMaterial;
        ChainRender.enabled = false;
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

            if (Tower.GetComponent<TowerAnimation>() != null)
                Debug.Log("Recoil anim");
            Tower.GetComponent<TowerAnimation>().Fire();

            float HitCheck = Random.Range(0, 100); //Check for Accuracy
            if (Tower.Accuracy - Target.DodgeRate >= HitCheck) //Handles Accuracy
            {
                GameLoopManager.EnqueueDamageData(new EnemyDamageData(Target, Damage, Shred, Penetration));
                Spread = 0;
                AlreadyHit = new List<Enemy> { Target };
                StartCoroutine(ChainToNextEnemy(Target.transform.position, Damage * ChainRatio, AlreadyHit, MaxChain - 1));
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
            {
                BarrelIndex = 0;
            }
        }
    }

    private IEnumerator ChainToNextEnemy(Vector3 CurrentPos, float ChainDamage, List<Enemy> AlreadyHit, int RemainingChains)
    {
        if (RemainingChains <= 0) yield break;

        yield return new WaitForSeconds(0.1f);

        Enemy nextTarget = FindClosestEnemy(CurrentPos, ChainRange, AlreadyHit);
        if (nextTarget == null) yield break;

        GameLoopManager.EnqueueDamageData(new EnemyDamageData(nextTarget, ChainDamage, Shred, Penetration));
        AlreadyHit.Add(nextTarget);

        DrawLightning(CurrentPos, nextTarget.transform.position);
    }

    private IEnumerator TurnOffLaser()
    {
        yield return new WaitForSeconds(0.1f);
        Laser.enabled = false;
    }

    private void DrawLightning(Vector3 from, Vector3 to)
    {
        ChainRender.useWorldSpace = true;
        ChainRender.enabled = true;
        ChainRender.positionCount = 2;
        ChainRender.SetPosition(0, from);
        ChainRender.SetPosition(1, to);
        StartCoroutine(ClearLightning());
    }

    private IEnumerator ClearLightning()
    {
        yield return new WaitForSeconds(0.05f);
        ChainRender.enabled = false;
        ChainRender.positionCount = 0;
    }

    private Enemy FindClosestEnemy(Vector3 origin, float range, List<Enemy> excludeList)
    {
        Collider[] hits = Physics.OverlapSphere(origin, range, LayerMask.GetMask("Enemy")); // assumes enemies are on "Enemy" layer

        Enemy closest = null;
        float closestDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy == null || excludeList.Contains(enemy)) continue;

            float dist = Vector3.Distance(origin, enemy.transform.position);
            if (dist < closestDist)
            {
                closest = enemy;
                closestDist = dist;
            }
        }

        return closest;
    }
}
