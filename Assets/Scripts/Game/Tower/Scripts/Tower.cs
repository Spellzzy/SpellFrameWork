using UnityEngine;
using Random = System.Random;

public abstract class Tower : GameTileContent
{
    // private static Collider[] _targetsBuffer = new Collider[100];
    // private const int EnemyLayerMask = 1 << 9;
    [SerializeField, Range(1.5f, 10.5f)]
    protected float targetingRange = 2.5f;

    public abstract TowerType TowerType { get; }

    protected bool AcquireTarget(out TargetPoint _targetPoint)
    {
        Vector3 a = transform.localPosition;
        Vector3 b = a;
        b.y += 2f;
        // int hits = Physics.OverlapCapsuleNonAlloc(a, b, targetingRange, _targetsBuffer, EnemyLayerMask);
        // if (hits > 0)
        // {
        //     _targetPoint = _targetsBuffer[UnityEngine.Random.Range(0, hits)].GetComponent<TargetPoint>();
        //     return true;
        // }
        if (TargetPoint.FillBuffer(transform.localPosition, targetingRange))
        {
            _targetPoint = TargetPoint.RandomBuffered;
            return true;
        }

        _targetPoint = null;
        return false;
    }
    
    protected bool TrackTarget (ref TargetPoint _targetPoint) {
        if (_targetPoint == null || !_targetPoint.Enemy.IsValidTarget ) {
            return false;
        }
        Vector3 a = transform.position;
        Vector3 b = _targetPoint.Position;
        float x = a.x - b.x;
        float z = a.z - b.z;
        float r = targetingRange + 0.125f * _targetPoint.Enemy.Scale;
        // 检查是否离开视野范围
        if (x * x + z * z > r * r)
        {
            _targetPoint = null;
            return false;
        }
        return true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 position = transform.localPosition;
        position.y += 0.01f;
        Gizmos.DrawWireSphere(position, targetingRange);
        // if (_targetPoint != null)
        // {
        //     Gizmos.DrawLine(position, _targetPoint.Position);
        // }
    }
}
