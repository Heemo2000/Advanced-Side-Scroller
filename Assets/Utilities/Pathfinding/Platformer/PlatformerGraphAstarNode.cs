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
        [SerializeField] private bool _isJumpable = false;

        private List<IAstarNode> _actualNeighbours;
        public bool IsWalkable { get => _isWalkable; set => _isWalkable = value; }
        public int GCost { get; set; }
        public int HCost { get; set; }
        public Vector2 WorldPos { get => transform.position; set => transform.position = new Vector3(value.x, value.y, 0.0f); }
        public List<IAstarNode> Neighbours { get => _actualNeighbours; set => SetNeighboursValues(value); }
        public IAstarNode ParentNode { get; set; }
        public PlatformerGraph Graph { get; set; }
        public bool IsJumpable { get => _isJumpable; set => _isJumpable = value; }

        private List<IAstarNode> GetNeighbours(List<InterfaceReference<IAstarNode>> neighbours)
        {
            List<IAstarNode> neighboursList = new List<IAstarNode>();

            foreach(var node in neighbours)
            {
                neighboursList.Add(node.Value);
            }

            return neighboursList;
        }

        private void SetNeighboursValues(List<IAstarNode> neighbours)
        {
            if(_neighbours == null)
            {
                _neighbours = new List<InterfaceReference<IAstarNode>>();
            }

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
            if(_neighbours.Count == 0)
            {
                return;
            }
            #if UNITY_EDITOR

            Handles.color = Color.green;
            foreach(var node in _neighbours)
            {
                if(node.Value == null)
                {
                    continue;
                }
                Handles.DrawLine(transform.position, node.Value.WorldPos);
            }
            
            #endif
        }

        private void OnDrawGizmos()
        {
            #if UNITY_EDITOR
            Handles.color = Color.turquoise;
            Handles.DrawWireDisc(transform.position, Vector3.forward, ShowRadius);
            #endif
        }
    }
}
