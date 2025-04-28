using System;
using System.Collections.Generic;
using System.Linq;
using GraphSystem;
using GridSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Grid = UnityEngine.Grid;

namespace _Project
{
    public class SimpleGridTest : MonoBehaviour
    {
        [SerializeField] private int width, height;
        [SerializeField] private Vector2 cellSize;

        private GridSystem.Grid _grid;
        private GridDrawer2D _gridDrawer2D;

        private GridCell _prevSelectedNode;
        private GridCell _selectedNode;
        private GridCell _startNode;

        private List<GridCell> _prevNeighbors = new();
        private List<GridCell> _currentNeighbors = new();
        private readonly HashSet<GridCell> _targetNodes = new();

        private bool[,] _valid;


        private void OnValidate()
        {
            _grid = new GridSystem.Grid(width, height, transform.position, cellSize);
        }

        private void Start()
        {
            _grid = new GridSystem.Grid(width, height, transform.position, cellSize);

            var xFactor = width * cellSize.x;
            var yFactor = height * cellSize.y;
            Camera.main.orthographicSize = ((xFactor / 1.5f + yFactor) / 2) * .65f;

            _gridDrawer2D = new GridDrawer2D(_grid);
            _valid = new bool[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _valid[x, y] = true;
                }
            }
        }


        private void Update()
        {
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            _selectedNode = _grid.WorldToCell(mousePos);

            if (!_selectedNode.Equals(_prevSelectedNode))
            {
                _prevNeighbors = _currentNeighbors;
                _currentNeighbors = _grid.GetNeighbors(_selectedNode);
            }

            UpdateValidCell();


            UpdateScale();

            UpdateStartPoint();

            UpdateTargetPoint();

            if (Input.GetKeyDown(KeyCode.G))
            {
                CalculateShortestPath();
            }

            UpdateColors();
        }

        Dictionary<GridCell, float> fCost = new Dictionary<GridCell, float>();

        private async void CalculateShortestPath()
        {
            fCost = new Dictionary<GridCell, float>();
            // calculate shortest path via a* algorithm

            if (_targetNodes.Count == 0)
            {
                return;
            }

            var currentNode = _startNode;
            var targetNode = _targetNodes.First();


            var toVisit = new Queue<GridCell>();
            var visited = new HashSet<GridCell>();

            var hCost = new Dictionary<GridCell, float>();
            var gCost = new Dictionary<GridCell, float>();

            toVisit.Enqueue(currentNode);

            while (toVisit.Count > 0)
            {
                Debug.Break();
                await Awaitable.NextFrameAsync();
                Debug.DrawLine(currentNode.WorldPosition, currentNode.WorldPosition + Vector3.up * .3f, Color.red);

                if (currentNode.Equals(targetNode))
                {
                    break;
                }

                currentNode = toVisit.Dequeue();
                visited.Add(currentNode);


                var neighbors = _grid.GetNeighbors(currentNode);

                hCost[currentNode] = Vector2Int.Distance(currentNode.GridPosition, targetNode.GridPosition);
                fCost[currentNode] = hCost[currentNode] + gCost.GetValueOrDefault(currentNode, 0);


                foreach (var neighbor in neighbors)
                {
                    if (!visited.Contains(neighbor) && !toVisit.Contains(neighbor)&& _valid[neighbor.GridPosition.x, neighbor.GridPosition.y])
                    {
                        gCost[neighbor] = gCost.GetValueOrDefault(currentNode, 0) + 1;
                        toVisit.Enqueue(neighbor);
                    }
                }
            }
        }

        private void UpdateColors()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var cell = _grid.GetCell(x, y);
                    var color = _currentNeighbors.Contains(cell)
                        ? Color.gray
                        : Color.Lerp(Color.clear, Color.white, .3f);


                    color = _startNode != null && _startNode.Equals(cell) ? Color.green : color;
                    color = _targetNodes.Contains(cell) ? Color.blue : color;

                    color = _valid[x, y]
                        ? color
                        : Color.black;


                    _gridDrawer2D.UpdateColor(x, y, color);
                }
            }
        }

        private void UpdateTargetPoint()
        {
            if (Input.GetMouseButtonDown(1)) // add or remove target points
            {
                if (!_valid[_selectedNode.GridPosition.x, _selectedNode.GridPosition.y] ||
                    _selectedNode.Equals(_startNode))
                    return;


                if (_targetNodes.Contains(_selectedNode))
                {
                    _targetNodes.Remove(_selectedNode);
                    return;
                }


                _targetNodes.Add(_selectedNode);
            }
        }

        private void UpdateStartPoint()
        {
            if (Input.GetMouseButtonDown(0)) // set start point
            {
                _prevSelectedNode = _startNode;
                _startNode = _selectedNode;

                if (_targetNodes.Remove(_startNode))
                {
                }
            }
        }

        private void UpdateValidCell()
        {
            var gridPositionX = _selectedNode.GridPosition.x;
            var gridPositionY = _selectedNode.GridPosition.y;

            if (Input.GetKeyDown(KeyCode.Space)) // add or remove cells
            {
                _valid[gridPositionX, gridPositionY] =
                    !_valid[gridPositionX, gridPositionY];
            }
        }

        private void UpdateScale()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var cell = _grid.GetCell(x, y);

                    if (_selectedNode.Equals(cell))
                    {
                        _gridDrawer2D.SetSize(x, y, _grid.CellSize * .8f);
                    }
                    else
                    {
                        _gridDrawer2D.SetSize(x, y, _grid.CellSize * .95f);
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            foreach (var f in fCost)
            {
                Handles.color = Color.black;
                Handles.Label(f.Key.WorldPosition, f.Value.ToString("F1"));
            }
        }
    }
}