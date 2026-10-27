using System.Collections.Generic;
using UnityEngine;

namespace Utilities.Pathfinding.Platformer
{
    public class PlatformerPathfindingPathResult
    {
        public List<IAstarNode> Path { get; private set; }
        public Vector2 Start {  get; private set; }
        public Vector2 Destination { get; private set; }

        public PlatformerPathfindingPathResult(List<IAstarNode> path, Vector2 start, Vector2 destination)
        {
            this.Path = path;
            this.Start = start;
            this.Destination = destination;
        }
    }
}
