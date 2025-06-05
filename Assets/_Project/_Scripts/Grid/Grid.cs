using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace GridSystem
{
    public class Grid
    {
        public readonly GridCell[,] Cells;
        public Vector2 Origin { get; private set; }
        public readonly int Width;
        public readonly int Height;

        public readonly Vector2 CellSize;

        public readonly List<GridCell> CellList;

        public Grid(int width, int height, Vector3 origin, Vector2 cellSize)
        {
            Width = width;
            Height = height;
            CellSize = cellSize;
            Origin = origin;

            CellList = new List<GridCell>();
            Cells = new GridCell[Width, Height];
            var offset = new Vector2((int)(Width / 2f), (int)(Height / 2f)) * CellSize;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var gridPos = new Vector2(x * cellSize.x, y * cellSize.y) + new Vector2(origin.x, origin.y);


                    Cells[x, y].GridPosition = new Vector2Int(x, y);
                    Cells[x, y].WorldPosition = gridPos - offset;
                    CellList.Add(Cells[x, y]);
                }
            }
        }

        public Grid(List<GridCell> cellList)
        {
            CellList = cellList;
        }

        public ref GridCell WorldToCell(Vector3 position)
        {
            var gridPos = WorldToGrid(position);
            return ref Cells[gridPos.x, gridPos.y];
        }

        public Vector2Int WorldToGrid(Vector2 worldPosition)
        {
            var relativeDistance = (worldPosition - Origin) + (CellSize * .5f);
            var x = Mathf.FloorToInt(relativeDistance.x / CellSize.x);
            var y = Mathf.FloorToInt(relativeDistance.y / CellSize.y);

            return new Vector2Int(
                Math.Clamp(x + (Width / 2), 0, Width - 1),
                Math.Clamp(y + (Height / 2), 0, Height - 1)
            );
        }

        public void UpdateOrigin(Vector3 newOrigin)
        {
            Origin = newOrigin;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Cells[x, y].GridPosition = new Vector2Int(x, y);
                    var xPos = (x * CellSize.x + Origin.x);
                    var yPos = (y * CellSize.y + Origin.y);
                    Cells[x, y].WorldPosition =
                        new Vector3(
                            xPos - (CellSize.x * Width / 2f),
                            yPos - (CellSize.y * Height / 2f),
                            0);
                }
            }
        }

        public ref GridCell GetCell(int x, int y) => ref Cells[x, y];

        public List<GridCell> GetNeighbors(GridCell selectedNode)
        {
            var p = new HashSet<GridCell>() { selectedNode };
            var gridPos = selectedNode.GridPosition;
            var defaultNeighbors = new HashSet<GridCell>()
            {
                GetCell(gridPos + Vector2Int.up),
                GetCell(gridPos + Vector2Int.left),
                GetCell(gridPos + Vector2Int.right),
                GetCell(gridPos + Vector2Int.down),
                GetCell(gridPos + Vector2Int.up + Vector2Int.left),
                GetCell(gridPos + Vector2Int.up + Vector2Int.right),
                GetCell(gridPos + Vector2Int.down + Vector2Int.left),
                GetCell(gridPos + Vector2Int.down + Vector2Int.right),
            };
            defaultNeighbors.ExceptWith(new[] { selectedNode });
            return defaultNeighbors.ToList();
        }

        private GridCell GetCell(Vector2Int gridPos)
        {
            var clampedX = Math.Clamp(gridPos.x, 0, Width - 1);
            var clampedY = Math.Clamp(gridPos.y, 0, Height - 1);
            return GetCell(clampedX, clampedY);
        }
    }

    public struct GridCell : IEquatable<GridCell>
    {
        public Vector3 WorldPosition;
        public Vector2Int GridPosition;

        public bool Equals(GridCell other)
        {
            return GridPosition.Equals(other.GridPosition);
        }

        public override bool Equals(object obj)
        {
            return obj is GridCell other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(WorldPosition, GridPosition);
        }

        public static bool operator ==(GridCell left, GridCell right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GridCell left, GridCell right)
        {
            return !left.Equals(right);
        }
    }
}