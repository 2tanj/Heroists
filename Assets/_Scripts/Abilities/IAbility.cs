using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable
// DONT FORGET FINISH ABILITY WHEN MAKING NEW ONES!!
[RequireComponent(typeof(SpriteRenderer))]
public abstract class IAbility : IAudioPlayer, IHoverable 
{
    [field: Header("Default ability parameters")]
    [field: SerializeField] public  SpriteRenderer ?Icon       { get; set; }
    [field: SerializeField] public  string         Name        { get; set; } = "Ability";
    [field: SerializeField] public  int            Cooldown    { get; set; } = 5;
    [field: SerializeField] public  int            Range       { get; set; } = 20;
    [SerializeField]        private AudioClip      _sound;
    [field: TextArea]
    [field: SerializeField] public string         Description { get; set; } = "This ability doesnt have a description. FIX IT!";

    public  bool IsNotOnCooldown => GameManager.Instance.RoundCounter >= UsedOnRound + Cooldown;
    [HideInInspector] public int  UsedOnRound = -100;

    void Start() { Icon = GetComponent<SpriteRenderer>();  SetupAudio(); }

    // offensive = true / deffensive = false
    public abstract void UseAbility(Unit player, Unit other, bool offensiveOrDefensive);

    internal void FinishAbility(Unit player) 
    { 
        PlaySound(_sound);
        player?.OnAbilityPerformed.Invoke(); 
        UsedOnRound = (int)GameManager.Instance.RoundCounter; 
    }
    internal bool CantBeUsed    (Unit player, Unit other, bool offOrDef)
    {
        Debug.Log("Trying to use: " + Name);
        string msg = IsNotOnCooldown ? "Succesfully not on CD" : "Ability on CD";
        Debug.Log(msg);

        bool b = offOrDef ? player.Stats.Team == other.Stats.Team : player.Stats.Team != other.Stats.Team;
        return (other == null || b || !IsNotOnCooldown);
    }

    public void ShowInfo() => GUIManager.Instance.ShowInfoGUI(
        Icon?.sprite, Name, Description);
}