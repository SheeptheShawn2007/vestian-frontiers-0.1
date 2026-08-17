using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Rendering;
using UnityEngine.Timeline;

public class GameLoopManager : MonoBehaviour
{
    [SerializeField] private UpgradeMenuBehavior UpgradeMenu;
    public static List<TowerBehavior> TowersInGame;
    public static Vector3[][] ListOfNodePositions;
    public static float[][] ListOfNodeDistances;

    private static Queue<EnemyDamageData> DamageData;
    private static Queue<Enemy> EnemiesToRemove;
    private static Queue<int[]> EnemyIDsToSummon;
    private static Queue<TowerBehavior> TowersToRemove;
    private static Queue<TowerUpgradeRequest> UpgradeRequests;
    private static Queue<ApplyEffectData> EffectsQueue;

    private NativeArray<Vector3> FlatNodes;
    private NativeArray<int> PathStartIndices;
    private NativeArray<int> PathLengths;
    private PlayerStats PlayerStatistics;
    public Transform[] NodeParents;
    public WaveManager WaveManager;
    public bool LoopShouldEnd;
    
    List<Vector3> flatNodeList = new List<Vector3>();
    List<int> pathStartIndices = new List<int>();
    List<int> pathLengths = new List<int>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        EffectsQueue = new Queue<ApplyEffectData>();
        PlayerStatistics = FindAnyObjectByType<PlayerStats>();
        DamageData = new Queue<EnemyDamageData>();
        TowersInGame = new List<TowerBehavior>();
        EnemyIDsToSummon = new Queue<int[]>();
        EnemiesToRemove = new Queue<Enemy>();
        TowersToRemove = new Queue<TowerBehavior>();
        UpgradeRequests = new Queue<TowerUpgradeRequest>();
        ListOfNodePositions = new Vector3[NodeParents.Length][];
        ListOfNodeDistances = new float[NodeParents.Length][];
        EntitySummoner.Init();

        for (int i = 0; i < ListOfNodePositions.Length; i++)
        {
            Transform CurrNodeParent = NodeParents[i];
            Vector3[] NodePositions = new Vector3[CurrNodeParent.childCount];


            for (int j = 0; j < NodePositions.Length; j++)
            {
                NodePositions[j] = CurrNodeParent.GetChild(j).position;
            }

            ListOfNodePositions[i] = NodePositions;

            float[] NodeDistances = new float[CurrNodeParent.childCount - 1];
            for (int j = 0; j < NodeDistances.Length; j++)
            {
                NodeDistances[j] = Vector3.Distance(NodePositions[j], NodePositions[j + 1]);
            }

            ListOfNodeDistances[i] = NodeDistances;

            // InvokeRepeating( "SummonTest", 0f, 1f );
            // InvokeRepeating( "RemoveTest", 0f, 0.5f );
        }


        //Flattens the path nodes
        for (int i = 0; i < ListOfNodePositions.Length; i++)
        {
            pathStartIndices.Add(flatNodeList.Count);
            pathLengths.Add(ListOfNodePositions[i].Length);
            flatNodeList.AddRange(ListOfNodePositions[i]);
        }

