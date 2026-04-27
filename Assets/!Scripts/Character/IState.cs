using System;

// State interface used by GravityManipulation's simple state machine.
// Each concrete state must implement lifecycle methods for enter/update/exit
// and provide a Type property so the machine can determine transitions.
public partial class GravityManipulation
{
    private interface IState
    {
        StateType Type { get; }
        void OnEnter(Action onCompletion = null);
        void OnUpdate();
        // Return the desired next state. Return the same state to remain.
        StateType OnTransitionCheck();
        void OnExit(StateType next, Action onCompletion = null);
    }
}

