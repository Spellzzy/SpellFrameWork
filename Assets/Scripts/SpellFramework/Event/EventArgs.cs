namespace SpellFramework.Event
{
    public class EventArgs
    {
        public readonly string key;

        public readonly object[] args;

        public EventArgs(string key)
        {
            this.key = key;
        }

        public EventArgs(string key, object[] args)
        {
            this.key = key;
            this.args = args;
        }

    }
}