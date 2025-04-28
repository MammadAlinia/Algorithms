using UnityEngine;

namespace _Project
{
    public class GridDrawer2D
    {
        private readonly GameObject[,] _gridCells;

        public GridDrawer2D(Grid.Grid grid)
        {
            _gridCells = new GameObject[grid.Width, grid.Height];

            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    var cell = grid.GetCell(x, y);
                    _gridCells[x, y] = GenerateSprite(cell.WorldPosition, grid.CellSize);
                }
            }
        }

        private GameObject GenerateSprite(Vector3 worldPosition, Vector3 cellSize)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.material.shader = Shader.Find("Universal Render Pipeline/Unlit");
            renderer.material.color = Color.white;
            
            go.transform.position = worldPosition;
            go.transform.localScale = cellSize;

            return go;
        }

        public void UpdateColor(int x, int y, Color color)
        {
            _gridCells[x, y].GetComponent<MeshRenderer>().material.color = color;
        }

        public void SetActive(int x, int y, bool state)
        {
            _gridCells[x, y].SetActive(state);
        }

        public void SetSize(int x, int y, Vector2 gridCellSize)
        {
            _gridCells[x, y].transform.localScale = gridCellSize;
        }

        public void UpdateColor(Vector2Int gridPosition, Color color)
        {
            UpdateColor(gridPosition.x, gridPosition.y, color);
        }
    }
}