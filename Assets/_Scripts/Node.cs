using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

#pragma warning disable CS0660  // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.Equals(object o)

[Serializable]
public struct NodeType
{
    public NodeType(string n, int w, Color c)
    {
        Name = n;
        Weight = w;
        Color = c;
    }

    [field: SerializeField] public string Name   { get; set; }
    [field: SerializeField] public int    Weight { get; set; }
    [field: SerializeField] public Color  Color  { get; set; }

    public static bool operator ==(NodeType lhs, NodeType rhs)
    {
        return lhs.Name == rhs.Name && lhs.Weight == rhs.Weight && lhs.Color == rhs.Color;
    }
    public static bool operator !=(NodeType lhs, NodeType rhs) => !(lhs == rhs);
}

#nullable enable
[RequireComponent(typeof(SpriteRenderer))]
public class Node : IAudioPlayer
{
    private SpriteRenderer _sprite;

    [SerializeField] private AudioClip _hoverSound;

    public static readonly NodeType NORMAL_TYPE =
                        new NodeType("Normal", 0, new Color(0.64f, 0.64f, 0.64f));
    public static readonly NodeType WALL_TYPE =
                        new NodeType("Wall", 0, Color.black);

    public NodeType Nodetype { get; private set; }

    public IHoverable? PlacedItem           => GetPlacedItem();
    public Unit?       Unit   { get; set; } = null;
    public IPickup?    Pickup { get; set; } = null;

    [field: Header("Node Colors")]
    [field: SerializeField] public Color MainColor      { get; private set; }
    [field: SerializeField] public Color OffsetColor    { get; private set; }
    [field: SerializeField] public Color OnHoverColor   { get; private set; }

    [field: Header("Game Colors")]
    [field: SerializeField] public Color ValidMovesColor { get; private set; }
    [field: SerializeField] public Color MoveRangeColor  { get; private set; }
    [field: SerializeField] public Color EnemyPathColor  { get; private set; }
    [field: SerializeField] public Color EnemyRangeColor { get; private set; }

    private Color _assignedColor;

    #region PATHFINDING VARIABLES
    [field: Header("Pathfinding variables")]
    public float G { get; set; }
    public float H { get; set; }
    public float F => G + H;
    public List<Node> Neighbors { get; private set; }
    public Node       Connection { get; private set; }
    // directions of all neighbors
    private static readonly List<Vector2> NEIGHBOR_DIRS = new List<Vector2>() {
            new Vector2(0, 1), new Vector2(-1, 0), new Vector2(0, -1), new Vector2(1, 0),
            new Vector2(1, 1), new Vector2(1, -1), new Vector2(-1, -1), new Vector2(-1, 1)
    };
    #endregion

    public SpriteRenderer GetSprite()    => _sprite; 
    public void SetColor(Color color)    => _sprite.color = color;
    public void SetConnection(Node unit) => Connection = unit;
    public void SetType(NodeType type, bool ignoreNormal = true) { 
        Nodetype = type;

        if (type == NORMAL_TYPE && ignoreNormal)
            return; 

        SetColor(type.Color); 
    }
    private IHoverable GetPlacedItem() 
    {
        if (Unit != null)
            return Unit;
        else if (Pickup != null)
            return Pickup;
        else
            return null;
    }

    private void Awake() { _sprite = GetComponent<SpriteRenderer>(); SetupAudio(); }

    #region PATHFINDING
    public void CacheNeighbors()
    {
        Neighbors = new List<Node>();

        // we respectivly add every dir to our current node and make it a Neighbor if its not null
        foreach (var node in NEIGHBOR_DIRS
                .Select(dir => GridManager.Instance.GetNodeAtExactPosition((Vector2)transform.position + dir))
                .Where(node => node != null))
            Neighbors.Add(node);
    }
    public float GetDistance(Node other, bool ignoreWeight = true)
    {
        var dist = new Vector2Int(Mathf.Abs((int)transform.position.x - (int)other.transform.position.x),
                                  Mathf.Abs((int)transform.position.y - (int)other.transform.position.y));

        var lowest = Mathf.Min(dist.x, dist.y);
        var highest = Mathf.Max(dist.x, dist.y);
        var horizontal = highest - lowest;

        return ignoreWeight ? (lowest * 14 + horizontal * 10) : 
            (lowest * 14 + horizontal * 10) + Nodetype.Weight;
    }
    #endregion

    private void OnMouseEnter()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        PlaySoundWithVariablePitch(_hoverSound);

        if (Unit != null)
        {
            //Unit.ShowUnitInfo();
            if (Unit.Stats.Team)
            {
                var enemyUnit = (EnemyUnit)Unit;
                Pathfinding.GetPath(this, enemyUnit.FindClosestHero().Position)
                    .ForEach(node => node.SetColor(EnemyPathColor));

                Neighbors.ForEach(node =>
                {
                    if (node._sprite.color != EnemyPathColor) node.SetColor(EnemyRangeColor);
                });
            }
        }
        if (PlacedItem != null/* && !GameManager.Instance.IsTutorialScene*/)
            PlacedItem.ShowInfo();

        _assignedColor = _sprite.color;
        _sprite.color  = OnHoverColor;
    }
    private void OnMouseExit()
    {
        if (_sprite.color != OnHoverColor)
            return;

        if (PlacedItem != null)
            GUIManager.Instance.HideInfoGUI();

        if (Unit != null)
        {
            GUIManager. Instance.HideSelectedUnitInfo();
            
            GridManager.Instance.ResetNodes();
            GameManager.Instance.GetValidMoves();
        }
        else
            _sprite.color = _assignedColor;
    }
    private void OnMouseUp()
    {
        // making GUI block clicks
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (GameManager.Instance.HeroToPlace != null &&
            GameManager.Instance.CoinCointer >= GameManager.Instance.HeroToPlace.Cost &&
            PlacedItem == null)
        {
            Unit.CreateUnit(GameManager.Instance.HeroToPlace, this);
            GameManager.Instance.CoinCointer -= GameManager.Instance.HeroToPlace.Cost;
            return;
        }

        if (GameManager.Instance.ValidMoves.Contains(this))
        {
            GameManager.Instance.SelectedUnit.PerformAbility
                (GameManager.Instance.SelectedAbility, this);
        }
    }
}
