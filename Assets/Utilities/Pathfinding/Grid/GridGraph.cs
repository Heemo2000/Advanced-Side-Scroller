using System.Collections.Generic;
using UnityEngine;

namespace Utilities.Pathfinding.Grid
{
    public class GridGraph : IGraph
    {
        private GridGraphAstarNode[,] _grid;
        private Vector2 _origin;
        private int _width = 0;
        private int _height = 0;
        private float _cellSize = 0.0f;
        private LayerMask _checkLayerMask;

        public GridGraphAstarNode[,] Grid { get => _grid; set => _grid = value; }
        public Vector2 Origin { get => _origin; set => _origin = value; }
        public int Width { get => _width; set => _width = value; }
        public int Height { get => _height; set => _height = value; }
        public float CellSize { get => _cellSize; set => _cellSize = value; }
        public LayerMask CheckLayerMask { get => _checkLayerMask; set => _checkLayerMask = value; }

        public GridGraph(Vector2 origin,int width, int height, float cellSize, LayerMask checkLayerMask)
        {
            _origin = origin;
            _width = width;
            _height = height;
            _cellSize = cellSize;
            _grid = new GridGraphAstarNode[width, height];
            _checkLayerMask = checkLayerMask;
        }
        public bool Contains(IAstarNode node)
        {
            if(node.GetType() != typeof(GridGraphAstarNode))
            {
                throw new System.ArgumentException("node at position " + node.WorldPos + " cannot be cast into GridGraphAstarNode");
            }

            GridGraphAstarNode gridNode = node as GridGraphAstarNode;
            
            Vector2Int inGridPosition = gridNode.InGridPos;

            return gridNode.Graph == this && inGridPosition.x >= 0 && inGridPosition.x < _width && inGridPosition.y >= 0 && inGridPosition.y < _height;
        }

        public void Initialize()
        {
            for (int i = 0; i < _width; i++)
            {
                for (int j = 0; j < _height; j++)
                {
                    Vector2Int inGridPos = new Vector2Int(i, j);
                    Vector2 worldPosition = GetWorldPositionCentre(inGridPos);
                    
                    bool isWalkable = Physics2D.OverlapCircle(worldPosition, _cellSize / 2.0f, _checkLayerMask.value) == null;
                    _grid[i, j] = new GridGraphAstarNode(isWalkable, worldPosition, inGridPos, null, this);
                }
            }

            for (int i = 0; i < _width; i++)
            {
                for (int j = 0; j < _height; j++)
                {
                    _grid[i,j].Neighbours = CalculateNeighbours(i, j);
                }
            }
        }

        public void Reset()
        {
            for (int i = 0; i < _width; i++)
            {
                for (int j = 0; j < _height; j++)
                {
                    _grid[i, j].GCost = 0;
                    _grid[i, j].HCost = 0;
                    _grid[i, j].ParentNode = null;
                }
            }
        }

        public static Vector2Int GetXY(Vector2 origin, float cellSize, Vector2 worldPosition)
        {
            int x = Mathf.FloorToInt((worldPosition.x - origin.x)/cellSize);
            int y = Mathf.FloorToInt((worldPosition.y - origin.y)/cellSize);

            return new Vector2Int(x, y);
        }

        public Vector2Int GetXY(Vector2 worldPosition)
        {
            int x = Mathf.FloorToInt((worldPosition.x - _origin.x) / _cellSize);
            int y = Mathf.FloorToInt((worldPosition.y - _origin.y) / _cellSize);

            return new Vector2Int(x, y);
        }

        public static Vector2 GetWorldPosition(Vector2 origin, float cellSize, Vector2Int inGridPosition)
        {
            return origin + new Vector2(inGridPosition.x, inGridPosition.y) * cellSize;
        }

        public Vector2 GetWorldPosition(Vector2Int inGridPosition)
        {
            return _origin + new Vector2(inGridPosition.x, inGridPosition.y) * _cellSize;
        }

        public static Vector2 GetWorldPositionCentre(Vector2 origin, float cellSize, Vector2Int inGridPosition)
        {
            return origin + new Vector2(inGridPosition.x, inGridPosition.y) * cellSize + Vector2.one * cellSize / 2.0f;
        }

        public Vector2 GetWorldPositionCentre(Vector2Int inGridPosition) 
        {
            return _origin + new Vector2(inGridPosition.x, inGridPosition.y) * _cellSize + Vector2.one * _cellSize/2.0f;
        }

        public List<IAstarNode> CalculateNeighbours(int inGridX, int inGridY)
        {
            List<IAstarNode> neighbours = new List<IAstarNode>();

            for(int x = -1; x <= -1; x++)
            {
                for(int y = -1; y <= -1; y++)
                {
                    int currentX = inGridX + x;
                    int currentY = inGridY + y;

                    if ((x == 0 && y == 0) || currentX < 0 || currentX >= _width || currentX < 0 || currentY >= _height)
                    {
                        continue;
                    }

                    neighbours.Add(_grid[currentX, currentY]);
                }
            }

            return neighbours;
        }
    }
}
