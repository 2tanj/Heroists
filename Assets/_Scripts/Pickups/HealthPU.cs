using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPU : IPickup
{
    [SerializeField, Range(1, 5)]
    private float _healAmount = 1;

    public override void UsePickup(Unit unit)
    {
        Debug.Log("HP picked up: " + unit.Health);
        unit.Health += _healAmount;
        Debug.Log("New health: " + unit.Health);

        Destroy(gameObject);
    }
}
