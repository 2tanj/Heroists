using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable
public class HeroUnit : Unit
{
    [field: SerializeField] public int Cost { get; private set; } = 5;

    [SerializeField] private IAbility _offensiveAbilityReference;
    [SerializeField] private IAbility _defensiveAbilityReference;

    private void Awake()
    {
        Stats      = ScriptableObject.Instantiate(_statsReference);
        Stats.Team = false;

        OffensiveAbility = Instantiate(_offensiveAbilityReference, transform);
        DefensiveAbility = Instantiate(_defensiveAbilityReference, transform);

        // hiding the instantiated ability icon on the board
        OffensiveAbility.Icon.forceRenderingOff = true;
        DefensiveAbility.Icon.forceRenderingOff = true;
    }

    public override void PerformAbility(AbilityType? type, Node? n)
    {
        if (DOTween.TotalPlayingTweens() > 0)
            return;

        // stop drawing valid moves after performing an ability
        GridManager.Instance.ResetNodes();

        switch (type)
        {
            case AbilityType.NONE:
                break;

            case AbilityType.MOVEMENT:
                if (n != null)
                    Move(n);
                break;

            case AbilityType.ATTACK:
                if (n != null && n.Unit != null)
                    Attack(n.Unit);
                break;

            case AbilityType.OFFENSIVE:
                if (n != null && n.Unit != null)
                    OffensiveAbility?.UseAbility(this, n.Unit, true);
                break;

            case AbilityType.DEFENSIVE:
                if (n != null && n.Unit != null)
                    DefensiveAbility?.UseAbility(this, n.Unit, false);
                break;
        }
        //GameManager.Instance.GetValidMoves();
    }
}
