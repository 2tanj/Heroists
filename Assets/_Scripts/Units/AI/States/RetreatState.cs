using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RetreatState : IState
{
    private Unit _runFromUnit;

    public override void SetItem(object unit) => _runFromUnit = (Unit)unit;

    // BUG:
    // if hero is diagonal down right, and parent is next to a left wall but
    // there is space above him he wont move!!
    public override void Execute()
    {
        // node name = Node 1 1
        var mySplit    = _parent.Position.name.Split(' ');
        var heroSplit  = _runFromUnit.Position.name.Split(' ');
        
        int myX   = int.Parse(mySplit[1]),   myY   = int.Parse(mySplit[2]);
        int heroX = int.Parse(heroSplit[1]), heroY = int.Parse(heroSplit[2]);

        var newPos = new Vector2(myX + Mathf.Clamp(myX - heroX, -1, 1), 
                                 myY + Mathf.Clamp(myY - heroY, -1, 1));

        // TODO: check if newNode is occupied!
        var newNode = GridManager.Instance.GetNodeAtExactPosition(newPos);
        if (newNode != null)
            _parent.Move(newNode);
        else
        {
            Debug.Log("No nodes for " + _parent.Stats.Name + " to move to");
            // if unit has nowhere to move to but has a target in range, attack
            if (_runFromUnit.Position.GetDistance(_parent.Position) <= _parent.Stats.AttackRadius)
                _parent.Attack(_runFromUnit);

            _parent.OnAbilityPerformed?.Invoke();
        }
    }
}
