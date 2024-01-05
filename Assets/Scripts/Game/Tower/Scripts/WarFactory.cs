using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu]
public class WarFactory : GameObjectFactory
{
    [SerializeField]
    private Shell shellPrefab;
    [SerializeField]
    Explosion explosionPrefab = default;

    Dictionary<string, Queue<WarEntity>> pool;

    public Shell Shell => Get(shellPrefab);
    
    public Explosion Explosion => Get(explosionPrefab);
    T Get<T>(T prefab) where T : WarEntity
    {
        if (pool == null)
        {
            pool = new Dictionary<string, Queue<WarEntity>>();
        }

        T instance = null;
        string ObjName = prefab.name + "(Clone)";
        if (pool.ContainsKey(ObjName))
        {
            if (pool[ObjName].Count > 0)
            {
                instance = (T)pool[ObjName].Dequeue();
                instance.gameObject.SetActive(true);
                return instance;
            }
        }
        
        instance = CreatGameObjectInstance(prefab);
        instance.OriginFactory = this;
        
        return instance;
    }

    public void Reclaim<T>(T entity) where T : WarEntity
    {
        string ObjName = entity.gameObject.name;
        entity.transform.localPosition = Vector3.zero;
        entity.gameObject.SetActive(false);
        if (!pool.ContainsKey(ObjName))
        {
            pool[ObjName] = new Queue<WarEntity>();
        }
        pool[ObjName].Enqueue(entity);
        
        // Destroy(entity.gameObject);
    }
}
