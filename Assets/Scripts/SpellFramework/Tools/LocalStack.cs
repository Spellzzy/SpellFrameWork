using System.Collections.Generic;

namespace SpellFramework.Tools
{
    public class LocalStack<T>
    {
        private readonly Stack<T> _stack;

        public LocalStack(int num = 4)
        {
            _stack = new Stack<T>(num);
        }

        public int Count
        {
            get { return _stack.Count; }
        }

        public void Push(T value)
        {
            _stack.Push(value);
        }

        public T Pop()
        {
            if (Count == 0)
            {
                return default(T);
            }

            return _stack.Pop();
        }
    }
}