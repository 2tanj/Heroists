using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable
public class StateMachine
{
    private IState? _currentState;

    private Unit _parentUnit;

    public StateMachine(Unit u) => _parentUnit = u;

    public void SetState(IState state)
    {
        if (_currentState != null)
            _currentState.Exit();

        _currentState = state;
        _currentState.Enter(_parentUnit);
    }
    public IState GetState() => _currentState;
    public Unit   GetUnit()  => _parentUnit;

    public void Execute() => _currentState?.Execute();

    // check for null when using
    public static IState GetStateFromList(List<IState> states, IState state)
                            => states.Find(s => s.GetType() == state.GetType());
}
