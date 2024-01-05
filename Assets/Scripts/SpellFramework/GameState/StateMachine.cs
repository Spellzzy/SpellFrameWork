using System.Collections.Generic;

namespace SpellFramework.GameState
{
    public class StateMachine
    {
        private Dictionary<int, StateBase> _stateCache = null;
        private StateBase _previousState = null;
        private StateBase _currentState = null;

        public StateMachine(StateBase beginState)
        {
            _previousState = null;
            _currentState = beginState;

            _stateCache = new Dictionary<int, StateBase>();
            AddState(beginState);
            _currentState.OnEnter();
        }

        public void AddState(StateBase state)
        {
            if (!_stateCache.ContainsKey(state.ID))
            {
                _stateCache.Add(state.ID, state);
                state.Machine = this;
            }
        }

        public void SwitchState(int id)
        {
            if (!_stateCache.ContainsKey(id))
            {
                return;
            }

            if (_currentState != null && _currentState.ID != id)
            {
                _currentState.OnExit();
                _previousState = _currentState;
            }

            _currentState = _stateCache[id];
            _currentState.OnEnter();
        }

        public void Update()
        {
            if (_currentState != null)
            {
                _currentState.OnStay();
            }
        }

    }
}