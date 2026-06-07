using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities.Pathfinding
{
    public class AstarPathfinding
    {
        private IGraph graph;
        private bool smoothPath;
        private Func<IAstarNode, IAstarNode, int> heuristicFunc;
        public AstarPathfinding(IGraph graph, Func<IAstarNode, IAstarNode, int> heuristicFunc, bool smoothPath)
        {
            this.graph = graph;
            this.smoothPath = smoothPath;
            this.heuristicFunc = heuristicFunc;
            graph.Initialize();
        }

        public List<Vector2> FindPath(IAstarNode startNode, IAstarNode endNode)
        {
            if(startNode == null || endNode == null || !graph.Contains(startNode) || !graph.Contains(endNode) ||
               !startNode.IsWalkable || !endNode.IsWalkable)
            {
                return null;
            }


            this.graph.Reset();

            List<IAstarNode> openList = new List<IAstarNode>();
            List<IAstarNode> closeList = new List<IAstarNode>();

            openList.Add(startNode);

            while (openList.Count > 0)
            {
                IAstarNode currentNode = GetLeastNode(openList);
                openList.Remove(currentNode);

                if(currentNode == endNode)
                {
                    if(smoothPath)
                    {
                        return RemoveUnnecessaryNodes(RetracePath(startNode, currentNode));
                    }

                    return RetracePath(startNode, currentNode);
                }

                closeList.Add(currentNode);

                List<IAstarNode> neigbours = GetNeighbours(currentNode);

                foreach (IAstarNode node in neigbours)
                {
                    if(!node.IsWalkable || closeList.Contains(node))
                    {
                        continue;
                    }

                    int newCostToNeighbour = currentNode.GCost + GetHeuristicDistance(currentNode,  node);
                    
                    if(newCostToNeighbour < node.GCost || !openList.Contains(node))
                    {
                        node.GCost = newCostToNeighbour;
                        node.HCost = GetHeuristicDistance(node, endNode);
                        node.ParentNode = currentNode;

                        if(!openList.Contains(node))
                        {
                            openList.Add(node);
                        }
                    }
                }
            }

            return null;
        }

        public List<IAstarNode> FindPathNodes(IAstarNode startNode, IAstarNode endNode)
        {
            if (startNode == null || endNode == null || !graph.Contains(startNode) || !graph.Contains(endNode) ||
               !startNode.IsWalkable || !endNode.IsWalkable)
            {
                return null;
            }


            this.graph.Reset();

            List<IAstarNode> openList = new List<IAstarNode>();
            List<IAstarNode> closeList = new List<IAstarNode>();

            openList.Add(startNode);

            while (openList.Count > 0)
            {
                IAstarNode currentNode = GetLeastNode(openList);
                openList.Remove(currentNode);

                if (currentNode == endNode)
                {
                    return RetracePathNodes(startNode, currentNode);
                }

                closeList.Add(currentNode);

                List<IAstarNode> neigbours = GetNeighbours(currentNode);

                foreach (IAstarNode node in neigbours)
                {
                    if (!node.IsWalkable || closeList.Contains(node))
                    {
                        continue;
                    }

                    int newCostToNeighbour = currentNode.GCost + GetHeuristicDistance(currentNode, node);

                    if (newCostToNeighbour < node.GCost || !openList.Contains(node))
                    {
                        node.GCost = newCostToNeighbour;
                        node.HCost = GetHeuristicDistance(node, endNode);
                        node.ParentNode = currentNode;

                        if (!openList.Contains(node))
                        {
                            openList.Add(node);
                        }
                    }
                }
            }

            return null;
        }

        private IAstarNode GetLeastNode(List<IAstarNode> openList)
        {
            if (openList.Count == 0)
            {
                return null;
            }
            else if (openList.Count == 1)
            {
                return openList[0];
            }

            IAstarNode result = openList[0];

            for (int i = 1; i < openList.Count; i++)
            {
                IAstarNode node = openList[i];
                if (node.FCost < result.FCost)
                {
                    result = node;
                }
                else if (node.FCost == result.FCost)
                {
                    if (node.GCost < result.GCost)
                    {
                        result = node;
                    }
                }
            }

            return result;
        }
    
        private List<Vector2> RetracePath(IAstarNode startNode, IAstarNode endNode)
        {
            List<Vector2> result = new List<Vector2>();

            result.Add(endNode.WorldPos);

            IAstarNode currentNode = endNode.ParentNode;
            while (currentNode != startNode)
            {
                result.Add(currentNode.WorldPos);
                currentNode = currentNode.ParentNode;
            }

            result.Add(currentNode.WorldPos);

            result.Reverse();
            return result;
        }

        private List<IAstarNode> RetracePathNodes(IAstarNode startNode, IAstarNode endNode)
        {
            List<IAstarNode> result = new List<IAstarNode>();

            result.Add(endNode);

            IAstarNode currentNode = endNode.ParentNode;
            while (currentNode != startNode)
            {
                result.Add(currentNode);
                currentNode = currentNode.ParentNode;
            }

            result.Add(currentNode);

            result.Reverse();
            return result;
        }

        private List<IAstarNode> GetNeighbours(IAstarNode node)
        {
            return node.Neighbours;
        }
    
        private int GetHeuristicDistance(IAstarNode nodeA, IAstarNode nodeB)
        {
            return this.heuristicFunc.Invoke(nodeA, nodeB);
        }

        private List<Vector2> RemoveUnnecessaryNodes(List<Vector2> path)
        {
            if(path.Count <= 2)
            {
                return path;
            }

            List<Vector2> result = new List<Vector2>();

            result.Add(path[0]);

            for(int i = 1; i < path.Count - 1; i++)
            {
                Vector2 previousNode = path[i-1];
                Vector2 node = path[i];
                Vector2 nextNode = path[i+1];

                Vector2 direction1 = (node - previousNode).normalized;
                Vector2 direction2 = (nextNode - node).normalized;

                if(direction1 == direction2)
                {
                    continue;
                }
                else
                {
                    result.Add(node);
                }
            }

            result.Add(path[path.Count - 1]);

            return result;
        }
    }
}

