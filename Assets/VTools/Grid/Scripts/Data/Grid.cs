using UnityEngine;
using System.Collections.Generic;
using VTools.Utility;

namespace VTools.Grid
{
    public class Grid
    {
        private readonly Cell[,] _gridArray;       
        private readonly List<Cell> _cells;

        public Vector3 OriginPosition { get; }
        public float CellSize { get; }
        public int Width { get; }
        public int Lenght { get; }
        public IReadOnlyList<Cell> Cells => _cells;

        // ------------------------------------------------------------------------- CONSTRUCTOR -------------------------------------------------------------------------
        public Grid(int width, int lenght, float cellSize, Vector3 originPosition, bool showDebug)
        {
            Width = width;
            Lenght = lenght;
            CellSize = cellSize;
            OriginPosition = originPosition;

            _gridArray = new Cell[width, lenght];
            _cells = new List<Cell>();

            for (int x = 0; x < _gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < _gridArray.GetLength(1); y++)
                {
                    //Create a new cell and add it to cell list
                    Cell cell = new Cell(x, y, cellSize);
                    _cells.Add(cell);
                    _gridArray[x, y] = cell;
                }
            }

            if (showDebug)
            {
                DrawGridDebug();
            }
        }

        // ------------------------------------------------------------------------- GRID INFOS -------------------------------------------------------------------------

        public Vector3 GetWorldPosition(int x, int y)
        {
            return new Vector3(x, 0, y) * CellSize + OriginPosition;
        }
        
        public Vector3 GetWorldPosition(Vector2Int coordinates)
        {
            return new Vector3(coordinates.x, 0, coordinates.y) * CellSize + OriginPosition;
        }

        private void GetCellCoordinates(Vector3 worldPosition, out int x, out int y)
        {
            x = Mathf.FloorToInt((worldPosition - OriginPosition).x / CellSize);
            y = Mathf.FloorToInt((worldPosition - OriginPosition).z / CellSize);
        }
        
        // ------------------------------------------------------------------------- CELLS -------------------------------------------------------------------------
        /// <summary>
        /// From a set of coordinates return a cell if one is found.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="foundCell">The cell potentially found on the coordinates.</param>
        /// <returns>True if a cell is found, otherwise false.</returns>
        public bool TryGetCellByCoordinates(int x, int y, out Cell foundCell)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Lenght)
            {
                foundCell = _gridArray[x, y];
                return true;
            }

            foundCell = null;
            return false;
        }

        public bool TryGetCellByCoordinates(Vector2Int coordinates, out Cell foundCell)
        {
            var cellFound = TryGetCellByCoordinates(coordinates.x, coordinates.y, out Cell cell);
            foundCell = cell;

            return cellFound;
        }

        /// <summary>
        /// From a world position return a cell if one is found.
        /// </summary>
        /// <param name="worldPosition">The position you want to test.</param>
        /// <param name="foundCell">The cell potentially found on the world position.</param>
        /// <returns>True if a cell is found, otherwise false.</returns>
        public bool TryGetCellByPosition(Vector3 worldPosition, out Cell foundCell)
        {
            GetCellCoordinates(worldPosition, out var x, out var y);

            if (TryGetCellByCoordinates(x, y, out var cell))
            {
                foundCell = cell;
                return true;
            }

            foundCell = null;
            return false;
        }

		/// <summary>
		/// Retrieves all cells within a circle defined by a center position and radius.
		/// </summary>
		/// <param name="center">Center position in world coordinates.</param>
		/// <param name="radius">Radius of the circle.</param>
		/// <returns>List of cells within the circle.</returns>
		public List<Cell> GetCellsInCircle(Vector3 center, float radius)
		{
			List<Cell> cellsInCircle = new List<Cell>();

			foreach (Cell cell in _cells)
			{
				// Calculate the world position of the center of the cell.
                Vector3 cellWorldPosition = GetWorldPosition(cell.Coordinates.x, cell.Coordinates.y) + new Vector3(CellSize / 2, 0, CellSize / 2);

				// Calculate the distance between the circle center and the cell center.
				float distance = Vector3.Distance(center, cellWorldPosition);

				// Add the cell to the list if it is within the radius.
				if (distance <= radius)
				{
					cellsInCircle.Add(cell);
				}
			}

			return cellsInCircle;
		}
        
        // ------------------------------------------------------------------------- DEBUG -------------------------------------------------------------------------
        public void DrawGridDebug()
        {
            var oldDebug = GameObject.Find("GRID_TEXT_DEBUG");
            if (oldDebug)
            {
                Object.Destroy(oldDebug);
            }
            
            var debugTextArray = new TextMesh[Width][];
            for (int index = 0; index < Width; index++)
            {
                debugTextArray[index] = new TextMesh[Lenght];
            }

            GameObject parent = new GameObject("GRID_TEXT_DEBUG");
            
            for (int x = 0; x < _gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < _gridArray.GetLength(1); y++)
                {
                    if (!TryGetCellByCoordinates(x,y, out var cell))
                    {
                        continue;
                    }

                    var worldPosition = GetWorldPosition(x, y) + new Vector3(CellSize / 2, 0, CellSize / 2);

                    var color = new Color(0.35f, 0.34f, 0.31f);
                    
                    debugTextArray[x][y] = UtilityMethods.CreateWorldText($"({x},{y})", parent.transform, worldPosition, 16, color, TextAnchor.MiddleCenter);
                    debugTextArray[x][y].transform.rotation = Quaternion.Euler(90, 0, 0);
                    
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, float.PositiveInfinity);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, float.PositiveInfinity);
                }
            }

            Debug.DrawLine(GetWorldPosition(0, Lenght), GetWorldPosition(Width, Lenght), Color.white, float.PositiveInfinity);
            Debug.DrawLine(GetWorldPosition(Width, 0), GetWorldPosition(Width, Lenght), Color.white, float.PositiveInfinity);
        }
    }
}