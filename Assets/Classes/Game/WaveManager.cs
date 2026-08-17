using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] private PlayerStats PlayerStats;
    public WaveButton NextWaveButton;
    public List<GameMode> Gamemodes;
    List<WavePreset> Gamemode;
    public int WaveIndex = -1;

    private int GroupsRemainingToSpawn = 0;
    private bool SpawningComplete;

    public void StartMode( int Mode )
    {
        Gamemode = Gamemodes[ Mode ].WavePresets;
        Debug.Log( $"Starting { Gamemodes[ Mode ].modeName  } mode" );
        StartWave();
    }

    public void StartWave()
    {
        WaveIndex++;
        GroupsRemainingToSpawn = Gamemode[ WaveIndex ].FullWave.Count;
        SpawningComplete = false;
        PlayerStats.ChangeWave(1);
        Debug.Log( $"Starting wave { WaveIndex + 1 }" );
        if ( WaveIndex >= Gamemode.Count )
        {
            Debug.Log("No more waves!"); //Game ending screen?
            return;
        }

        List<WaveGroup> waveGroups = Gamemode[WaveIndex].FullWave;
        
        foreach ( WaveGroup group in waveGroups )
        {
            Debug.Log( "Initiating spawn group" );
            StartCoroutine( SpawnGroup( group ) );
        }

        StartCoroutine( WaitForWaveToEnd() );
    }

    private IEnumerator SpawnGroup( WaveGroup group )
    {
        Debug.Log( "Beginning spawn group" );
        yield return new WaitForSeconds(group.SpawnDelay);
        for( int j = 0; j < group.EnemyCount; j++ )
        {
            GameLoopManager.EnqueueEnemyIDToSummon( group.EnemyID, group.Path );
            yield return new WaitForSeconds( group.Spacing );
        }

        GroupsRemainingToSpawn--;
        if (GroupsRemainingToSpawn <= 0)
        {
            SpawningComplete = true;
        }
    }

    private IEnumerator WaitForWaveToEnd()
    {
        // Wait until no enemies are left
        while ( EntitySummoner.EnemiesInGame.Count > 0 || !SpawningComplete )
        {
            yield return null; // wait 1 frame
        }

        // All enemies are gone; show the next wave button
        Debug.Log( "Setting button to active now" );
        PlayerStats.ChangeMoney(50);
        NextWaveButton.WaveNeedsToStart();
    }
}