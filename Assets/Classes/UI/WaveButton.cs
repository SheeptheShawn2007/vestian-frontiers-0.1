using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class WaveButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI WaveButtonText;

    public WaveManager WaveManager;
    public GameObject NextWaveButton;
    public int Mode;

    void Start()
    {
        NextWaveButton.SetActive( true );
    }
    public void WaveNeedsToStart()
    {
        Debug.Log( "Setting active nextwave button" );
        NextWaveButton.SetActive( true );
        Debug.Log("Button active: " + NextWaveButton.activeSelf);
    }
    public void WaveButtonClicked()
    {
        if( WaveManager.WaveIndex == -1 )
        {
            WaveManager.StartMode( Mode );
        }
        else
        {
            WaveManager.StartWave();
        }
        NextWaveButton.SetActive( false );
    }
}
