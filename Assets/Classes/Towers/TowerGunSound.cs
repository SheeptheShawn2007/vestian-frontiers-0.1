using UnityEditor.ShaderKeywordFilter;
using UnityEngine;

public class TowerGunSound : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void ShootSound()
    {
        audioSource.Play();
    }
}

