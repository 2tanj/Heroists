using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FireMeteorAbility : IAbility
{
    [SerializeField, Space(20)]
    private float _damage = 5f;

    public override void UseAbility(Unit player, Unit target, bool offOrDef)
    {
        if (CantBeUsed(player, target, offOrDef))
            return;

        var spawnPos = new Vector3(target.transform.position.x + 2, target.transform.position.y + 3);
        var spawnedAbility = Instantiate(Icon, spawnPos, Icon.transform.rotation, target.transform);
        spawnedAbility.transform.DOScale(new Vector3(25, 25), .4f).SetEase(Ease.Linear);
        spawnedAbility.transform.DOMove(target.transform.position, .4f).SetEase(Ease.Linear)
            .OnComplete(() => {
                Destroy(spawnedAbility.gameObject);
                player.DoDamage(target, _damage);
                FinishAbility(player);
            });
    }
}
