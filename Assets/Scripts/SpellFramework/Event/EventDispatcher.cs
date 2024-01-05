using System.Collections.Generic;

namespace SpellFramework.Event
{
    public class EventDispatcher
    {
        private Dictionary<string, EventListener> _eventPool;

        public EventDispatcher()
        {
            _eventPool = new Dictionary<string, EventListener>();
        }

        public void AddListener(string eventType, EventListener.EventHandler handler)
        {
            var listener = !_eventPool.ContainsKey(eventType) ? new EventListener() : _eventPool[eventType];
            listener.BindHandler(handler);
            _eventPool[eventType] = listener;
        }

        public void RemoveListener(string eventType, EventListener.EventHandler handler)
        {
            if (_eventPool.ContainsKey(eventType))
            {
                _eventPool[eventType].RemoveHandler(handler);
            }
        }
        
        public void ClearListener(string eventType)
        {
            if (!_eventPool.ContainsKey(eventType)) return;
            _eventPool[eventType].Clear();
            _eventPool.Remove(eventType);
        }
        
        public void DispatchEvent(string eventType, params object[] args)
        {
            if (!_eventPool.ContainsKey(eventType)) return;
            var eventArgs = args == null ? new EventArgs(eventType) : new EventArgs(eventType, args);
            var tempEvent = _eventPool[eventType];
            tempEvent.Invoke(eventArgs);
        }
        
        public bool Contains(string eventType)
        {
            return _eventPool.ContainsKey(eventType);
        }
        
        public void Clear()
        {
            foreach (var listener in _eventPool)
                listener.Value.Clear();

            _eventPool.Clear();
            _eventPool = null;
        }
    }
}