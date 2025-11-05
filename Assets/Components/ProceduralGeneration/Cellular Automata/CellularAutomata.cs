using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VTools.Grid;
using VTools.ScriptableObjectDatabase;

namespace Components.ProceduralGeneration.CellularAutomata
{
    [CreateAssetMenu(menuName = "Procedural Generation Method/Cellular Automata")]
    public class CellularAutomata : ProceduralGenerationMethod
    {
        [Header("Initialization Parameters")]
        [SerializeField, Range(0f, 1f)] private float initialFillChance = 0.45f;
        [SerializeField] private bool useBorderWalls = true;

        [Header("Automata Rules")]
        [SerializeField, Range(0, 8)] private int birthThreshold = 4;
        [SerializeField, Range(0, 8)] private int deathThreshold = 3;

        [Header("Tiles")]
        [SerializeField] private string aliveTileName = GRASS_TILE_NAME;
        [SerializeField] private string deadTileName = ROCK_TILE_NAME;

        [NonSerialized] private bool[,] _currentState;
        [NonSerialized] private bool[,] _nextState;

        protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
        {
            // Initialize the grid state
            InitializeGrid();
            await VisualizeGrid(cancellationToken);

            // Run cellular automata simulation
            for (int step = 0; step < _maxSteps; step++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                SimulateStep();
                await VisualizeGrid(cancellationToken);

                await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
            }

            // Optional: post-processing
            await PostProcess(cancellationToken);
        }

        private void InitializeGrid()
        {
            _currentState = new bool[Grid.Width, Grid.Lenght];
            _nextState = new bool[Grid.Width, Grid.Lenght];

            for (int x = 0; x < Grid.Width; x++)
            {
                for (int y = 0; y < Grid.Lenght; y++)
                {
                    // Border cells are always walls if enabled
                    if (useBorderWalls && (x == 0 || y == 0 || x == Grid.Width - 1 || y == Grid.Lenght - 1))
                    {
                        _currentState[x, y] = false; // Dead/Wall
                    }
                    else
                    {
                        // Random initialization based on fill chance
                        _currentState[x, y] = RandomService.Chance(initialFillChance);
                    }
                }
            }
        }

        private void SimulateStep()
        {
            for (int x = 0; x < Grid.Width; x++)
            {
                for (int y = 0; y < Grid.Lenght; y++)
                {
                    int aliveNeighbours = CountAliveNeighbours(x, y);
                    bool currentlyAlive = _currentState[x, y];

                    // Apply cellular automata rules
                    if (currentlyAlive)
                    {
                        // Cell survives if it has enough neighbours
                        _nextState[x, y] = aliveNeighbours >= deathThreshold;
                    }
                    else
                    {
                        _nextState[x, y] = aliveNeighbours >= birthThreshold;
                    }

                    if (useBorderWalls && (x == 0 || y == 0 || x == Grid.Width - 1 || y == Grid.Lenght - 1))
                    {
                        _nextState[x, y] = false;
                    }
                }
            }

            bool[,] temp = _currentState;
            _currentState = _nextState;
            _nextState = temp;
        }

        private int CountAliveNeighbours(int x, int y)
        {
            int count = 0;
            
            for (int offsetX = -1; offsetX <= 1; offsetX++)
            {
                for (int offsetY = -1; offsetY <= 1; offsetY++)
                {
                    if (offsetX == 0 && offsetY == 0)
                        continue;

                    int nx = x + offsetX;
                    int ny = y + offsetY;
                    
                    if (nx < 0 || ny < 0 || nx >= Grid.Width || ny >= Grid.Lenght)
                    {
                        count++;
                        continue;
                    }

                    if (_currentState[nx, ny])
                        count++;
                }
            }

            return count;
        }

        private async UniTask VisualizeGrid(CancellationToken cancellationToken)
        {
            for (int x = 0; x < Grid.Width; x++)
            {
                for (int y = 0; y < Grid.Lenght; y++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (!Grid.TryGetCellByCoordinates(x, y, out Cell cell))
                        continue;

                    string tileName = _currentState[x, y] ? aliveTileName : deadTileName;
                    AddTileToCell(cell, tileName, true);
                }
            }

            await UniTask.Yield();
        }

        private async UniTask PostProcess(CancellationToken cancellationToken)
        {
            await UniTask.Yield();
            cancellationToken.ThrowIfCancellationRequested();
        }
        
        private List<List<Vector2Int>> FindRegions(bool targetState)
        {
            List<List<Vector2Int>> regions = new List<List<Vector2Int>>();
            bool[,] visited = new bool[Grid.Width, Grid.Lenght];

            for (int x = 0; x < Grid.Width; x++)
            {
                for (int y = 0; y < Grid.Lenght; y++)
                {
                    if (!visited[x, y] && _currentState[x, y] == targetState)
                    {
                        List<Vector2Int> region = FloodFill(x, y, targetState, visited);
                        regions.Add(region);
                    }
                }
            }

            return regions;
        }

        private List<Vector2Int> FloodFill(int startX, int startY, bool targetState, bool[,] visited)
        {
            List<Vector2Int> region = new List<Vector2Int>();
            Queue<Vector2Int> queue = new Queue<Vector2Int>();

            queue.Enqueue(new Vector2Int(startX, startY));
            visited[startX, startY] = true;

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                region.Add(current);

                Vector2Int[] directions = new Vector2Int[]
                {
                    new Vector2Int(1, 0), new Vector2Int(-1, 0),
                    new Vector2Int(0, 1), new Vector2Int(0, -1)
                };

                foreach (Vector2Int dir in directions)
                {
                    int nx = current.x + dir.x;
                    int ny = current.y + dir.y;

                    if (nx >= 0 && ny >= 0 && nx < Grid.Width && ny < Grid.Lenght &&
                        !visited[nx, ny] && _currentState[nx, ny] == targetState)
                    {
                        visited[nx, ny] = true;
                        queue.Enqueue(new Vector2Int(nx, ny));
                    }
                }
            }

            return region;
        }
    }
}