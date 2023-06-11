using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : IState
{
    private Unit _target;

    public override void SetItem(object target) => _target = (Unit)target;

    public override void Execute()
    {
        if (_target.Position.GetDistance(_parent.Position) > _parent.Stats.AttackRadius) 
            _parent.Move(GetSecondLast(Pathfinding.GetPath(_parent.Position, _target.Position)));
        else
            _parent.Attack(_target);
    }

    private Node GetSecondLast(List<Node> nodes) => nodes[nodes.Count - 1];
}


