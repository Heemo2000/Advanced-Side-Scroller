using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utilities.IOC;
namespace Utilities.Pathfinding.Platformer
{
    public class PlatformerPathfindingManager : MonoBehaviour
    {
        #region Constants
        private const float RegisterInstanceCheckInterval = 1.0f;
        #endregion
        [SerializeField] private Tilemap _tilemap;
        [SerializeField] private List<PlatformerGraphAstarNode> _nodesList;

        private PlatformerGraph _graph;
        private AstarPathfinding _pathfinding;
        

        private void Start()
        {
            if (_nodesList == null || _nodesList.Count == 0)
            {
                _nodesList = GenerateNodesList();
            }
            
            _graph = new PlatformerGraph(_nodesList);
            _pathfinding = new AstarPathfinding(_graph, EuclideanHeuristic, true);
            ServiceLocator sceneServiceLocator = ServiceLocator.ForSceneOf(this);
            sceneServiceLocator.Register(this);
        }

        public List<PlatformerGraphAstarNode> GenerateNodesList()
        {
            if(_tilemap == null)
            {
                throw new System.ArgumentException("The property _tilemap is null!");
            }

            _tilemap.CompressBounds();
            BoundsInt tilemapBounds = _tilemap.cellBounds;
            TileBase[] allTiles = _tilemap.GetTilesBlock(tilemapBounds);

            List<PlatformerGraphAstarNode> nodesList = new List<PlatformerGraphAstarNode>();

            //This dictionary is responsible for storing a list of nodes which are at same X-axis position.
            Dictionary<int, List<PlatformerGraphAstarNode>> nodesInXAxisDict = 
                            new Dictionary<int, List<PlatformerGraphAstarNode>>();

            Dictionary<int, List<PlatformerGraphAstarNode>> nodesInYAxisDict =
                            new Dictionary<int, List<PlatformerGraphAstarNode>>();


            //This dictionary is responsible for storing the positions of all
            //PlatformerGraphAstarNode nodes.
            Dictionary<int, Vector3Int> positionsDict = new Dictionary<int, Vector3Int>();


            for (int x = 0; x < tilemapBounds.size.x; x++)
            {
                for (int y = 0; y < tilemapBounds.size.y; y++)
                {
                    TileBase tile = allTiles[x + y * tilemapBounds.size.x];
                    if(tile != null)
                    {
                        Vector3Int cellPos = new Vector3Int(tilemapBounds.xMin + x, tilemapBounds.yMin + y, 0);
                        Vector3Int topPos = cellPos + Vector3Int.up;
                        if (_tilemap.HasTile(topPos))
                        {
                            continue;
                        }

                        PlatformerGraphAstarNode node = new GameObject("Node " + topPos.ToString()).AddComponent<PlatformerGraphAstarNode>();
                        node.transform.parent = transform;
                        node.IsWalkable = true;
                        node.WorldPos = _tilemap.GetCellCenterWorld(topPos) - Vector3.up * _tilemap.cellSize.y / 2.0f;
                        nodesList.Add(node);

                        if(!nodesInXAxisDict.ContainsKey(topPos.x))
                        {
                            nodesInXAxisDict.Add(topPos.x, new List<PlatformerGraphAstarNode>());
                        }

                        nodesInXAxisDict[topPos.x].Add(node);

                        if(!nodesInYAxisDict.ContainsKey(topPos.y))
                        {
                            nodesInYAxisDict.Add(topPos.y, new List<PlatformerGraphAstarNode>());
                        }

                        nodesInYAxisDict[topPos.y].Add(node);

                        positionsDict.Add(node.GetInstanceID(), topPos);
                    }
                }
            }

            //This list is responsible for storing all the founded neighbours of the
            //particular node. At each iteration before checking, we cleared this list out.
            List<PlatformerGraphAstarNode> tempNeighbours = new List<PlatformerGraphAstarNode>();
            
            foreach(PlatformerGraphAstarNode node in nodesList)
            {
                tempNeighbours.Clear();
                Vector3Int nodePosition = positionsDict[node.GetInstanceID()];

                //First, add all the left neighbours to the temp neighbours list
                //if they exist for this position-X.
                if(nodesInXAxisDict.ContainsKey(nodePosition.x - 1))
                {
                    tempNeighbours.AddRange(nodesInXAxisDict[nodePosition.x - 1]);
                }

                //Second, add all the right neighbours to the temp neighbours list
                //if they exist for this position-X.
                if (nodesInXAxisDict.ContainsKey(nodePosition.x + 1))
                {
                    tempNeighbours.AddRange(nodesInXAxisDict[nodePosition.x + 1]);
                }

                //First, add all the down neighbours to the temp neighbours list
                //if they exist for this position-Y.
                if (nodesInYAxisDict.ContainsKey(nodePosition.y - 1))
                {
                    tempNeighbours.AddRange(nodesInYAxisDict[nodePosition.y - 1]);
                }

                //Second, add all the up neighbours to the temp neighbours list
                //if they exist for this position-Y.
                if (nodesInYAxisDict.ContainsKey(nodePosition.y + 1))
                {
                    tempNeighbours.AddRange(nodesInYAxisDict[nodePosition.y + 1]);
                }

                bool isJumpable = false;

                foreach(PlatformerGraphAstarNode neighbour in tempNeighbours)
                {
                    Vector3Int neighbourPosition = positionsDict[neighbour.GetInstanceID()];
                    Vector3Int difference = neighbourPosition - nodePosition;

                    if(Mathf.Abs(difference.y) > 0 || (difference.y == 0 && Mathf.Abs(difference.x) > 1))
                    {
                        isJumpable = true;
                        break;
                    }
                }

                node.IsJumpable = isJumpable;

                
                List<IAstarNode> iNeighbours = new List<IAstarNode>();

                foreach(PlatformerGraphAstarNode neighbour in tempNeighbours)
                {
                    iNeighbours.Add(neighbour);
                }
                node.Neighbours = iNeighbours;
            }

            return nodesList;
        }

        public List<Vector2> FindPath(Vector2 positionA, Vector2 positionB)
        {
            PlatformerGraphAstarNode nodeA = _graph.FindNode(positionA);
            PlatformerGraphAstarNode nodeB = _graph.FindNode(positionB);

            return _pathfinding.FindPath(nodeA, nodeB);
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
