namespace SpellFramework.Event
{
    public class EventSystem
    {
        private static EventDispatcher _dispatcher = new EventDispatcher();

        public static void AddListener(string eventType, EventListener.EventHandler eventHandler)
        {
            _dispatcher.AddListener(eventType, eventHandler);
        }

        public static void RemoveListener(string eventType, EventListener.EventHandler eventHandler)
        {
            _dispatcher.RemoveListener(eventType, eventHandler);
        }
        
        public static void ClearListener(string eventType)
        {
            _dispatcher.ClearListener(eventType);
        }
        
        // 检查是否存在
        public static bool Contains (string eventType)
        {
            return _dispatcher.Contains(eventType);
        }

        // 触发
        public static void Dispatch(string eventType, params object[] args)
        {
            _dispatcher.DispatchEvent(eventType, args);
        }
        
        // 清空所有注册事件
        public static void Clear()
        {
            _dispatcher.Clear();
        }
    }
}