using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealState : IState
{
    private Node _heal;

    public override void SetItem(object heal) => _heal = (Node)heal;

    public override void Execute()
    {
        if (_parent.Position.GetDistance(_heal) <= _parent.Stats.MoveRadius)
        {
            _parent.Move(_heal);
            return;
        }
        _parent.Move(Pathfinding.GetPath(_heal, _parent.Position)[1]);
    }
}