        StartCoroutine(GameLoop());
    }

    private void CleanEffectsQueue()
    {
        int originalCount = EffectsQueue.Count;
        var cleanedQueue = new Queue<ApplyEffectData>(originalCount);

        while (EffectsQueue.Count > 0)
        {
            var data = EffectsQueue.Dequeue();
            if (data.TargetedEnemy != null && !data.TargetedEnemy.IsDead)
            {
                cleanedQueue.Enqueue(data);
            }
        }

        EffectsQueue = cleanedQueue;
    }

    IEnumerator GameLoop()
    {
        while( LoopShouldEnd == false )
        {
            //Spawn Enemies
            int count = EnemyIDsToSummon.Count;
            if( count > 0 )
            {
                for( int i = 0; i < count; i++ )
                {
                    EntitySummoner.SummonEnemy( EnemyIDsToSummon.Dequeue() );
                }
            }

            //Upgrade Towers

            if( UpgradeRequests.Count > 0 )
            {
                Debug.Log( "Upgrade Request Processing" );
                for( int i = 0; i < UpgradeRequests.Count; i++ )
                {
                    var request = UpgradeRequests.Dequeue();
                    var tower = request.Tower;
                    var branch = request.Branch;

                    if( tower == null ) continue;

                    GameObject upgradePrefab = null;
                    switch ( branch )
                    {
                        case 1: upgradePrefab = tower.Upgrade1; break;
                        case 2: upgradePrefab = tower.Upgrade2; break;
                        case 3: upgradePrefab = tower.Upgrade3; break;
                    }

                    if( upgradePrefab != null )
                    {
                        int upgradeCost = upgradePrefab.GetComponent<TowerBehavior>().SummonCost;
                        var playerStats = tower.GetComponent<PlayerStats>() ?? FindAnyObjectByType<PlayerStats>();

                        if (playerStats.GetMoney() >= upgradeCost)
                        {
                            playerStats.ChangeMoney(-upgradeCost);
                            GameObject newTower = GameObject.Instantiate(upgradePrefab, tower.transform.position, tower.transform.rotation);
                            TowerBehavior NewTower = newTower.GetComponent<TowerBehavior>();
                            TowersInGame.Add(NewTower);
                            EnqueueTowerToRemove(tower);
                            tower.IsDead = true;
                            UpgradeMenu.UpdateSelectedTower(NewTower);
                            UpgradeMenu.OpenUpgradeMenu(NewTower);
                        }
                    }
                }
            }

            //Remove Towers
            count = TowersToRemove.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    TowerBehavior towerToRemove = TowersToRemove.Dequeue();
                    TowersInGame.Remove(towerToRemove);
                    Destroy(towerToRemove.gameObject);
                }
            }

            //Move Enemies
            NativeArray<Vector3> flatNodesNative = new NativeArray<Vector3>(flatNodeList.ToArray(), Allocator.TempJob);
            NativeArray<int> pathStartIndicesNative = new NativeArray<int>(pathStartIndices.ToArray(), Allocator.TempJob);
            NativeArray<int> pathLengthsNative = new NativeArray<int>(pathLengths.ToArray(), Allocator.TempJob);
            NativeArray<Vector3>[] NativePaths = new NativeArray<Vector3>[ListOfNodePositions.Length];
                for (int i = 0; i < ListOfNodePositions.Length; i++)
                {
                    NativePaths[i] = new NativeArray<Vector3>(ListOfNodePositions[i], Allocator.TempJob);
                }
            NativeArray<int> PathIndices = new NativeArray<int>(EntitySummoner.EnemiesInGame.Count, Allocator.TempJob);
            NativeArray<int> NodeIndices = new NativeArray<int>( EntitySummoner.EnemiesInGame.Count, Allocator.TempJob );
            NativeArray<float> EnemySpeeds = new NativeArray<float>( EntitySummoner.EnemiesInGame.Count, Allocator.TempJob );
            TransformAccessArray EnemyAccess = new TransformAccessArray( EntitySummoner.EnemiesInGameTransform.ToArray(), 2 );

            for( int i = 0; i < EntitySummoner.EnemiesInGame.Count; i++ )
            {
                PathIndices[i] = EntitySummoner.EnemiesInGame[i].Path;
                EnemySpeeds[i] = EntitySummoner.EnemiesInGame[i].Speed;
                NodeIndices[i] = EntitySummoner.EnemiesInGame[i].NodeIndex;
            }

            var moveJob = new MoveEnemiesJob
            {
                FlatNodes = flatNodesNative,
                PathStartIndices = pathStartIndicesNative,
                PathLengths = pathLengthsNative,
                NodeIndex = NodeIndices,
                EnemySpeed = EnemySpeeds,
                PathIndex = PathIndices,
                deltaTime = Time.deltaTime
            };

            JobHandle jobHandle = moveJob.Schedule(EnemyAccess);
            jobHandle.Complete();

            flatNodesNative.Dispose();
            pathStartIndicesNative.Dispose();
            pathLengthsNative.Dispose();

            count = EntitySummoner.EnemiesInGame.Count;
            for (int i = 0; i < count; i++)
            {
                Enemy CurrEnemy = EntitySummoner.EnemiesInGame[i];
                CurrEnemy.NodeIndex = NodeIndices[i];

                if (CurrEnemy.NodeIndex == ListOfNodePositions[CurrEnemy.Path].Length)
                {
                    PlayerStatistics.ChangeLeak(EntitySummoner.EnemiesInGame[i].LeakValue);
                    EnqueueEnemyToRemove(EntitySummoner.EnemiesInGame[i]);
                    Debug.Log($"ENEMIES LEFT TO REMOVE {EnemiesToRemove.Count}");
                }
            }

            for (int i = 0; i < NativePaths.Length; i++)
            {
                NativePaths[i].Dispose();
            }
            NodeIndices.Dispose();
            EnemySpeeds.Dispose();
            PathIndices.Dispose();
            EnemyAccess.Dispose();

            //Tick Towers

            EffectsQueue.Clear();

            foreach (TowerBehavior tower in TowersInGame)
            {
                tower.Target = TowerTargeting.GetTarget(tower, tower.CurrentTargetType);
                tower.Tick();
            }

            //Apply Effects

            CleanEffectsQueue();

            int effectCount = EffectsQueue.Count;
            for (int i = 0; i < effectCount; i++)
            {
                ApplyEffectData effectData = EffectsQueue.Dequeue();

                if (effectData.TargetedEnemy == null || effectData.TargetedEnemy.IsDead)
                    continue; // skip dead or null enemies

                Effect existingEffect = effectData.TargetedEnemy.ActiveEffects.Find(e => e.EffectName == effectData.EffectToApply.EffectName);
                if (existingEffect == null)
                {
                    effectData.TargetedEnemy.ActiveEffects.Add(effectData.EffectToApply);
                }
                else
                {
                    existingEffect.Duration += effectData.EffectToApply.Duration;
                }
            }

            //Damage Enemies

            if (DamageData.Count > 0)
            {
                for (int i = 0; i < DamageData.Count; i++)
                {
                
                    EnemyDamageData CurrentDamageData = DamageData.Dequeue();
                    Enemy CurrentTargetedEnemy = CurrentDamageData.TargetedEnemy;
                    float CurrentArmor = CurrentTargetedEnemy.Armor;
                    float CurrentTotalDamage = CurrentDamageData.TotalDamage;

                    Debug.Log($"Starting damage {CurrentTotalDamage}");

                    
                    if (CurrentArmor > 0)
                    {
                        if (CurrentArmor > CurrentDamageData.ArmorPen) //Handles Armor Penetration
                        {
                            CurrentTotalDamage -= CurrentArmor;
                            CurrentTotalDamage += CurrentDamageData.ArmorPen;
                        }
                        Debug.Log($"damage2 check {CurrentTotalDamage}");
                        CurrentTargetedEnemy.Armor -= CurrentDamageData.ArmorShred; //Handles Armor Shred
                    }
                    if (CurrentArmor < 0)
                    {
                        CurrentTargetedEnemy.Armor = 0;
                    }
                    if (CurrentTotalDamage > 0)
                    {
                        CurrentTargetedEnemy.Health -= CurrentTotalDamage;
                        Debug.Log($"Dealt {CurrentTotalDamage}");
                    }
                    
                
                    if (CurrentTargetedEnemy.Health <= 0f)
                    {
                        Debug.Log("Enemy Kill Attempting Register");
                        if (CurrentTargetedEnemy != null && !CurrentTargetedEnemy.IsDead)
                        {
                            PlayerStatistics.ChangeMoney(CurrentTargetedEnemy.Bounty);
                        }
                        Debug.Log("Killed Enemy");
                        CurrentTargetedEnemy.IsDead = true;
                        EnqueueEnemyToRemove(CurrentDamageData.TargetedEnemy);
                    }
                }
            }
            
            //Tick Enemies

            foreach(Enemy CurrentEnemy in EntitySummoner.EnemiesInGame)
            {
                CurrentEnemy.Tick();
            }

            //Remove Enemies

            if (EnemiesToRemove.Count > 0)
            {
                for (int i = 0; i < EnemiesToRemove.Count; i++)
                {
                    Enemy EnemyToRemove = EnemiesToRemove.Dequeue();
                    EntitySummoner.RemoveEnemy(EnemyToRemove);
                }
            }

            yield return null;
        }
    }

    public static void EnqueueEffectsToApply(ApplyEffectData EffectData)
    {
        EffectsQueue.Enqueue(EffectData);
    }
    public static void EnqueueDamageData(EnemyDamageData damageData)
    {
        DamageData.Enqueue(damageData);
    }
    public static void EnqueueEnemyIDToSummon( int ID, int Path )
    {
        int[] EnemyToSummonData = new int[] {ID, Path};
        EnemyIDsToSummon.Enqueue( EnemyToSummonData );
    }
    public static void EnqueueEnemyToRemove( Enemy enemy )
    {
        EnemiesToRemove.Enqueue( enemy );
    }
    public static void EnqueueTowerToRemove( TowerBehavior tower )
    {
        if( !tower.IsDead )
            TowersToRemove.Enqueue( tower );
    }
    public static void EnqueueUpgradeRequest( TowerBehavior tower, int branch )
    {
        Debug.Log( "Tower Request Enqueueing" );
        TowerUpgradeRequest CurrentTowerRequest = new TowerUpgradeRequest( tower, branch );
        UpgradeRequests.Enqueue( CurrentTowerRequest );
    }
}

