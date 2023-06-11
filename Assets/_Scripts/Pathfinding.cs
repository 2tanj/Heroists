using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    // AStar pathfidning
    public static List<Node> GetPath(Node start, Node target)
    {
        if (start == null || target == null)
            return null;

        GridManager.Instance.ResetNodes();

        var toSearch = new List<Node>() { start };
        var searched = new List<Node>();

        while (toSearch.Any())
        {
            var current = toSearch[0];
            foreach (var node in toSearch)
                if (node.F < current.F || node.F == current.F && node.H < current.H)
                    current = node;

            searched.Add(current);
            toSearch.Remove(current);
            //current.SetColor(current.ProcessedColor);

            if (current == target)
            {
                var currentPathTile = target;
                var path = new List<Node>();

                while (currentPathTile != start)
                {
                    path.Add(currentPathTile);
                    currentPathTile = currentPathTile.Connection;
                }

                //path.ForEach(node => node.SetColor(node.PathColor));
                //start.SetColor(start.PathColor);

                return path;
            }

            foreach (var neighbor in current.Neighbors.Where(node => !(node.Nodetype.Name == "Wall") && !searched.Contains(node)))
            {
                bool inSearch = toSearch.Contains(neighbor);
                var costToNeighbor = current.G + current.GetDistance(neighbor, false);

                if (!inSearch || costToNeighbor < neighbor.G)
                {
                    neighbor.G = costToNeighbor;
                    neighbor.SetConnection(current);

                    if (!inSearch)
                    {
                        neighbor.H = neighbor.GetDistance(target, false);
                        toSearch.Add(neighbor);
                        //neighbor.SetColor(neighbor.NeighbourColor);
                    }
                }
            }
        }

        return null;
    }
}
