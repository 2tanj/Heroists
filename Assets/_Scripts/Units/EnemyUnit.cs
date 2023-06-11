using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyUnit : Unit, IEnemyAI
{
    [SerializeField]
    protected List<IState> _states;

    [SerializeField] protected float _healthToRetreatThreshold = 3;
    [SerializeField] protected float _moveDelay                = .3f;

    // stavljamo referencu state-a dirktno na prefab!!
    public StateMachine FSM { get; private set; }

    protected virtual void Awake() 
    {
        Stats      = ScriptableObject.Instantiate(_statsReference);
        Stats.Team = true;
        FSM        = new StateMachine(this);
    }

    public override void PerformAbility(AbilityType? type, Node? n)
    {
        // following the enemy
        CameraController.FollowUnit(this);

        StartCoroutine(WaitAndDo(_moveDelay, () => {
            MakeDecision();
            FSM.Execute();
        }));
    }

    //TODO: add fuzzy logic for deciding
    public virtual void MakeDecision()
    {
        Debug.Log(Stats.Name + " is thinking!");
        // if there are killable units(if result isnt null)
        // go after them
        if (CheckForKillableUnits())
            SetUnitState(new AttackState(), CheckForKillableUnits());
        else if (Health <= _healthToRetreatThreshold)
        {
            // if there are spawned health packs
            if (GridManager.Instance.Nodes.Where(node =>
                    node.Pickup != null &&
                    node.Pickup is HealthPU).Any())
            {
                // go after the closest heal
                SetUnitState(new HealState(), FindClosestHeal());
            }
            else
                // if there are no heals run from the closest hero
                SetUnitState(new RetreatState(), FindClosestHero());
        }
        // if there is a pickup closer than a hero, go after it
        else if (CheckForPickups())
            SetUnitState(new HealState(), CheckForPickups());
        else
            // attack the closest hero by default
            SetUnitState(new AttackState(), FindClosestHero());
    }

    protected void SetUnitState(IState stateInstance, object item)
    {
        var finalState = Instantiate(StateMachine.GetStateFromList(
                _states, stateInstance), transform);

        if (finalState != null) FSM.SetState(finalState);
        finalState.SetItem(item);
    }

    //BUG:  Check trello
    public Unit FindClosestHero()
    {
        // if multiple heroes are in attack range 
        // attack the weakest one
        var neighbourHeroes = Position.Neighbors
                .Where(node =>
                       node.Unit != null &&
                       node.Unit.Stats.Team != Stats.Team)
                .Select(node => node.Unit).ToList();

        if (neighbourHeroes != null && neighbourHeroes.Count() > 1)
        {
            Debug.Log("test");

            var weakest = neighbourHeroes[0];
            neighbourHeroes.ForEach(hero => {
                if (hero.Stats.AttackDamage + (hero.Stats.Defense * 5) <
                    weakest.Stats.AttackDamage + (weakest.Stats.Defense * 5))
                {
                    weakest = hero;
                }
            });
            return weakest;
        }


        var closest = GameManager.Instance.FriendlyUnits[0];
        if (Position.GetDistance(closest.Position) <= Stats.AttackRadius)
            return closest;

        return FindClosest(GameManager.Instance.FriendlyUnits.Select(
                            hero => hero.Position).ToList()).Unit;
    }

    private Node FindClosestHeal()
    {
        var heals = GridManager.Instance.Nodes.Where(node => 
                        node.Pickup != null &&                
                        node.Pickup is HealthPU).ToList();

        Node closest = heals[0];
        if (Position.GetDistance(closest) <= Stats.MoveRadius)
            return closest;

        return FindClosest(heals);
    }

    private Node CheckForPickups() 
    {
        var pickups = GridManager.Instance.Nodes.Where(node =>
                            node.Pickup != null).ToList();

        if (!pickups.Any())
            return null;

        var closestPu   = FindClosest(pickups);
        var closestHero = FindClosest(GameManager.Instance.FriendlyUnits.Select(
            hero => hero.Position).ToList());

        //Debug.Log("CLOSEST PU: " + (Pathfinding.GetPath(Position, closestPu)[0].F + 28));
        //Debug.Log("CLOSEST HERO: " + Pathfinding.GetPath(Position, closestHero)[0].F);

        if ((Pathfinding.GetPath(Position, closestPu)[0].F + 28) < Pathfinding.GetPath(Position, closestHero)[0].F)
            return closestPu;
        else
            return null;
    }

    private Unit CheckForKillableUnits()
    {
        var query = GridManager.Instance.Nodes.Where(
            node => node.Unit != null &&
                    node.Unit.Stats.Team != Stats.Team &&
                    node.Unit.Health <= Stats.AttackDamage
        ).FirstOrDefault();
        return query == null ? null : query.Unit;
    }

    protected Node FindClosest(List<Node> nodes)
    {
        var closest   = nodes[0];
        float currMin = Pathfinding.GetPath(Position, closest)[0].F;

        nodes.ForEach(node => {
            float dist = Pathfinding.GetPath(Position, node)[0].F;

            if (dist < currMin)
            {
                closest = node;
                currMin = dist;
            }
        });

        return closest;
    }
}
