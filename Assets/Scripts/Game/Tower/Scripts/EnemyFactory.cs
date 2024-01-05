using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EnemyFactory : GameObjectFactory
{
    [System.Serializable]
    class EnemyConfig
    {
        public Enemy prefab = default;
        
        [FloatRangeSlider(0.5f, 2f)]
        public FloatRange scale = new FloatRange(1f);

        [FloatRangeSlider(0.2f, 5f)]
        public FloatRange speed = new FloatRange(1f);

        [FloatRangeSlider(-0.4f, 0.4f)]
        public FloatRange pathOffset = new FloatRange(0f);
        
        [FloatRangeSlider(10f, 1000f)]
        public FloatRange health = new FloatRange(100f);
    }
    
    // [SerializeField]
    // private Enemy prefab = default;
    //
    // [SerializeField, FloatRangeSlider(0.5f, 2f)]
    // private FloatRange scale = new FloatRange(1f);
    // // 请注意，敌人永远不会改变其相对路径偏移，即使在转身时也是如此。因此，每个敌人的总路径长度各不相同。
    // // 另请注意，为了防止敌人刺入相邻的瓷砖，必须考虑其最大可能的规模。
    // // 我只是将最大尺寸设置为 1，因此立方体允许的最大偏移量为 0.25。如果最大尺寸为 1.5，则偏移最大值应减少到 0.125。
    // [SerializeField, FloatRangeSlider(-0.4f, 0.4f)]
    // private FloatRange pathOffset = new FloatRange(0f);
    
    // [SerializeField, FloatRangeSlider(0.2f, 5f)]
    // FloatRange speed = new FloatRange(1f);
    
    [SerializeField]
    EnemyConfig small = default, medium = default, large = default;

    public Enemy Get(EnemyType type = EnemyType.Medium)
    {
        EnemyConfig config = GetConfig(type);
        Enemy instance = CreatGameObjectInstance(config.prefab);
        instance.OriginFactory = this;
        instance.Initialize(config.scale.RandomValueInRange, 
            config.speed.RandomValueInRange,
            config.pathOffset.RandomValueInRange,
            config.health.RandomValueInRange);
        return instance;
    }
    
    EnemyConfig GetConfig (EnemyType type) {
        switch (type) {
            case EnemyType.Small: return small;
            case EnemyType.Medium: return medium;
            case EnemyType.Large: return large;
        }
        Debug.Assert(false, "Unsupported enemy type!");
        return null;
    }

    public void Reclaim(Enemy enemy)
    {
        Debug.Assert(enemy.OriginFactory == this, "Wrong factory reclaimed!");
        Destroy(enemy.gameObject);
    }
}
