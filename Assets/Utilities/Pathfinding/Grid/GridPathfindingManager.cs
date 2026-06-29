using System.Collections.Generic;
using UnityEngine;

namespace Utilities.Pathfinding.GridHandling
{
    public class GridPathfindingManager : MonoBehaviour
    {
        private const int StraightCost = 10;
        private const int DiagonalCost = 14;

        [Min(1)]
        [SerializeField] private int _width = 10;
        [Min(1)]
        [SerializeField] private int _height = 10;
        [Min(0.1f)]
        [SerializeField] private float _cellSize = 1.0f;
        [SerializeField] private LayerMask _checkLayerMask;

        private GridGraph _grid;
        private AstarPathfinding _pathfinding;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _grid = new GridGraph(transform.position, _width, _height, _cellSize, _checkLayerMask);
            _pathfinding = new AstarPathfinding(_grid, ManhattanHeuristic, true);
        }

        private void OnValidate()
        {
            if (_grid != null)
            {
                _grid.Origin = transform.position;
                _grid.CellSize = _cellSize;
                _grid.CheckLayerMask = _checkLayerMask;
            }
        }

        public List<Vector2> FindPath(Vector2 startPosition, Vector2 endPosition)
        {
            Vector2Int startXY = _grid.GetXY(startPosition);
            Vector2Int endXY = _grid.GetXY(endPosition);

            if(startXY.x < 0 || startXY.y < 0 || endXY.x < 0 || endXY.y < 0)
            {
                return null;
            }

            GridGraphAstarNode startNode = _grid.Grid[startXY.x, startXY.y];
            GridGraphAstarNode endNode = _grid.Grid[endXY.x, endXY.y];
            return _pathfinding.FindPath(startNode, endNode);
        }

        private int ManhattanHeuristic(IAstarNode a, IAstarNode b)
        {
            GridGraphAstarNode nodeA = a as GridGraphAstarNode;
            GridGraphAstarNode nodeB = b as GridGraphAstarNode;

            int xDistance = Mathf.Abs(nodeB.InGridPos.x - nodeA.InGridPos.x);
            int yDistance = Mathf.Abs(nodeB.InGridPos.y -  nodeA.InGridPos.y);

            return StraightCost * Mathf.Min(xDistance, yDistance) + DiagonalCost * Mathf.Abs(xDistance - yDistance);
        }

    }
}
