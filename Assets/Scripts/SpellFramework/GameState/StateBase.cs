namespace SpellFramework.GameState
{
        public abstract class StateBase
        {
                public int ID { get; set; }
                public StateBase(int id)
                {
                        ID = id;
                }

                public StateMachine Machine;
                public abstract void OnEnter(params object[] args);
                public abstract void OnStay(params object[] args);
                public abstract void OnExit(params object[] args);
        }
}