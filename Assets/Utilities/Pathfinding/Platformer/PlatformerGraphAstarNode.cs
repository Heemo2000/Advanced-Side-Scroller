using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Utilities.InterfaceSerialization;

namespace Utilities.Pathfinding.Platformer
{
    public class PlatformerGraphAstarNode : MonoBehaviour, IAstarNode
    {
        #region Constants

        private static float ShowRadius = 1.0f;

        #endregion
        [SerializeField] private bool _isWalkable = true;
        
        [Tooltip("Drag the references of PlatformerGraphAstarNode nodes here.")]
        [SerializeField] private List<InterfaceReference<IAstarNode>> _neighbours;


        private List<IAstarNode> _actualNeighbours;
        public bool IsWalkable { get => _isWalkable; set => _isWalkable = value; }
        public int GCost { get; set; }
        public int HCost { get; set; }
        public Vector2 WorldPos { get => transform.position; set => transform.position = new Vector3(value.x, value.y, 0.0f); }
        public List<IAstarNode> Neighbours { get => _actualNeighbours; set => SetNeighboursValues(value); }
        public IAstarNode ParentNode { get; set; }
        public PlatformerGraph Graph { get; set; }

        private List<IAstarNode> GetNeighbours(List<InterfaceReference<IAstarNode>> neighbours)
        {
            List<IAstarNode> neighboursList = new List<IAstarNode>();

            foreach(IAstarNode node in neighbours)
            {
                neighboursList.Add(node);
            }

            return neighboursList;
        }

        private void SetNeighboursValues(List<IAstarNode> neighbours)
        {
            _neighbours.Clear();

            foreach(IAstarNode node in neighbours)
            {
                _neighbours.Add(new InterfaceReference<IAstarNode>()
                {
                    UnderlyingValue = node as UnityEngine.Object
                });
            }

            _actualNeighbours = neighbours;
        }

        private void Start()
        {
            _actualNeighbours = GetNeighbours(_neighbours);
        }

        private void OnDrawGizmosSelected()
        {
            #if UNITY_EDITOR

            Handles.color = Color.green;
            Handles.DrawWireDisc(transform.position, Vector3.forward, ShowRadius);

            foreach(var nodes in _neighbours)
            {
                Handles.DrawLine(transform.position, nodes.Value.WorldPos);
            }
            
            #endif
        }
    }
}
