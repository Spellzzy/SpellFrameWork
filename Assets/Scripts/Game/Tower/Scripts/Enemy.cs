using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : GameBehavior
{
    [SerializeField] private EnemyAnimationConfig _animationConfig = default;
    [SerializeField] private Transform _model = default;
    
    private EnemyFactory _originFactory;
    private EnemyAnimator _animator;

    private GameTile _tileFrom, _tileTo;
    private Vector3 _positionFrom, _positionTo;
    private float _progress, _progressFactor;

    private Direction _direction;
    private DirectionChange _directionChange;

    private float _directionAngleFrom, _directionAngleTo;
    private float _pathOffset;
    private float _speed;
    
    float Health { get; set; }
    
    public float Scale { get; private set; }

    public bool IsValidTarget => _animator.CurrentClip == Clip.Move;
    
    private Collider targetPointCollider;

    public Collider TargetPointCollider {
        set {
            Debug.Assert(targetPointCollider == null, "Redefined collider!");
            targetPointCollider = value;
        }
    }
    
    public EnemyFactory OriginFactory
    {
        get => _originFactory;
        set
        {
            Debug.Assert(_originFactory == null, "Redefined origin factory!");
            _originFactory = value;
        }
    }

    private void Awake()
    {
        _animator.Config(_model.GetChild(0).gameObject.GetOrAddComponent<Animator>(), _animationConfig);
    }

    public void Initialize(float scale, float speed, float pathOffset, float health)
    {
        _model.localScale = new Vector3(scale, scale, scale);
        this._speed = speed;
        this._pathOffset = pathOffset;
        Scale = scale;
        Health = health;
        _animator.PlayIntro();
        // _animator.Play(speed / scale);
        targetPointCollider.enabled = false;
    }

    public void SpawnOn(GameTile tile)
    {
        _tileFrom = tile;
        _tileTo = tile.NextTileOnPath;
        _progress = 0f;
        PrepareIntro();
    }

    void PrepareIntro()
    {
        _positionFrom = _tileFrom.transform.localPosition;
        transform.localPosition = _positionFrom;
        _positionTo = _tileFrom.ExitPoint;
        _direction = _tileFrom.PathDirection;
        _directionChange = DirectionChange.None;
        _directionAngleFrom = _directionAngleTo = _direction.GetAngle();
        transform.localRotation = _direction.GetRotation();
        _model.localPosition = new Vector3(_pathOffset, 0f);
        // 起步加速
        _progressFactor = 2f * _speed;
    }

    void PrepareOutro()
    {
        _positionTo = _tileFrom.transform.localPosition;
        _directionChange = DirectionChange.None;
        _directionAngleTo = _direction.GetAngle();
        transform.localRotation = _direction.GetRotation();
        _model.localPosition = new Vector3(_pathOffset, 0f);
        _progressFactor = 2f * _speed;
    }

    void PrepareNextState()
    {
        _tileFrom = _tileTo;
        _tileTo = _tileTo.NextTileOnPath;
        
        _positionFrom = _positionTo;
        if (_tileTo == null)
        {
            // 准备到达终点
            PrepareOutro();
            return;
        }
        _positionTo = _tileFrom.ExitPoint;
        _directionChange = _direction.GetDirectionChangeTo(_tileFrom.PathDirection);
        _direction = _tileFrom.PathDirection;
        _directionAngleFrom = _directionAngleTo;
        
        switch (_directionChange) {
            case DirectionChange.None: PrepareForward(); break;
            case DirectionChange.TurnRight: PrepareTurnRight(); break;
            case DirectionChange.TurnLeft: PrepareTurnLeft(); break;
            default: PrepareTurnAround(); break;
        }
    }

    void PrepareForward()
    {
        transform.localRotation = _direction.GetRotation();
        _directionAngleTo = _direction.GetAngle();
        _model.localPosition = new Vector3(_pathOffset, 0f);
        // 前进时正常速率
        _progressFactor = _speed;
    }
    
    void PrepareTurnRight () {
        _directionAngleTo = _directionAngleFrom + 90f;
        _model.localPosition = new Vector3(_pathOffset - 0.5f, 0f);
        transform.localPosition = _positionFrom + _direction.GetHalfVector();
        // 转弯时减速
        // 周长或圆等于 2π 乘以半径。右转或左转仅覆盖其中的四分之一，半径为 1/2，因此为 1/2π × 1/2。
        // 由于路径偏移会在转弯时改变半径，因此我们必须调整计算进度因子的方式。必须从 ½ 中减去路径偏移以获得右转的半径，并添加到左转的半径。
        _progressFactor = _speed / (Mathf.PI * 0.5f * (0.5f - _pathOffset));
    }

    void PrepareTurnLeft () {
        _directionAngleTo = _directionAngleFrom - 90f;
        _model.localPosition = new Vector3(_pathOffset + 0.5f, 0f);
        transform.localPosition = _positionFrom + _direction.GetHalfVector();
        // 转弯时减速
        _progressFactor = _speed / (Mathf.PI * 0.5f * (0.5f + _pathOffset));
    }

    void PrepareTurnAround () {
        _directionAngleTo = _directionAngleFrom + (_pathOffset < 0f ? 180f : -180f);
        _model.localPosition = new Vector3(_pathOffset, 0f);
        transform.localPosition = _positionFrom;
        // 转身加速
        // 强制执行速度计算的最小半径以防止瞬时转弯，例如 0.2。
        _progressFactor = _speed / (Mathf.PI * Mathf.Max(Mathf.Abs(_pathOffset), 0.2f));
    }

    public override bool GameUpdate()
    {
        _animator.GameUpdate();
        if (_animator.CurrentClip == Clip.Intro)
        {
            if (!_animator.IsDone)
            {
                return true;
            }
            _animator.PlayMove(_speed / Scale);
            targetPointCollider.enabled = true;
        }else if (_animator.CurrentClip >= Clip.Outro)
        {
            if (_animator.IsDone)
            {
                Recycle();
                return false;
            }
            return true;
        }
        
        if (Health <= 0f)
        {
            // Recycle();
            // return false;
            _animator.PlayDying();
            targetPointCollider.enabled = false;
            return true;
        }
        _progress += Time.deltaTime * _progressFactor;
        while (_progress >= 1f)
        {
            // _tileFrom = _tileTo;
            // _tileTo = _tileTo.NextTileOnPath;
            if (_tileTo == null)
            {
                Game.EnemyReachedDestination();
                // Recycle();
                // return false;
                _animator.PlayOutro();
                targetPointCollider.enabled = false;
                return true;
            }
            // _progress -= 1f;
            // normalize progress
            _progress = (_progress - 1f) / _progressFactor;
            PrepareNextState();
            _progress *= _progressFactor;
        }
        // transform.localPosition = Vector3.LerpUnclamped(_positionFrom, _positionTo, _progress);
        if (_directionChange == DirectionChange.None)
        {
            // 无角度变化
            transform.localPosition = Vector3.LerpUnclamped(_positionFrom, _positionTo, _progress);
        }
        else
        {
            // 角度插值
            float angle = Mathf.LerpUnclamped(_directionAngleFrom, _directionAngleTo, _progress);
            transform.localRotation = Quaternion.Euler(0f, angle, 0f);
        }
        return true;
    }

    public override void Recycle()
    {
        _animator.Stop();
        OriginFactory.Reclaim(this);
    }

    public void ApplyDamage (float damage) {
        Debug.Assert(damage >= 0f, "Negative damage applied.");
        Health -= damage;
    }
    
    void OnDestroy () {
        _animator.Destroy();
    }
}
