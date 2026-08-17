using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EntitySummoner : MonoBehaviour
{
    public static List<Enemy> EnemiesInGame;
    public static List<Transform> EnemiesInGameTransform;
    public static Dictionary<int, GameObject> EnemyPrefabs;
    public static Dictionary<int, Queue<Enemy>> EnemyObjectPools;

    private static bool IsInitialized = false;
    public static void Init()
    {
        if( !IsInitialized )
        {
            EnemyPrefabs = new Dictionary<int, GameObject>();
            EnemyObjectPools = new Dictionary<int, Queue<Enemy>>();
            EnemiesInGameTransform = new List<Transform>();
            EnemiesInGame = new List<Enemy>();

            //Basically goes through to the folder until it gets to Resrouces folder, then puts a / there so therefore typing "Enemies" in the parenthases lets us get to enemy stuff
            EnemySummonData[] Enemies = Resources.LoadAll<EnemySummonData>( "Enemies" );
            //Debug checks our enemy names
            //Debug.Log( Enemies[ 0 ].name );

            foreach( EnemySummonData enemy in Enemies )
            {
                //Empty object pools created for each ID
                EnemyPrefabs.Add( enemy.EnemyID, enemy.EnemyPrefab );
                EnemyObjectPools.Add( enemy.EnemyID, new Queue<Enemy>() );
            }

            IsInitialized = true;
        }
        else
        {
            Debug.Log( "ENTITIYSUMMONER: THIS CLASS IS ALREADY INITIALIZED" );
        }
    }

    public static Enemy SummonEnemy( int[] Data )
    {
        int EnemyID = Data[0];
        int EnemyPath = Data[1];
        Enemy SummonedEnemy;

        //Checks if enemy exists
        if (EnemyPrefabs.ContainsKey(EnemyID))
        {
            Queue<Enemy> ReferencedQueue = EnemyObjectPools[EnemyID];
            //Check if enemies in queue, if yes, intialize, if not then create new instance and then initialize
            if (ReferencedQueue.Count > 0)
            {
                //Dequeue enemy and insitialize
                SummonedEnemy = ReferencedQueue.Dequeue();
                SummonedEnemy.Path = EnemyPath;
                SummonedEnemy.Init();
                //Debug.Log( "INITIALIZED ENEMY!!!" );
                SummonedEnemy.gameObject.SetActive(true);
                SummonedEnemy.IsPooled = false;
            }
            else
            {
                //Instantiate new instance of enemy and initialize
                GameObject NewEnemy = Instantiate(EnemyPrefabs[EnemyID], GameLoopManager.ListOfNodePositions[EnemyPath][0], Quaternion.identity);
                SummonedEnemy = NewEnemy.GetComponent<Enemy>();
                SummonedEnemy.Path = EnemyPath;
                SummonedEnemy.Init();
                if (SummonedEnemy == null)
                {
                    Debug.LogError($"ENTITYSUMMONER: Enemy prefab with ID {EnemyID} is missing the Enemy component!");
                    Destroy(NewEnemy);
                    return null;
                }
            }
            Debug.Log($"Path assigned to {SummonedEnemy.ID}: {SummonedEnemy.Path}");
            Debug.Log($"Start index: {SummonedEnemy.NodeIndex}");
        }
        else
        {
            Debug.Log($"ENTITYSUMMONER: ENEMY WITH ID OF {EnemyID} DOES NOT EXIST!");
            return null;
        }

        EnemiesInGameTransform.Add( SummonedEnemy.transform );
        EnemiesInGame.Add( SummonedEnemy );
        //Debug.Log( $"SUMMONED ENEMY OF {EnemyID}!" );
        SummonedEnemy.SetID( EnemyID );
        return SummonedEnemy;
    }

    public static void RemoveEnemy(Enemy EnemyToRemove)
    {
        if (EnemyToRemove == null)
        {
            Debug.LogWarning("RemoveEnemy: Tried to remove a null enemy.");
            return;
        }
        if (EnemyToRemove.IsPooled)
        {
            Debug.LogWarning("RemoveEnemy: Enemy already in pool.");
            return;
        }

        EnemyToRemove.RemoveEffects();
        EnemyToRemove.ActiveEffects.Clear();

        EnemyToRemove.IsDead = true;
        EnemyToRemove.Health = 0;
        EnemyToRemove.Armor = 0;
        EnemyToRemove.NodeIndex = 0;
        EnemyToRemove.transform.SetParent(null);

        if (EnemiesInGame.Contains(EnemyToRemove))
        {
            EnemiesInGame.Remove(EnemyToRemove);
        }
        if (EnemiesInGameTransform.Contains(EnemyToRemove.transform))
        {
            EnemiesInGameTransform.Remove(EnemyToRemove.transform);
        }

        EnemyToRemove.IsPooled = true;
        EnemyObjectPools[EnemyToRemove.ID].Enqueue(EnemyToRemove);
        EnemyToRemove.gameObject.SetActive(false);
    }
}
