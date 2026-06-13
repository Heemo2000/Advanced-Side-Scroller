using System.Collections.Generic;
using UnityEngine;

using Utilities.IOC;
namespace Utilities.Pathfinding.Platformer
{
    public class PlatformerPathfindingManager : MonoBehaviour
    {
        #region Constants
        private const float RegisterInstanceCheckInterval = 1.0f;
        #endregion
        [SerializeField] private List<PlatformerGraphAstarNode> _nodesList;

        private PlatformerGraph _graph;
        private AstarPathfinding _pathfinding;
        
        private void Awake()
        {
            _graph = new PlatformerGraph(_nodesList);
            _pathfinding = new AstarPathfinding(_graph, EuclideanHeuristic, true);
        }

        private void Start()
        {
            ServiceLocator sceneServiceLocator = ServiceLocator.ForSceneOf(this);
            sceneServiceLocator.Register(this);
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
