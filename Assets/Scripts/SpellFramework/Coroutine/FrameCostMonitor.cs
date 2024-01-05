using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpellFramework.Coroutine
{
    public class GlobalCostMonitor
    {
        private static readonly FrameCostMonitor Monitor = new FrameCostMonitor();
        public static bool Check()
        {
            return Monitor.Check();
        }
    }

    public class FrameCostMonitor
    {
        // 可通过的每帧最小耗时
        private const float MaxCostTimePerFrame = 16f;
        // 限制1s最高帧数
        private const int MaxFrameLimit = 60;
        // 60帧内 渲染帧时间节点记录
        private readonly Dictionary<int, long> _cacheFrameCost = new Dictionary<int, long>(MaxFrameLimit);

        private long GetMilliSecond()
        {
            var time = DateTime.Now.Ticks / 10000;
            return time;
        }

        // 更新当前时间节点距离上一帧耗时
        private long UpdateFrameConst(long time)
        {
            var frame = Time.frameCount;
            if (_cacheFrameCost.TryGetValue(frame, out var cost)) return time - cost;
            if (_cacheFrameCost.Count + 1 >= MaxFrameLimit)
            {
                _cacheFrameCost.Clear();
            }
            // 记录当前渲染帧时间节点
            _cacheFrameCost[frame] = time;
            return 0;
        }

        public bool Check()
        {
            var now = GetMilliSecond();
            var ret = UpdateFrameConst(now);
            return ret > MaxCostTimePerFrame;                 
        }
    }
}