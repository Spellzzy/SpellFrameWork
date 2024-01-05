using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpellFramework.Coroutine
{
    public class CoroutineBehaviour : MonoBehaviour
    {   
    }
    
    /// <summary>
    /// 协程管理
    /// </summary>
    public class CoroutineManager
    {
        private static MonoBehaviour _BindingBehaviour;

        private static List<CoroutineNode> _CachedCoroutine;

        public delegate bool Condition();

        private static int _GID;
        public static int GID { get { return ++_GID; } }

        public static void Init(GameObject go)
        {
            var mainBehaviour = go.AddComponent<CoroutineBehaviour>();
            GameObject.DontDestroyOnLoad(go);
            Binding(mainBehaviour);
        }

        private static void Binding(MonoBehaviour main)
        {
            if (_BindingBehaviour != null)
                return;

            _BindingBehaviour = main;

            _CachedCoroutine = new List<CoroutineNode>(8);
            for (int i = 0; i < _CachedCoroutine.Capacity; i++)
            {
                _CachedCoroutine.Add(new CoroutineNode(GID));
            }
            // todo 注册事件 在某个时间节点下 清除所有的协程 比如场景卸载时
        }


        public static void StopAllCoroutine()
        {
            CoroutineNode node = null;
            for (int i = 0; i < _CachedCoroutine.Count; i++)
            {
                node = _CachedCoroutine[i];
                if (!node.IsUsing)
                    continue;

                if (!node.IsKeep || node.IsOver)
                {
                    StopCoroutine(node);
                }
            }
        }

        #region Base

        public static UnityEngine.Coroutine StartCoroutine(IEnumerator routine)
        {
            if (routine is CoroutineNode)
            {
                var node = routine as CoroutineNode;
            }
            return _BindingBehaviour.StartCoroutine(routine);
        }


        public static void StopCoroutine(IEnumerator routine)
        {
            if (routine == null || _BindingBehaviour == null)
            {
                return;
            }

            if (routine is CoroutineNode)
            {
                var node = routine as CoroutineNode;
                node.Clear();
            }
            _BindingBehaviour.StopCoroutine(routine);
        }

        public static void StopCoroutine(int id)
        {
            var node = MatchCoroutineNode(m => m.RuntimeID == id);
            if (node != null)
            {
                StopCoroutine(node);
            }
        }

        #endregion

        /// <summary>
        /// 外部接口 启动一个协程
        /// </summary>
        /// <param name="routine"></param>
        /// <returns></returns>
        public static CoroutineNode StartCoroutineAction(IEnumerator routine)
        {
            var node = GetCoroutineNode();
            node.SetRoutine(routine, true);
            StartCoroutine(node);
            return node;
        }

        /// <summary>
        /// 条件满足后执行
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static UnityEngine.Coroutine WaitAction(Condition condition, Action action)
        {
            var node = GetCoroutineNode();
            node.SetRoutine(DoWaitConditionAction(condition, action), false);
            return StartCoroutine(node);
        }

        /// <summary>
        /// 一帧结束后执行
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static UnityEngine.Coroutine StartDelayFrameAction(Action action)
        {
            return StartCoroutine(DoWaitFrameAction(action));
        }

        /// <summary>
        /// 延迟N帧后执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        public static UnityEngine.Coroutine StartDelayFrameAction(Action action, int frame)
        {
            var node = GetCoroutineNode();
            node.SetRoutine(DoWaitFrameAction(action, frame), true);
            return StartCoroutine(node);
        }

        /// <summary>
        /// 掩饰N秒后执行 默认不重复
        /// </summary>
        /// <param name="action"></param>
        /// <param name="second"></param>
        /// <param name="isKeep"></param>
        /// <returns></returns>
        public static UnityEngine.Coroutine StartDelaySecondAction(Action action, float second, bool isKeep = false)
        {
            var node = GetCoroutineNode();
            node.SetRoutine(DoWaitSecondAction(action, second), isKeep);
            return StartCoroutine(node);
        }

        /// <summary>
        /// 延时N秒重复执行 action
        /// </summary>
        /// <param name="action"></param>
        /// <param name="second"></param>
        /// <param name="duration"></param>
        /// <param name="repeatedTimes"></param>
        /// <param name="isKeep">是否持续（是否切场景继续运行）</param>
        /// <returns></returns>
        public static int StartDelaySecondRepeatedAction(Action action, float second, float duration, int repeatedTimes, bool isKeep)
        {
            var node = GetCoroutineNode();
            node.SetRoutine(DoWaitSecondRepeatedAction(action, second, duration, repeatedTimes), isKeep);
            StartCoroutine(node);
            return node.RuntimeID;
        }

        /// <summary>
        /// 延时多少秒后执行BeginAction 延时多少秒后执行EndAction
        /// </summary>
        /// <param name="beginAction"></param>
        /// <param name="beginSecond"></param>
        /// <param name="endAction"></param>
        /// <param name="endSecond"></param>
        /// <param name="isKeep"></param>
        /// <returns></returns>
        public static UnityEngine.Coroutine StartDelaySecondBeginEndAction(Action beginAction, float beginSecond, Action endAction, float endSecond, bool isKeep = false)
        {
            var node = GetCoroutineNode();
            node.SetRoutine(DoWaitSecondBeginEndAction(beginAction, beginSecond, endAction, endSecond), isKeep);
            return StartCoroutine(node);
        }

        /// <summary>
        /// 执行一个一段时间内的Update action
        /// </summary>
        /// <param name="action"></param>
        /// <param name="second">时间,如果是负数，则表示一直call</param>
        /// <param name="delay"></param>
        /// <param name="isKeep">是否持续(默认不持续，如果持续为切换场景不销毁)</param>
        /// <returns></returns>
        public static int PostUpdateAction(Action<float> action, float second, float delay = 0f, bool isKeep = false)
        {
            var node = GetCoroutineNode();
            node.SetRoutine(DoUpdateAction(action, second, delay), isKeep);
            StartCoroutine(node);
            return node.RuntimeID;
        }

        private static CoroutineNode GetCoroutineNode()
        {
            var node = MatchCoroutineNode(n => n.IsOver || !n.IsUsing);
            if (node == null)
            {
                node = new CoroutineNode(GID);
                AddCoroutineNode(node);
            }

            return node;
        }

        private static void AddCoroutineNode(CoroutineNode node)
        {
            _CachedCoroutine.Add(node);
        }

        private static CoroutineNode MatchCoroutineNode(Predicate<CoroutineNode> match)
        {
            if (_CachedCoroutine == null)
            {
                return null;
            }

            CoroutineNode node = null;
            int coroutineCount = _CachedCoroutine.Count;
            for (int i = coroutineCount - 1; i >= 0; i--)
            {
                node = _CachedCoroutine[i];
                if (match(node))
                {
                    return node;
                }
            }

            return null;
        }

        #region IEnumerator Func
        private static IEnumerator DoWaitConditionAction(Condition conditon, Action action)
        {
            while (!conditon.Invoke())
            {
                yield return 0;
            }
            action();
        }

        private static IEnumerator DoWaitFrameAction(Action action, int frame = 1)
        {
            for (int i = 0; i < frame; i++)
            {
                yield return YieldCache.GetWaitForEndOfFrame();
            }

            action();
        }

        private static IEnumerator DoWaitSecondAction(Action action, float second)
        {
            float passTime = 0f;

            while (passTime < second)
            {
                passTime += Time.deltaTime;
                yield return YieldCache.GetWaitForEndOfFrame();
            }
            action();
        }

        private static IEnumerator DoWaitSecondRepeatedAction(Action action, float second, float duration, int times)
        {
            var passTime = 0f;
            while (passTime < second)
            {
                passTime += Time.deltaTime;
                yield return YieldCache.GetWaitForEndOfFrame();
            }

            for (int i = 0; i < times || times < 0; i++)
            {
                passTime = 0f;

                while (passTime < duration)
                {
                    passTime += Time.deltaTime;
                    yield return YieldCache.GetWaitForEndOfFrame();
                }

                action();
            }
        }

        private static IEnumerator DoWaitSecondBeginEndAction(
            Action beginAction, float beginSec, Action endAction, float endSec)
        {
            var passTime = 0f;
            while (passTime < beginSec)
            {
                passTime += Time.deltaTime;
                yield return YieldCache.GetWaitForEndOfFrame();
            }
            beginAction();

            while (passTime < endSec)
            {
                passTime += Time.deltaTime;
                yield return YieldCache.GetWaitForEndOfFrame();
            }

            endAction();
        }

        private static IEnumerator DoUpdateAction(Action<float> action, float second, float delay)
        {
            var passTime = 0f;
            while (passTime < delay)
            {
                passTime += Time.deltaTime;
                yield return YieldCache.GetWaitForEndOfFrame();
            }

            passTime = 0f;
            while (passTime < second || second < 0)
            {
                passTime += Time.deltaTime;
                action(passTime);
                yield return YieldCache.GetWaitForEndOfFrame();
            }
        }

        #endregion

    }
}