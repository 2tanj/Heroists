using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HealerEnemyUnit : EnemyUnit
{
    [SerializeField] private IAbility _defensiveAbilityReference;

    protected override void Awake()
    {
        base.Awake();

        DefensiveAbility = Instantiate(_defensiveAbilityReference, transform);
        DefensiveAbility.Icon.forceRenderingOff = true;
    }

    public override void MakeDecision()
    {
        if (FindHealTarget())
            SetUnitState(new HealerState(), FindHealTarget());
        else
            SetUnitState(new WanderState(), FindWanderPos());
    }

    private Unit FindHealTarget()
    {
        if (!DefensiveAbility.IsNotOnCooldown)
        {
            Debug.LogWarning("Enemy healer ability on cooldown!");
            return null;
        }

        var unitsToHeal = GameManager.Instance.EnemyUnits.Where(
                unit => unit.Health < unit.Stats.MaxHealth).ToList();

        // if there are no enemies bellow full hp return
        if (!unitsToHeal.Any())
            return null;

        // if there is only one, return it
        // to skip unnecessary checks
        if (unitsToHeal.Count == 1)
            return unitsToHeal[0];

        // our heal target is the one with the least hp
        var lowest = unitsToHeal[0];
        unitsToHeal.ForEach(unit => {
            if (unit.Health < lowest.Health)
                lowest = unit;
        });

        return lowest;
    }

    private Node FindWanderPos()
    {
        var validNodes = Position.Neighbors.Where(node =>
            node.Nodetype != Node.WALL_TYPE &&
            node.PlacedItem == null).ToList();

        return validNodes[Random.Range(0, validNodes.Count)];
    }
}
