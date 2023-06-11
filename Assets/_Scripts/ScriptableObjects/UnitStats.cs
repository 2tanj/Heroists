using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Units/UnitStats")]
public class UnitStats : ScriptableObject
{
    public string Name = "Unit";

    private const int _maxMaxHealth = 30;
    private const int _maxAtkDmg    = 10;

    [Range(1, _maxMaxHealth)]
    public float MaxHealth    = 10;
    [Range(1, _maxAtkDmg)]
    public float AttackDamage = 3;
    [Range(0, 1)]
    public float Defense      = 0;

    [Range(10, 50)]
    public int MoveRadius   = 14;
    [Range(10, 50)]
    public int AttackRadius = 14;

    [TextArea]
    public string Description = "This unit doesn't have a description";

    // "FALSE = FRIENDLY / TRUE = ENEMY"
    [HideInInspector]
    public bool Team;

    public int GetMaxMaxHealth() => _maxMaxHealth;
    public int GetMaxAtkDmg()    => _maxAtkDmg;
}
