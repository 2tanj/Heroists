using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefPU : IPickup
{
    [SerializeField, Range(0, 1)]
    private float _boostAmount = .1f;

    public override void UsePickup(Unit unit)
    {
        Debug.Log("DEF boosted: " + unit.Stats.Defense);
        unit.Stats.Defense += _boostAmount;
        Debug.Log("New DEF: " + unit.Stats.Defense);

        Destroy(gameObject);
    }
}
