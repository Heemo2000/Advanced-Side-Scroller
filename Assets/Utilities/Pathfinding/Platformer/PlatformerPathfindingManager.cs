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

        public List<PlatformerGraphAstarNode> NodesList { get => _nodesList; set => _nodesList = value; }

        private void Start()
        {
            if (_nodesList == null || _nodesList.Count == 0)
            {
                _nodesList = GenerateNodesAndPartiallyCreateConnections();
            }
            
            _graph = new PlatformerGraph(_nodesList);
            _pathfinding = new AstarPathfinding(_graph, EuclideanHeuristic, true);
            ServiceLocator sceneServiceLocator = ServiceLocator.ForSceneOf(this);
            sceneServiceLocator.Register(this);
        }


        /// <summary>
        /// Generates just left and right for a particular node, doesn't work for nodes higher or lower.
        /// That is designer's headache.
        /// </summary>
        /// <returns>List of nodes with partial connections.</returns>
        public List<PlatformerGraphAstarNode> GenerateNodesAndPartiallyCreateConnections()
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

            //Now, find and connect neighbours.
            foreach(PlatformerGraphAstarNode node in nodesList)
            {
                Vector3Int position = positionsDict[node.GetInstanceID()];

                //First, connect nodes which are just left or right of each other.
                Vector3Int justLeftPosition = new Vector3Int(position.x - 1, position.y, position.z);
                Vector3Int justRightPosition = new Vector3Int(position.x + 1, position.y, position.z);
                
                List<IAstarNode> iNeighbours = new List<IAstarNode>();

                //If the particular position exists in both x-axis nodes dictionary and y-axis nodes dictionary, then add that node.
                //First for the left.
                if (nodesInXAxisDict.ContainsKey(justLeftPosition.x) && nodesInYAxisDict.ContainsKey(justLeftPosition.y))
                {
                    var justLeftNodesX = nodesInXAxisDict[justLeftPosition.x];
                    var justLeftNodesY = nodesInYAxisDict[justLeftPosition.y];

                    PlatformerGraphAstarNode justLeftNode = null;
                    foreach(var a in justLeftNodesX)
                    {
                        if(justLeftNode != null)
                        {
                            break;
                        }
                        foreach(var b in justLeftNodesY)
                        {
                            if(a.GetInstanceID() == b.GetInstanceID())
                            {
                                justLeftNode = a;
                                break;
                            }
                        }
                    }

                    if(justLeftNode != null)
                    {
                        iNeighbours.Add(justLeftNode);
                    }
                }

                //Then, for the right
                if (nodesInXAxisDict.ContainsKey(justRightPosition.x) && nodesInYAxisDict.ContainsKey(justRightPosition.y))
                {
                    var justRightNodesX = nodesInXAxisDict[justRightPosition.x];
                    var justRightNodesY = nodesInYAxisDict[justRightPosition.y];

                    PlatformerGraphAstarNode justLeftNode = null;
                    foreach (var a in justRightNodesX)
                    {
                        if (justLeftNode != null)
                        {
                            break;
                        }
                        foreach (var b in justRightNodesY)
                        {
                            if (a.GetInstanceID() == b.GetInstanceID())
                            {
                                justLeftNode = a;
                                break;
                            }
                        }
                    }

                    if (justLeftNode != null)
                    {
                        iNeighbours.Add(justLeftNode);
                    }
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
