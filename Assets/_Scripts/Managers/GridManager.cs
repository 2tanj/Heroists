using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [SerializeField] private Node _nodePrefab;

    // start = false, target = true
    private bool _isStartSet; // so we dont set the start multiple times on beginning

    [SerializeField]
    private int _width = 5, _height = 5;
    
    // references to the prefabs
    [field: SerializeField] public List<Unit> AllHeroes { get; private set; }
    [field: SerializeField] public List<Unit> AllEnemies{ get; private set; }

    [field: SerializeField]
    public List<NodeType> NodeTypes    { get; private set; }
    public NodeType       SelectedType { get; private set; }

    public List<Node> Nodes { get; private set; }

    void Awake() => Instance = this;
    void Start()
    {
        NodeTypes.Add(Node.NORMAL_TYPE);
        NodeTypes.Add(Node.WALL_TYPE);

        SelectedType = Node.NORMAL_TYPE;

        Nodes = new List<Node>();
        DrawGrid();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
            SpawnUnitOnClick(AllHeroes[0]);
        else if (Input.GetKeyDown(KeyCode.E))
            SpawnUnitOnClick(AllEnemies[0]);
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            var clickedNode = GetNodeAtPosition(CameraController.GetWorldPosition(0));
            if (clickedNode != null && clickedNode.PlacedItem == null)
                GameManager.Instance.Pickups[Random.Range(0, GameManager.Instance.Pickups.Count)].Spawn(clickedNode);
        }
    }

    public void DrawGrid()
    {
        if (!(Nodes.Count <= 1))
        {
            Nodes.ForEach(s => Destroy(s.gameObject));
            Nodes.Clear();
        }
        
        _isStartSet = false;

        InitNodes();
        Debug.Log("Node count:" + Nodes.Count);

        // moving the camera to the middle of the grid
        var pos = new Vector3(((float)_width / 2) - .5f, ((float)_height / 2) - .5f, 
                               _width > _height ? (-_width - 5) : (-_height - 5));
        Camera.main.transform.position = pos;
    }

    private void InitNodes()
    {
        _width = Random.Range(3, 10);
        _height= Random.Range(3, 10);

        for (int x = 0; x < _width; x++)
            for (int y = 0; y < _height; y++)
            {
                var node = Instantiate(_nodePrefab, new Vector3(x, y, 0),
                    Quaternion.identity, transform);

                node.name = $"Node {x} {y}";
                node.SetColor(((x + y) % 2 == 0) ? node.MainColor : node.OffsetColor);
                node.SetType(Node.NORMAL_TYPE);

                Nodes.Add(node);
            }

        // we do this after the nested loop because all neighbours of a node are not already initialised
        Nodes.ForEach(n => n.CacheNeighbors());

        if (!_isStartSet)
        {
            _isStartSet = true;
            InitUnits();
        }
    }

    public void RandomizeNodes()
    {
        // randomizing all nodes
        foreach (var node in Nodes)
        {
            var type = NodeTypes[Random.Range(0, NodeTypes.Count)];
            node.SetType(type, false);
            //node.SetColor(node.Nodetype.Color);
        }

        // setting the start and target nodes by getting two random non-wall nodes
        var noWalls = Nodes.Where(node => node.Nodetype != Node.WALL_TYPE).ToList();

        var heroNode = noWalls[Random.Range(0, noWalls.Count)];
        Node enemyNode;
        // making sure the nodes arent the same
        do
            enemyNode = noWalls[Random.Range(0, noWalls.Count)];
        while (enemyNode == heroNode);

        var startHero = Unit.CreateUnit(AllHeroes[0], heroNode);
                        Unit.CreateUnit(AllEnemies[0], enemyNode);

        GameManager.Instance.SelectedUnit = startHero;
    }

    // resetAll=true resets all types: walls, sand...
    public void ResetNodes(bool resetAll = false)
    {
        int last = -1;
        for (int i = 0; i < Nodes.Count; i++)
        {
            // normalColor creates a random pattern(depending on the size of the board)
            // which is used only for nodes of types normal
            // if a node is not of type normal we just get the color of its type
            var normalColor = (i+last) % 2 == 0 ? Nodes[i].MainColor : Nodes[i].OffsetColor;
            var finalColor = Nodes[i].Nodetype == Node.NORMAL_TYPE ? normalColor : Nodes[i].Nodetype.Color;

            if (resetAll)
            {
                 Nodes[i].SetType(Node.NORMAL_TYPE);
                 Nodes[i].SetColor(normalColor);
            }
            else Nodes[i].SetColor(finalColor);

            if (i % 2 == 0) last++;
        }
    }

    public Node GetNodeAtExactPosition(Vector2 pos)
    {
        foreach (var r in Nodes)
            if ((Vector2)r.transform.position == pos)
                return r;

        return null;
    }
    public Node GetNodeAtPosition(Vector2 pos)
    {
        foreach (var r in Nodes)
            if (pos.x <= r.transform.position.x + .5f && pos.x >= r.transform.position.x - .5f && 
                pos.y <= r.transform.position.y + .5f && pos.y >= r.transform.position.y - .5f)
                    return r;

        return null;
    }

    // function for changing the x and y values with sliders
    // if value is less then 0, we change the y value by inverting
    // if the value is more then 0 we change the x value
    // done like this for the sole purpose of not making more functions and duplicating code
    public void OnSliderValueChanged(Slider s)
    {
        int newSize = s.value > 0 ? (int)s.value : (int)s.value * -1;
        
        if (s.value > 0) _width  = newSize;
        else             _height = newSize;

        DrawGrid();
        GameManager.Instance.ValidMoves.Clear();
    }
    public void OnDropdownValueChanged(TMP_Dropdown d)
    {
        SelectedType = 
            NodeTypes.Single(type => type.Name == d.options[d.value].text);
    }
    public void OnResetGridButtonClicked()
    {
        ResetNodes(true);
    }

    private void InitDropdown()
    {
        var dropdown = FindObjectOfType<TMP_Dropdown>();
        // initializing the options from the NodeTypes list
        NodeTypes.ForEach(type => dropdown.options.Add(new TMP_Dropdown.OptionData(type.Name)));

        // initializing the default selected value
        for (int i = 0; i < dropdown.options.Count; i++)
            if (dropdown.options[i].text == "Normal")
                dropdown.value = i;
    }

    private void InitUnits()
    {
        // we init units inside this function
        RandomizeNodes();

        //var startHero = Unit.CreateUnit(AllHeroes[0], Nodes.First());
        //Unit.CreateUnit(AllEnemies[0], Nodes.Last());

        //GameManager.Instance.SelectedUnit = startHero;

        Debug.Log("Units initialized");

        StartCoroutine(GameManager.Instance.StartGame(.5f));
    }

    private void SpawnUnitOnClick(Unit u)
    {
        var clickedNode = GetNodeAtPosition(CameraController.GetWorldPosition(0));
        if (clickedNode != null && clickedNode.Unit == null)
            Unit.CreateUnit(u, clickedNode);
    }
}
