using System.Collections.Generic;
using UnityEngine;

namespace Utilities.Pathfinding.GridHandling
{
    public class GridGraphAstarNode : IAstarNode
    {
        public GridGraphAstarNode(bool isWalkable, Vector2 worldPos, Vector2Int inGridPos, List<IAstarNode> neighbours, GridGraph graph)
        {
            IsWalkable = isWalkable;
            WorldPos = worldPos;
            InGridPos = inGridPos;
            Neighbours = neighbours;
            Graph = graph;
        }

        public bool IsWalkable { get; set; }
        public int GCost { get; set; }
        public int HCost { get; set; }

        public Vector2 WorldPos { get; set; }

        public List<IAstarNode> Neighbours { get; set; }
        public IAstarNode ParentNode { get; set; }

        public Vector2Int InGridPos { get; set; }

        public GridGraph Graph { get; set; }
    }
}
