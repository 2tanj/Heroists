using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtkPU : IPickup
{
    [SerializeField, Range(1, 3)]
    private float _boostAmount = 1;

    public override void UsePickup(Unit unit)
    {
        Debug.Log("ATK boosted: " + unit.Stats.AttackDamage);
        unit.Stats.AttackDamage += _boostAmount;
        Debug.Log("New ATK: " + unit.Stats.AttackDamage);

        Destroy(gameObject);
    }
}
