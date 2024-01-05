using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTower : Tower
{
    [SerializeField, Range(1f, 100f)]
    float damagePerSecond = 10f;
    
    [SerializeField] private Transform turret = default, laserBeam = default;
    Vector3 _laserBeamScale;
    private TargetPoint _targetPoint;

    public override TowerType TowerType => TowerType.Laser;

    private void Awake()
    {
        _laserBeamScale = laserBeam.localScale;
    }

    public override void GameUpdate()
    {
        if (TrackTarget(ref _targetPoint) || AcquireTarget(out _targetPoint))
        {
            Shoot();
        }
        else
        {
            laserBeam.localScale = Vector3.zero;
        }
    }
    
    void Shoot () {
        Vector3 point = _targetPoint.Position;
        turret.LookAt(point);
        laserBeam.localRotation = turret.localRotation;

        float d = Vector3.Distance(turret.position, point);
        _laserBeamScale.z = d;
        laserBeam.localScale = _laserBeamScale;
        laserBeam.localPosition = turret.localPosition + 0.5f * d * laserBeam.forward;
        
        _targetPoint.Enemy.ApplyDamage(damagePerSecond * Time.deltaTime);
    }
}
