using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealAbility : IAbility
{
    [SerializeField, Space(20)]
    private float _healAmount = 3;

    public override void UseAbility(Unit player, Unit other, bool offOrDef)
    {
        if (CantBeUsed(player, other, offOrDef))
        {
            if (player is EnemyUnit)
            {
                Debug.Log("");
                FinishAbility(player);
            }

            return;
        }

        Debug.Log("Old health: " + other.Health);
        other.Health += _healAmount;
        Debug.Log("New health: " + other.Health);

        FinishAbility(player);
    }
}
