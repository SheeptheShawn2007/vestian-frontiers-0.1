using System;
using UnityEngine;

public class TowerLaserRender : MonoBehaviour
{
    [SerializeField] private LineRenderer Laser;
    public Material LaserMaterial;

    public object SetPosition { get; internal set; }


    void Awake()
    {
        Laser = gameObject.GetComponent<LineRenderer>();
        Laser.positionCount = 2;
        Laser.startWidth = 0.1f;
        Laser.endWidth = 0.1f;
        Laser.material = LaserMaterial;
        Laser.enabled = false;
    }
}
