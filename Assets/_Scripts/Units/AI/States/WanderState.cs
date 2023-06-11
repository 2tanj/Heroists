using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WanderState : IState
{
    private Node _node;

    public override void SetItem(object node) => _node = (Node)node;
    public override void Execute() => _parent.Move(_node);
}
