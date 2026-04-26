using System;

public partial class GravityManipulation
{
    // Base state interface
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

