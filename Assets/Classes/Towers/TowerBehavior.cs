using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class TowerBehavior : MonoBehaviour
{
    public LayerMask EnemiesLayer;
    public Enemy Target;
    public Transform TowerPivot;

    public float Damage;
    public float Firerate;
    public float Range;
    public float Accuracy;
    public float Shred;
    public float Penetration;
    public int SummonCost;
    public int Refund;
    public GameObject Upgrade1;
    public GameObject Upgrade2;
    public GameObject Upgrade3;
    public bool IsDead = false;
    public bool Firing = false;
    public string UpgradeName;
    public string Name;
    public string Description;

    public TowerTargeting.TargetType CurrentTargetType;
    private float Delay;
    private PlayerStats PlayerStatistics;
    private IDamageMethod CurrentDamageMethodClass;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayerStatistics = FindAnyObjectByType<PlayerStats>();
        CurrentDamageMethodClass = GetComponent<IDamageMethod>();

        if (CurrentDamageMethodClass == null)
        {
            Debug.Log("No damage class attached to this tower");
        }
        else
        {
            CurrentDamageMethodClass.Init(Damage, Firerate, Accuracy, Shred, Penetration);
        }

        Delay = 0;
        CurrentTargetType = TowerTargeting.TargetType.First;
    }

    public void Tick()
    {
        if (Target == null || CurrentDamageMethodClass == null)
        {
            Debug.Log("Null Target");
            return;
        }

        CurrentDamageMethodClass.DamageTick(Target);
    }

    public void RotateTowardsTarget()
    {
        if (Target == null) return;

        Vector3 direction = Target.transform.position - TowerPivot.position;
        direction.y = 0; // flatten vertical

        if (direction != null )
        {
            Debug.Log("Turning Tower");
 
            TowerPivot.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, Range);

        if (Target != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, Target.transform.position);
            }
    }    

    public void SellTower()
    {
        PlayerStatistics.ChangeMoney(Refund);
        GameLoopManager.TowersInGame.Remove(this);
        GameLoopManager.EnqueueTowerToRemove(this);
    }

    public float GetDelay()
    {
        return Delay;
    }
}
