using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyAI
{
    StateMachine FSM { get; }

    public abstract void MakeDecision();
}
