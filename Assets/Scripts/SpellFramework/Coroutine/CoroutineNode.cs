using System;
using System.Collections;
using UnityEngine;

namespace SpellFramework.Coroutine
{
    public class CoroutineNode : IEnumerator
    {
        // 节点ID
        public int ID { get; private set; }
        // 运行ID
        public int RuntimeID { get; private set; }
        // 当前是否在使用中
        public bool IsUsing { get; private set; }
        // 是否持续使用
        public bool IsKeep { get; private set; }
        // 是否执行结束
        public bool IsOver { get; private set; }
        // 当前占用协程
        private IEnumerator _Routine { get; set; }
        
        public CoroutineNode(int id)
        {
            ID = id;
        }

        public void Clear()
        {
            IsUsing = false;
        }

        public void SetRoutine(IEnumerator routine, bool isKeep)
        {
            IsUsing = true;
            _Routine = routine;
            IsKeep = isKeep;
            IsOver = false;

            RuntimeID = routine.GetHashCode();
        }

        public bool MoveNext()
        {
            if (_Routine == null)
                return false;

            bool next = false;
            try
            {
                GlobalCostMonitor.Check();
                bool _next = _Routine.MoveNext();
                while (_next)
                {
                    if (GlobalCostMonitor.Check())
                    {
                        next = true;
                        break;
                    }
                }
                
            }
            catch (Exception e)
            {
                next = false;
            }
            if (!next)
            {
                IsOver = true;
            }

            return next;
        }

        public void Reset()
        {
            if (_Routine != null)
            {
                _Routine.Reset();
                IsOver = false;
                IsUsing = false;
                _Routine = null;
            }
        }

        public object Current
        {
            get { return _Routine.Current; }
        }

        public override string ToString()
        {
            return "ID:" + ID + " RuntimeID:" + RuntimeID;
        }
    }
}