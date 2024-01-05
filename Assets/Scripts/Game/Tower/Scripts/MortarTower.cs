using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MortarTower : Tower
{
    [SerializeField, Range(0.5f, 2f)]
    float shotsPerSecond = 1f;
    
    [SerializeField, Range(0.5f, 3f)]
    float shellBlastRadius = 1f;

    [SerializeField, Range(1f, 100f)]
    float shellDamage = 10f;

    [SerializeField]
    Transform mortar = default;
    
    public override TowerType TowerType => TowerType.Mortar;

    private float launchSpeed;
    private float launchProgress;
    private void Awake()
    {
        OnValidate();
    }

    private void OnValidate()
    {
        float x = targetingRange + 0.25001f;
        float y = -mortar.position.y;
        launchSpeed = Mathf.Sqrt(9.81f * (y + Mathf.Sqrt(x * x + y * y)));
    }

    public override void GameUpdate()
    {
        launchProgress += shotsPerSecond * Time.deltaTime;
        while (launchProgress >= 1f)
        {
            if (AcquireTarget(out TargetPoint target))
            {
                Launch(target);
                launchProgress -= 1f;
            }
            else
            {
                launchProgress = 0.999f;
            }
        }
        // Launch(new Vector3(3f, 0f, 0f));
        // Launch(new Vector3(0f, 0f, 1f));
        // Launch(new Vector3(1f, 0f, 1f));
        // Launch(new Vector3(3f, 0f, 1f));
    }

    public void Launch(TargetPoint target)
    {
        Launch(target.Position);
    }

    public void Launch(Vector3 targetPosition)
    {
        Vector3 launchPoint = mortar.position;
        Vector3 targetPoint = targetPosition;
        targetPoint.y = 0f;

        Vector2 dir;
        dir.x = targetPoint.x - launchPoint.x;
        dir.y = targetPoint.z - launchPoint.z;
        
        float x = dir.magnitude;
        float y = -launchPoint.y;
        if (x == 0f)
        {
            return;
        }
        dir /= x;

        float g = 9.81f;
        float s = launchSpeed;
        float s2 = s * s;
        float r = s2 * s2 - g * (g * x * x + 2f * y * s2);
        // Debug.Assert(r >= 0f , "r 计算需要大于0");
        float tanTheta = (s2 + Mathf.Sqrt(r)) / (g * x);
        float cosTheta = Mathf.Cos(Mathf.Atan(tanTheta));
        float sinTheta = cosTheta * tanTheta;

        mortar.localRotation = Quaternion.LookRotation(new Vector3(dir.x, tanTheta, dir.y));
        
        Game.SpawnShell().Initialize(launchPoint, targetPoint,
            new Vector3(s * cosTheta *dir.x ,
                s * sinTheta, s * cosTheta * dir.y), shellBlastRadius, shellDamage);
        
        // Vector3 prev = launchPoint;
        // Vector3 next = Vector3.zero;
        //
        // for (int i = 1; i <= 10; i++)
        // {
        //     float t = i / 10f;
        //     float dx = s * cosTheta * t;
        //     float dy = s * sinTheta * t - 0.5f * g * t * t;
        //     next = new Vector3( launchPoint.x + dir.x * dx, launchPoint.y + dy, launchPoint.z + dir.y * dx);
        //     Debug.DrawLine(prev, next, Color.blue, 1f);
        //     prev = next;
        // }
        //
        // Debug.DrawLine(launchPoint, targetPoint, Color.green, 1f);
        // Debug.DrawLine(new Vector3(launchPoint.x, 0.01f, launchPoint.z), 
        //     new Vector3(launchPoint.x + dir.x *x , 0.01f, launchPoint.z + dir.y * x), Color.white,  1f);
        //
    }
}
