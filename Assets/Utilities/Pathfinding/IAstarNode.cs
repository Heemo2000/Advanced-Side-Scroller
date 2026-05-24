using System.Collections.Generic;
using UnityEngine;


namespace Utilities.Pathfinding
{
    public interface IAstarNode
    {
        bool IsWalkable { get; set; }
        int GCost { get; set; }
        int HCost { get; set; }
        int FCost { get => GCost + HCost; }
        Vector2 WorldPos { get; }
        List<IAstarNode> Neighbours { get; set; }
        IAstarNode ParentNode { get; set; }
    }
}

