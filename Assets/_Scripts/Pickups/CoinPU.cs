using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinPU : IPickup
{
    private int _coinAmount => Random.Range(1, 10);

    public override void UsePickup(Unit unit)
    {
        if (unit is HeroUnit)
        {
            Debug.Log($"{unit.Stats.name} just picked up {_coinAmount} coins");
            GameManager.Instance.CoinCointer += _coinAmount;
        }

        Destroy(gameObject);
    }
}
