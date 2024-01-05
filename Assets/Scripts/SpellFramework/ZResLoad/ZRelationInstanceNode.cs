using System;
using SpellFramework.Tools;
using UnityEngine;
using UObject = UnityEngine.Object;
namespace ZResLoad
{
    class NodeTimeOutCheckMonitor
    {
        // 最后使用时间
        private float _lastUsedTime;
        // 超时时间
        private float _maxTimeOutTime = 0;
        // 下次超时时间 Random(TimeOutTime/2, TimeOutTime)
        private float _nextCheckTime = 0;
        
        public NodeTimeOutCheckMonitor(float timeOut)
        {
            _maxTimeOutTime = timeOut;
        }

        public void SetMaxTimeOutTime(float timeOut)
        {
            _maxTimeOutTime = timeOut;
        }

        public void SetLastUseTime()
        {
            _lastUsedTime = GetNow();
            _nextCheckTime = _lastUsedTime + ZMath.Random(_maxTimeOutTime * 0.5f, _maxTimeOutTime);
        }

        public bool HasTimeOut()
        {
            return GetNow() > _nextCheckTime;
        }

        private float GetNow()
        {
            return Time.realtimeSinceStartup;
        }

    }
    public class ZRelationInstanceNode
    {
        private UObject _mainObject;
        private int _mainObjID;
        private string _relationObjName;
        private int _relationSceneID;
        private Type _type;
        // 节点有效标识
        private bool _isValid = true;
        // 节点有效性检查器
        private NodeTimeOutCheckMonitor _monitor;
        public ZRelationInstanceNode()
        {
            float timeout = 2;
            _monitor = new NodeTimeOutCheckMonitor(timeout);
        }

        public void BindRelation(UObject mainObj, UObject relationObj, float timeout)
        {
            _mainObject = mainObj;
            _mainObjID = mainObj.GetInstanceID();
            _relationObjName = relationObj.name;
            _relationSceneID = ZResource.GetCacheSceneId();
            _type = relationObj.GetType();

            _isValid = true;
            _monitor.SetMaxTimeOutTime(timeout);
        }
        
        public string GetInstanceName()
        {
            return _relationObjName;
        }

        public int GetInstanceID()
        {
            return _mainObjID;
        }

        public Type GetInstanceType()
        {
            return _type;
        }
        
        public int GetRelationSceneId()
        {
            return _relationSceneID;
        }

        public bool IsValid()
        {
            if (!_isValid)
            {
                return false;
            }
            
            if (_mainObject != null)
            {
                return true;
            }
            _monitor.SetLastUseTime();
            _isValid = false;
            return _isValid;
        }

        public bool HasTimeOut()
        {
            return _monitor.HasTimeOut();
        }

        public void Reset()
        {
            _mainObject = null;
            _mainObjID = 0;
            _relationObjName = null;
            _type = null;
            _relationSceneID = 0;
        }
    }
}