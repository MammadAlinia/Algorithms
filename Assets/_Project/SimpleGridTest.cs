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
        private readonly List<GridCell> _targetNodes = new();

        private bool[,] _valid;

        private bool calculating = false;

        private Graph<GridCell> Graph;

        private void Start()
        {
            _grid = new GridSystem.Grid(width, height, transform.position, cellSize);

            Graph = new Graph<GridCell>(_grid.CellList,
                _grid.CellList.ToDictionary(x => x, x => _grid.GetNeighbors(x)));

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

        private List<GridCell> FindNeighbors(GridCell arg)
        {
            return _grid.GetNeighbors(arg).ToList();
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
                    ShortestPathDijkstra();
            }

            UpdateColors();
        }

        Dictionary<GridCell, float> fCost = new Dictionary<GridCell, float>();
        Dictionary<GridCell, GridCell> cameFrom = new Dictionary<GridCell, GridCell>();
        private GridCell currentNode = new GridCell();
        public Dictionary<GridCell, List<GridCell>> shortestPath = new Dictionary<GridCell, List<GridCell>>();

        private void ShortestPathAStar()
        {
            // calculate shortest path via a* algorithm
            shortestPath.Clear();
            if (_targetNodes.Count == 0)
                return;

            if (_valid[_startNode.GridPosition.x, _startNode.GridPosition.y] == false)
                return;

            if (_targetNodes.Contains(_startNode))
                return;
            calculating = true;

            var path = Graph.AStarSearch(_startNode, _targetNodes.First(), (cell1, cell2) =>
            {
                if (!_valid[cell2.GridPosition.x, cell2.GridPosition.y])
                    return float.MaxValue;
                return Vector2Int.Distance(cell1.GridPosition, cell2.GridPosition);
            });

            shortestPath[_targetNodes.First()] = path;
            calculating = false;
        }

        private void ShortestPathDijkstra()
        {
            shortestPath.Clear();
            if (_targetNodes.Count == 0)
                return;

            if (_valid[_startNode.GridPosition.x, _startNode.GridPosition.y] == false)
                return;

            if (_targetNodes.Contains(_startNode))
                return;

            Debug.Log("calculate");


            var path = Graph.DijkstraSearch(_startNode, _targetNodes.ToArray(), (cell1, cell2) =>
            {
                if (!_valid[cell2.GridPosition.x, cell2.GridPosition.y])
                    return float.MaxValue;
                return Vector2Int.Distance(cell1.GridPosition, cell2.GridPosition);
            });

            for (int i = 0; i < _targetNodes.Count; i++)
            {
                shortestPath[_targetNodes[i]] = path[i];
            }
            calculating = false;
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


                    color = _valid[x, y] ? color : Color.black;

                    // if (shortestPath.Count > 0)
                    // {
                    //     color = shortestPath[_targetNodes.First()].Contains(cell) ? Color.blue : color;
                    // }


                    color = _targetNodes.Contains(cell) ? Color.red : color;


                    _gridDrawer2D.UpdateColor(x, y, color);
                }
            }

            foreach (var shortestPathValue in shortestPath.Values)
            {
                foreach (var cell in shortestPathValue)
                {
                    if (_targetNodes.Contains(cell))
                    {
                        continue;
                    }

                    _gridDrawer2D.UpdateColor(cell.GridPosition.x, cell.GridPosition.y,
                        Color.Lerp(Color.blue, Color.cyan,
                            1f * shortestPath.Values.ToList().IndexOf(shortestPathValue) / shortestPathValue.Count));
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