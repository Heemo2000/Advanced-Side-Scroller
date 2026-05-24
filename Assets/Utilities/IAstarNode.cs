using System.Collections.Generic;
using UnityEngine;


namespace Utilities
{
    public interface IAstarNode
    {
        bool IsWalkable { get; set; }
        int GCost { get; set; }
        int HCost { get; set; }
        int FCost { get => GCost + HCost; }
        Vector3 WorldPos { get; }
        List<IAstarNode> Neighbours { get; set; }
        IAstarNode ParentNode { get; set; }
    }
}

