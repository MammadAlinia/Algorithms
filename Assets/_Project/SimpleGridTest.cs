using System;
using System.Collections.Generic;
using GraphSystem;
using Grid;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project
{
    public class SimpleGridTest : MonoBehaviour
    {
        [SerializeField] private int width, height;
        [SerializeField] private Vector2 cellSize;

        private Grid.Grid _grid;
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
            _grid = new Grid.Grid(width, height, transform.position, cellSize);
        }

        private void Start()
        {
            _grid = new Grid.Grid(width, height, transform.position, cellSize);

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
            UpdateColors();
        }

        private void UpdateColors()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var cell = _grid.GetCell(x, y);
                    var color = _currentNeighbors.Contains(cell) ? Color.gray : Color.white;

                    color = _startNode.Equals(cell) ? Color.green : color;
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
                if (!_valid[_selectedNode.GridPosition.x, _selectedNode.GridPosition.y] || _selectedNode.Equals(_startNode))
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
    }
}