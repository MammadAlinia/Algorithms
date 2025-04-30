using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        private bool calculating = false;


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

            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Space))
            {
                if (!calculating)
                    _ = CalculateShortestPath();
            }

            UpdateColors();
        }

        Dictionary<GridCell, float> fCost = new Dictionary<GridCell, float>();
        Dictionary<GridCell, GridCell> cameFrom = new Dictionary<GridCell, GridCell>();
        private GridCell currentNode = new GridCell();
        public List<GridCell> shortestPath = new List<GridCell>();

        private async Task CalculateShortestPath()
        {
            // calculate shortest path via a* algorithm

            if (_targetNodes.Count == 0)
                return;

            if (_valid[_startNode.GridPosition.x, _startNode.GridPosition.y] == false)
                return;

            if (_targetNodes.Contains(_startNode))
                return;

            Debug.Log("calculate");

            calculating = true;
            fCost = new Dictionary<GridCell, float>();
            cameFrom = new Dictionary<GridCell, GridCell>();

            currentNode = _startNode;
            var targetNode = _targetNodes.First();


            var toVisit = new List<GridCell>();
            var visited = new HashSet<GridCell>();

            var hCost = new Dictionary<GridCell, float>();
            var gCost = new Dictionary<GridCell, float>();

            toVisit.Add(currentNode);

            while (toVisit.Count > 0)
            {
                //  currentNode = toVisit.First();
                currentNode = toVisit.OrderBy(n => fCost.GetValueOrDefault(n, float.MaxValue)).First();

                await Awaitable.MainThreadAsync();
                await Awaitable.NextFrameAsync();

                if (currentNode.Equals(targetNode))
                {
                    // construct the shortest path
                    ReconstructPath(currentNode);
                    calculating = false;
                    break;
                }

                toVisit.Remove(currentNode);
                visited.Add(currentNode);

                var neighbors = _grid.GetNeighbors(currentNode);


                foreach (var neighbor in neighbors)
                {
                    if (!visited.Contains(neighbor) &&
                        _valid[neighbor.GridPosition.x, neighbor.GridPosition.y])
                    {
                        float tentativeGCost = gCost.GetValueOrDefault(currentNode, 0) + 1;

                        if (!gCost.ContainsKey(neighbor) || tentativeGCost < gCost[neighbor])
                        {
                            cameFrom[neighbor] = currentNode;
                            gCost[neighbor] = tentativeGCost;
                            hCost[neighbor] = Vector2Int.Distance(neighbor.GridPosition, targetNode.GridPosition);
                            fCost[neighbor] = gCost[neighbor] + hCost[neighbor];

                            if (!toVisit.Contains(neighbor))
                                toVisit.Add(neighbor);
                        }
                    }
                }
            }

            calculating = false;
        }

        void ReconstructPath(GridCell current)
        {
            var path = new List<GridCell>();

            while (cameFrom.ContainsKey(current))
            {
                path.Add(current);
                current = cameFrom[current];
            }

            path.Reverse();
            shortestPath = new List<GridCell>(path);
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


                    color = _startNode.Equals(cell) ? Color.green : color;
                    color = shortestPath.Contains(cell) ? Color.blue : color;
                    color = _valid[x, y] ? color : Color.black;

                    color = _targetNodes.Contains(cell) ? Color.red : color;


                    _gridDrawer2D.UpdateColor(x, y, color);
                }
            }
        }

        private void UpdateTargetPoint()
        {
            if (Input.GetMouseButtonDown(1))
            {
                if (!_valid[_selectedNode.GridPosition.x, _selectedNode.GridPosition.y] ||
                    _selectedNode.Equals(_startNode))
                    return;

                if (_targetNodes.Contains(_selectedNode))
                    _targetNodes.Remove(_selectedNode);
                else
                    _targetNodes.Add(_selectedNode);
            }
        }

        private void UpdateStartPoint()
        {
            if (Input.GetMouseButtonDown(0)) // set start point
            {
                _prevSelectedNode = _startNode;
                _startNode = _selectedNode;
                _targetNodes.Remove(_startNode);
            }
        }

        private void UpdateValidCell()
        {
            var x = _selectedNode.GridPosition.x;
            var y = _selectedNode.GridPosition.y;

            if (Input.GetKeyDown(KeyCode.Space)) // add or remove cells
            {
                _valid[x, y] =
                    !_valid[x, y];
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

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_grid != null && currentNode != null)
            {
                Gizmos.DrawWireSphere(currentNode.WorldPosition, .4f);

                foreach (var f in fCost)
                {
                    Handles.color = Color.black;
                    Handles.Label(f.Key.WorldPosition, f.Value.ToString("F1"));
                }
            }
        }
#endif
    }
}