public class Effect
{
    public Effect(string effectName, float damageRate, float damage, float expireTime, float shred, float pen, float delay, float slow, float cripple, float mark, GameObject particle)
    {
        EffectName = effectName;
        DamageRate = damageRate;
        Damage = damage;
        OriginalDuration = expireTime;
        Duration = expireTime;  // current countdown
        Shred = shred;
        Penetration = pen;
        OriginalDelay = delay;
        Delay = delay;          // current countdown
        Particles = particle;
        Slow = slow;
        Cripple = cripple;
        Marked = mark;
    }

    public Effect Clone()
    {
        var clonedEffect = new Effect(EffectName, DamageRate, Damage, OriginalDuration, Shred, Penetration, OriginalDelay, Slow, Cripple, Marked, Particles);
        clonedEffect.SpawnedParticle = null;
        return clonedEffect;
    }

    public string EffectName;
    public float Damage;

    // Track original and current separately
    public float OriginalDuration;
    public float Duration;
    public float Slow;
    public float DamageRate;
    public float OriginalDelay;
    public float Delay;
    public float Shred;
    public float Penetration;
    public float Cripple;
    public float Marked;
    public GameObject Particles;

    [System.NonSerialized]
    public GameObject SpawnedParticle;
}

public struct ApplyEffectData
{
    public ApplyEffectData(Enemy enemyToAffect, Effect effectToApply)
    {
        TargetedEnemy = enemyToAffect;
        EffectToApply = effectToApply;
    }
    public Enemy TargetedEnemy;
    public Effect EffectToApply;
}

