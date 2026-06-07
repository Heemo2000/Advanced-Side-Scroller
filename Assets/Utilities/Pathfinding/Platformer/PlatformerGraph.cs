using System.Collections.Generic;
using UnityEngine;

namespace Utilities.Pathfinding.Platformer
{
    public class PlatformerGraph : IGraph
    {
        private List<PlatformerGraphAstarNode> _nodes;

        public PlatformerGraph(List<PlatformerGraphAstarNode> nodes)
        {
            _nodes = nodes;
        }

        public bool Contains(IAstarNode node)
        {
            PlatformerGraphAstarNode platformerGraphAstarNode = node as PlatformerGraphAstarNode;
            
            return platformerGraphAstarNode.Graph == this && _nodes.Contains(platformerGraphAstarNode);
        }

        public void Initialize()
        {
            foreach (var node in _nodes)
            {
                node.Graph = this;
            }
            Reset();
        }

        public void Reset()
        {
            foreach (var node in _nodes)
            {
                node.HCost = 0;
                node.GCost = 0;
            }
        }

        public PlatformerGraphAstarNode FindNode(Vector2 position)
        {
            PlatformerGraphAstarNode result = null;
            float closestSqrDistance = float.MaxValue;

            foreach (var node in _nodes)
            {
                float sqrDistance = Vector2.SqrMagnitude(node.WorldPos -  position);
                if (sqrDistance < closestSqrDistance)
                {
                    closestSqrDistance = sqrDistance;
                    result = node;
                }
            }

            return result;
        }
    }
}
