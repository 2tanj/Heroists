using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum AbilityType
{
    NONE      = 0,
    MOVEMENT  = 1,
    ATTACK    = 2,
    OFFENSIVE = 3,
    DEFENSIVE = 4,
    PLACEMENT = 5
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public AbilityType SelectedAbility { get; private set; } = AbilityType.NONE;

    [field: SerializeField]
    public bool IsTutorialScene { get; set; } = false;

    [field: SerializeField]
    private int _coinCounter = 10;
    public  int CoinCointer
    {
        get => _coinCounter;
        set {
            _coinCounter = value;
            GUIManager.Instance.CoinText.text = _coinCounter.ToString();
        }
    }
    private float _roundCounter = 1;
    public  float RoundCounter
    { 
        get => _roundCounter;
        set {
            _roundCounter = value;
            GUIManager.Instance.RoundText.text = _roundCounter.ToString();
            //if (_roundCounter % 1 == 0)
            //    OnRoundFinished?.Invoke();
        } 
    }
    [SerializeField] private int _enemySpawnCooldown = 5;

    // false = friendly / true = enemy
    public bool TeamsTurn { get; private set; } = false;
    public List<Unit> FriendlyUnits => GridManager.Instance.Nodes
                                .Select(node => node.Unit)
                                .Where(unit => unit != null && !unit.Stats.Team).ToList();
    public List<Unit> EnemyUnits    => GridManager.Instance.Nodes
                                .Select(node => node.Unit)
                                .Where(unit => unit != null && unit.Stats.Team).ToList();
    // selected hero in hero selection
    public HeroUnit HeroToPlace  { get; set; }

    // the unit thats currently playing
    public Unit     SelectedUnit { get; set; }

    [HideInInspector] public UnityEvent OnTurnFinished;
    [HideInInspector] public UnityEvent OnRoundFinished;
    //[HideInInspector] public UnityEvent OnRoundFinished;


    [field: SerializeField] public List<IPickup> Pickups    { get; private set; }
    [SerializeField]        private int          _pickupCooldown = 3; // the rate at which pickup's spawn
    
    // caching so we dont dont calculate constantly
    public List<Node>    ValidMoves { get; private set; }


    private void Awake() => Instance = this;
    private void Start()
    {
        OnTurnFinished .AddListener(() => {
            Debug.Log("Turn finished!");

            Debug.Log($"{EnemyUnits.Count} enemies, {FriendlyUnits.Count} friendies");

            if (EnemyUnits.Count <= 0 || FriendlyUnits.Count <= 0)
                SceneManager.LoadScene(3);
        });
        OnRoundFinished.AddListener(() => {
            Debug.Log("Round finished");

            SwitchPlayingTeam();
            
            if (RoundCounter % _pickupCooldown == 0)
                SpawnObject(Pickups[Random.Range(0, Pickups.Count)]);
            if (RoundCounter % _enemySpawnCooldown == 0)
                SpawnObject(GridManager.Instance.AllEnemies[Random.Range(0, GridManager.Instance.AllEnemies.Count)]); // TODO: add actual logic here instead of spawning the first one

            Debug.Log("Switching teams -" + RoundCounter);
            StartCoroutine(CycleTeamTurns(TeamsTurn ? EnemyUnits : FriendlyUnits));
        });

        ValidMoves = new List<Node>();
    }

    private void Update()
    {
        if (     Input.GetButtonUp("Movement"))
            SetAbilityType(AbilityType.MOVEMENT,  GUIManager.Instance.MoveButton);
        else if (Input.GetButtonUp("Attack"))
            SetAbilityType(AbilityType.ATTACK,    GUIManager.Instance.AttackButton);
        else if (Input.GetButtonUp("Cancel"))
        {
            SetAbilityType(AbilityType.NONE,      null);
            GUIManager.Instance.HideInfoGUI();
            GUIManager.Instance.HideSelectedUnitInfo();
        }
        else if (Input.GetButtonUp("Offensive"))
            SetAbilityType(AbilityType.OFFENSIVE, GUIManager.Instance.OffensiveButton);
        else if (Input.GetButtonUp("Defensive"))
            SetAbilityType(AbilityType.DEFENSIVE, GUIManager.Instance.DefensiveButton);

        else if (Input.GetKeyUp(KeyCode.Tab))
            GUIManager.Instance.OnSettingsPressed();
        else if (Input.GetKeyUp(KeyCode.F))
            StartCoroutine(StartGame());
    }

    public IEnumerator StartGame(float delay = 0)
    {
        yield return new WaitForSeconds(delay);

        if (IsTutorialScene)
        {
            EnemyUnits[0].gameObject.SetActive(false);
            Debug.Log("Tutorial started");
        }
        else
        {
            Debug.Log("Game started");
            SpawnObject(Pickups[Random.Range(0, Pickups.Count)]);
            yield return StartCoroutine(CycleTeamTurns(FriendlyUnits));
        }
    }
    private IEnumerator CycleTeamTurns(List<Unit> team)
    {
        foreach (var unit in team)
        {
            SelectedUnit = unit;
            GUIManager.Instance.ShowSelectedUnitInfo(SelectedUnit);

            yield return StartCoroutine(unit.WaitUntilAbilityIsPerformed());
            
            yield return new WaitForSeconds(.3f);
            OnTurnFinished?.Invoke();
        }
        OnRoundFinished?.Invoke();
    }

    private void GetValidAttackMoves(float radius, bool frienlyTeam = false)
    {
        bool teamToCheck = !frienlyTeam ? 
            SelectedUnit.Stats.Team : !SelectedUnit.Stats.Team;

        ValidMoves = GridManager.Instance.Nodes.Where(
            node => node.GetDistance(SelectedUnit.Position) <= radius && 
            node.Unit != null && 
            node.Unit.Stats.Team != teamToCheck && 
            node.Nodetype != Node.WALL_TYPE).ToList();

        if (frienlyTeam)
            ValidMoves.Add(SelectedUnit.Position);
    }
    private void DrawMoveRange(float radius) => 
        GridManager.Instance.Nodes.Where(
        node => node.GetDistance(SelectedUnit.Position) <= radius &&
        node.Nodetype != Node.WALL_TYPE &&
        node != SelectedUnit.Position).ToList().
            ForEach(node => node.SetColor(node.MoveRangeColor));

    public void DrawValidMoves()
    {
        if (ValidMoves.Count <= 0 || TeamsTurn)
            return;

        ValidMoves.ForEach(node => node.SetColor(node.ValidMovesColor));
    }
    public void GetValidMoves()
    {
        GridManager.Instance.ResetNodes();
        switch (SelectedAbility)
        {
            case AbilityType.NONE:
                ValidMoves.Clear();
                break;

            case AbilityType.MOVEMENT:
                ValidMoves = GridManager.Instance.Nodes.Where(node => 
                    node.GetDistance(SelectedUnit.Position) <= SelectedUnit.Stats.MoveRadius &&
                    node != SelectedUnit.Position &&
                    node.Nodetype != Node.WALL_TYPE &&
                    node.Unit == null).ToList();
                break;

            case AbilityType.ATTACK:
                GetValidMoves(SelectedUnit.Stats.AttackRadius);
                break;

            case AbilityType.OFFENSIVE:
                if (SelectedUnit.OffensiveAbility == null)
                {
                    Debug.LogWarning("Hero has no offensive ability!");
                    return;
                }
                GetValidMoves(SelectedUnit.OffensiveAbility.Range);
                break;

            case AbilityType.DEFENSIVE:
                if (SelectedUnit.DefensiveAbility == null)
                {
                    Debug.LogWarning("Hero has no defensive ability!");
                    return;
                }
                GetValidMoves(SelectedUnit.DefensiveAbility.Range, true);
                break;
        }

        DrawValidMoves();
    }
    public void GetValidMoves(float radius, bool includeFriendlyTeam = false) {
        if (TeamsTurn)
            return;
        
        GetValidAttackMoves(radius, includeFriendlyTeam);
        DrawMoveRange      (radius);
    }

    public void SetAbilityType(AbilityType type, CustomButton btn)
    {
        // if move == none we deselect the last selected button
        if (!btn)
            EventSystem.current.SetSelectedGameObject(null);
        else
        {
            btn.ButtonRef.Select();

            if (btn is AbilityButton)
                btn.PlayButtonSound();
        }

        SelectedAbility = type;
        GetValidMoves();
    }

    private void SwitchPlayingTeam()
    {
        RoundCounter += .5f;
        TeamsTurn     = !TeamsTurn;

        // stop following enemy
        CameraController.FollowUnit(null);
    }

    public void SpawnObject(object objectToSpawn)
    {
        var validNodes = GridManager.Instance.Nodes.Where(
            node => node.PlacedItem == null &&
            node.Nodetype != Node.WALL_TYPE).ToList();

        if (validNodes.Count <= 0)
        {
            Debug.LogWarning("No valid nodes for object spawn!");
            return;
        }

        if (objectToSpawn is IPickup)
        {
            IPickup ob = (IPickup)objectToSpawn;
            ob.Spawn(validNodes[Random.Range(0, validNodes.Count)]);
        }
        else if (objectToSpawn is Unit)
        {
            Unit ob = (Unit)objectToSpawn;
            Unit.CreateUnit(
                    unitPrefab: ob,
                    unitPos: validNodes[Random.Range(0, validNodes.Count)]);

        }
    }

    // TODO: put this in a helper class and use thorughout the whole codebase
    private T GetRandomItem<T>(List<T> list) => list[Random.Range(0, list.Count)];
}