public struct EnemyDamageData
{
    public EnemyDamageData(Enemy target, float damage, float shred, float penetration)
    {
        TargetedEnemy = target;
        TotalDamage = damage;
        ArmorShred = shred;
        ArmorPen = penetration;
    }
    public Enemy TargetedEnemy;
    public float TotalDamage;
    public float ArmorShred;
    public float ArmorPen;
}

public struct MoveEnemiesJob : IJobParallelForTransform
{
    [ReadOnly] public NativeArray<Vector3> FlatNodes;
    [ReadOnly] public NativeArray<int> PathStartIndices;
    [ReadOnly] public NativeArray<int> PathLengths;

    public NativeArray<int> NodeIndex;
    [ReadOnly] public NativeArray<float> EnemySpeed;
    [ReadOnly] public NativeArray<int> PathIndex;

    public float deltaTime;

    public void Execute(int index, TransformAccess transform)
    {
        int path = PathIndex[index];
        int node = NodeIndex[index];
        int pathStart = PathStartIndices[path];
        int pathLength = PathLengths[path];

        if (node < pathLength)
        {
            Vector3 positionToMoveTo = FlatNodes[pathStart + node];
            Vector3 direction = positionToMoveTo - transform.position;

            if (direction != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(direction);

            transform.position = Vector3.MoveTowards(transform.position, positionToMoveTo, EnemySpeed[index] * deltaTime);

            if (transform.position == positionToMoveTo)
                NodeIndex[index]++;
        }
    }
}

public struct TowerUpgradeRequest
{
    public TowerBehavior Tower;
    public int Branch;

    public TowerUpgradeRequest(TowerBehavior tower, int branch)
    {
        Tower = tower;
        Branch = branch;
    }
}