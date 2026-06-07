using System.Collections.Generic;
using UnityEngine;


namespace Utilities.Pathfinding.Platformer
{
    public class PlatformerPathfindingManager : MonoBehaviour
    {
        [SerializeField] private List<PlatformerGraphAstarNode> _nodesList;

        private PlatformerGraph _graph;
        private AstarPathfinding _pathfinding;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _graph = new PlatformerGraph(_nodesList);
            _pathfinding = new AstarPathfinding(_graph, EuclideanHeuristic, true);
        }

        public List<IAstarNode> FindPathNodes(Vector2 positionA,  Vector2 positionB)
        {
            PlatformerGraphAstarNode nodeA = _graph.FindNode(positionA);
            PlatformerGraphAstarNode nodeB = _graph.FindNode(positionB);

            return _pathfinding.FindPathNodes(nodeA, nodeB);
        }

        private int EuclideanHeuristic(IAstarNode nodeA, IAstarNode nodeB)
        {
            return (int)Vector2.SqrMagnitude(nodeB.WorldPos - nodeA.WorldPos);
        }
    }
}
