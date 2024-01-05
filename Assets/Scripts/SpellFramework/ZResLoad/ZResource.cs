using System;
using System.Collections.Generic;
using SpellFramework.Tools;
using UnityEngine;
using UObject = UnityEngine.Object;
using ZResLoad;

/// <summary>
/// 资源缓存节点
/// </summary>
class ZResourceCacheNode
{
    /// <summary>
    /// 资源名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 资源引用
    /// </summary>
    protected UnityEngine.Object Res;

    /// <summary>
    /// 场景使用标记
    /// </summary>
    private int _SceneUseFlag;

    /// <summary>
    /// 资源引用计数
    /// </summary>
    public int Ref { get; private set; }

    public ZResourceCacheNode(string name, UnityEngine.Object res)
    {
        this.Name = name;
        this.Res = res;
    }

    public T GetRes<T>() where T : UnityEngine.Object
    {
        return Res as T;
    }

    public void SetFlag(int sceneID)
    {
        _SceneUseFlag |= (1 << sceneID);
    }

    public void ClearFlag(int sceneID)
    {
        _SceneUseFlag &= (~(1 << sceneID));
    }

    public bool HasFlag(int sceneID)
    {
        return (_SceneUseFlag & (1 << sceneID)) > 0;
    }

    public bool HasMoreFlag(int sceneID)
    {
        var flag = (_SceneUseFlag & (1 << sceneID));
        return flag > 0 && flag != _SceneUseFlag;
    }

    public void IncRef()
    {
        Ref++;
    }

    public void DecRef()
    {
        Ref--;
    }
}

public static class ZResource
{
    private static readonly IDictionary<string, ZResourceCacheNode> PersistentCache;
    
    /// <summary>
    /// 资源实例检测节点
    /// </summary>
    private static readonly IList<ZRelationInstanceNode> UsedGameObjectNodes;
    private static readonly LocalStack<ZRelationInstanceNode> UnusedGameObjectPool;

    private static int _sceneId;
    
    // 实例节点轮训检查开关
    private static bool _openFreeNodeLoop = true;
    
    private static float _instanceNodeCacheTime = 10;

    static ZResource()
    {
        PersistentCache = new Dictionary<string, ZResourceCacheNode>(1000);
        UsedGameObjectNodes = new List<ZRelationInstanceNode>(200);
        UnusedGameObjectPool = new LocalStack<ZRelationInstanceNode>(200);
        _sceneId = 0;
    }
    
    public static void SetCacheSceneId(int id)
    {
        _sceneId = id;
    }

    public static int GetCacheSceneId()
    {
        return _sceneId;
    }
    
    public static void SetInstanceCacheTime(float time)
    {
        _instanceNodeCacheTime = time;
    }

    private static float GetInstanceCacheTime()
    {
        return _instanceNodeCacheTime;
    }


    public static T Query<T>(string package, string name, bool cache = true) where T : UnityEngine.Object
    {
        package = package.ToLower();
        name = name.ToLower();
        // 先从缓存中检查是否存在 
        var node = GetCacheNode<T>(name);
        if (node != null)
        {
            // 绑定当前场景标识
            node.SetFlag(_sceneId);
            return node.GetRes<T>();
        }

        // 创建资源  加入缓存
        var obj = LoadAsset<T>(package, name);
        if (obj != null && cache)
        {
            node = CacheObject(name, obj);
            node.SetFlag(_sceneId);
        }

        return obj;
    }

    private static ZResourceCacheNode CacheObject(string name, UObject res)
    {
        ZResourceCacheNode node = null;
        var hashCode = HashNodeName(name, res.GetType());
        if (!PersistentCache.TryGetValue(hashCode, out node))
        {
            node = new ZResourceCacheNode(hashCode, res);
            PersistentCache.Add(hashCode, node);
        }

        return node;
    }

    private static ZResourceCacheNode GetCacheNode<T>(string name) where T : UnityEngine.Object
    {
        ZResourceCacheNode node = null;
        var hashCode = HashNodeName(name, typeof(T));
        if (PersistentCache.ContainsKey(hashCode))
        {
            node = PersistentCache[hashCode];
        }
        return node;
    }
    
    private static ZResourceCacheNode GetCacheNode(string name, Type type)
    {
        var hashcode = HashNodeName(name, type);
        ZResourceCacheNode node = null;
        if (PersistentCache.ContainsKey(hashcode))
        {
            node = PersistentCache[hashcode];
        }
        return node;
    }

    private static string HashNodeName(string name, Type t)
    {
        if (t == typeof(RuntimeAnimatorController))
        {
            return "AnimatorController" + "--" + name.ToLower().TrimEnd('.', 'a', 's', 's', 'e', 't');
        }

        return t.Name + "--" + name.ToLower().TrimEnd('.', 'a', 's', 's', 'e', 't');
    }

