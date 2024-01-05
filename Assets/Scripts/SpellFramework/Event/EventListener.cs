using System.Collections.Generic;

namespace SpellFramework.Event
{
    public class EventListener
    {
        public delegate void EventHandler(EventArgs args);

        private Dictionary<int, EventHandler> _handlerDic;

        public EventListener()
        {
            _handlerDic = new Dictionary<int, EventHandler>();
        }

        // 触发
        public void Invoke(EventArgs args)
        {
            foreach (var handler in _handlerDic)
            {
                handler.Value.Invoke(args);
            }
        }
        
        // 为当前事件绑定handler
        public void BindHandler(EventHandler handler)
        {
            int hashCode = handler.GetHashCode();
            _handlerDic[hashCode] = handler;
        }

        public void RemoveHandler(EventHandler handler)
        {
            int hashCode = handler.GetHashCode();
            if (_handlerDic.ContainsKey(hashCode))
            {
                _handlerDic.Remove(hashCode);
            }
        }

        public void Clear()
        {
            _handlerDic.Clear();
        }

    }
}