    private static void RemoveCacheObject(string hashcode)
    {
        PersistentCache.Remove(hashcode);
    }

    #region  Load && Unload
    public static void Init(string rootPath, string appContentPath)
    {
        ZResPackage.Init(rootPath, appContentPath);
    }

    private static T LoadAsset<T>(string package, string name) where T : UObject
    {
        return ZResPackage.LoadAsset<T>(package, name);
    }

    public static void UnloadAsset(UObject asset)
    {
        if (asset == null)
            return;

        var hashcode = HashNodeName(asset.name, asset.GetType());
        var hasRemoved = ZResPackage.UnloadAsset(asset);
        if (hasRemoved)
        {
            RemoveCacheObject(hashcode);
        }
    }

    public static void UnloadAllUnusedCacheAsset()
    {
        using (var i = PersistentCache.GetEnumerator())
        {
            ZResourceCacheNode node = null;
            var sceneID = _sceneId;
            var removedKeyList = new List<string>(PersistentCache.Count);
            while (i.MoveNext())
            {
                node = i.Current.Value;
                if (node.HasFlag(sceneID))
                    continue;

                var res = node.GetRes<UObject>();
                if (res == null)
                {
                    Debug.LogFormat("Node:{0} Has Null Res!!!", node.Name);
                    continue;
                }
                ZResPackage.UnloadAsset(res);
                removedKeyList.Add(i.Current.Key);
            }
            for (int j = 0; j < removedKeyList.Count; j++)
            {
                RemoveCacheObject(removedKeyList[j]);
            }
            removedKeyList.Clear();
            removedKeyList = null;
        }
    }

    public static void UnloadAllCacheAsset()
    {
        using (var i = PersistentCache.GetEnumerator())
        {
            while (i.MoveNext())
            {
                ZResPackage.UnloadAsset(i.Current.Value.GetRes<UObject>());
            }
        }
        PersistentCache.Clear();
    }

    public static void UnloadAllLoadedAsset()
    {
        ZResPackage.UnloadAllPackage();
        PersistentCache.Clear();
    }

    #endregion

    #region Instance

    public static GameObject CreateInstance(GameObject prefab)
    {
        if (prefab == null)
            return null;
        var clone = GameObject.Instantiate(prefab);
        AddInstanceNode(clone, prefab);
        return clone;
    }

    // 添加一个实例节点
    private static void AddInstanceNode(UObject mainObject, UObject relationObject)
    {
        var pkg =ZResPackage.GetPackage(relationObject);
        if (pkg == null)
            return;
        // 取空闲的实例节点
        var node = GetRelationInstanceNode();
        // 绑定节点与实例
        node.BindRelation(mainObject, relationObject, GetInstanceCacheTime());
        UsedGameObjectNodes.Add(node);
        // 引用计数自增
        IncRef(node);
    }

    // 移除实例节点
    private static void RemoveInstanceNode(ZRelationInstanceNode node)
    {
        // 移除该引用节点
        DecRef(node);
        // 重置
        node.Reset();
        // 归池
        UnusedGameObjectPool.Push(node);
    }

    // 获取一个空闲的实例节点
    private static ZRelationInstanceNode GetRelationInstanceNode()
    {
        ZRelationInstanceNode node = null;
        if (UnusedGameObjectPool.Count > 0)
        {
            node = UnusedGameObjectPool.Pop();
        }

        return node ?? new ZRelationInstanceNode();
    }
    
    public static void SetOpenCheckAssets(bool openCheck)
    {
        _openFreeNodeLoop = openCheck;
    }

    private static void UpdateInstanceNode()
    {
        if (!_openFreeNodeLoop)
        {
            return;
        }
        // 当前实例Go数量
        var num = UsedGameObjectNodes.Count;
        for (int i = 0; i < num; i++)
        {
            var node = UsedGameObjectNodes[i];
            // 检查节点有效性 和 保留时间是否到期
            if (node.IsValid() || !node.HasTimeOut()) continue;
            UsedGameObjectNodes.RemoveAt(i);
            RemoveInstanceNode(node);
            break;
        }
    }

    #endregion

    #region ARC

    private static void IncRef(ZRelationInstanceNode instanceNode)
    {
        var node = GetCacheNode(instanceNode.GetInstanceName(), instanceNode.GetInstanceType());
        node.IncRef();
    }

    private static void DecRef(ZRelationInstanceNode instanceNode)
    {
        var node = GetCacheNode(instanceNode.GetInstanceName(), instanceNode.GetInstanceType());
        if (node == null)
        {
            return;
        }
        node.DecRef();
        if (node.Ref == 0)
            UnloadAsset(node.GetRes<UObject>());
    }

    #endregion

    #region Tick

    public static void Tick(float deltaTime)
    {
        // todo 做资源引用检查
        UpdateInstanceNode();
    }

    #endregion
}
