using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using VTools.Grid;
using VTools.ScriptableObjectDatabase;
using VTools.Utility;

namespace Components.ProceduralGeneration.SimpleRoomPlacement
{
    [CreateAssetMenu(menuName = "Procedural Generation Method/Simple Room Placement")]
    public class SimpleRoomPlacement : ProceduralGenerationMethod
    {
        [FormerlySerializedAs("_maxRooms")]
        [Header("Room Parameters")]
        [SerializeField] private int maxRooms = 10;
        [SerializeField] private Vector2Int roomMinSize = new Vector2Int(4, 4);
        [SerializeField] private Vector2Int roomMaxSize = new Vector2Int(8, 8);

        [Header("Map Parameters")]
        [SerializeField] private int spacing = 2;

        [NonSerialized] private List<RectInt> _rooms = new();

        protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
        {
            BuildGround();

            for (int i = 0; i < maxRooms; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int w = RandomService.Range(roomMinSize.x, roomMaxSize.x);
                int h = RandomService.Range(roomMinSize.y, roomMaxSize.y);

                int x = RandomService.Range(0, Grid.Width - w);
                int y = RandomService.Range(0, Grid.Lenght - h);

                RectInt newRoom = new RectInt(x, y, w, h);

                if (CanPlaceRoom(newRoom))
                {
                    PlaceRoom(newRoom);
                    _rooms.Add(newRoom);
                }

                await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
            }

            BuildCorridor();
        }

        private bool CanPlaceRoom(RectInt room)
        {
            if (room.xMin < 0 || room.yMin < 0 ||
                room.xMax > Grid.Width || room.yMax > Grid.Lenght)
                return false;

            foreach (RectInt existing in _rooms)
            {
                RectInt expanded = new RectInt(
                    existing.xMin - spacing,
                    existing.yMin - spacing,
                    existing.width + spacing * 2,
                    existing.height + spacing * 2
                );

                if (expanded.Overlaps(room))
                    return false;
            }

            return true;
        }

        private void PlaceRoom(RectInt room)
        {
            for (int x = room.xMin; x < room.xMax; x++)
            {
                for (int y = room.yMin; y < room.yMax; y++)
                {
                    if (!Grid.TryGetCellByCoordinates(x, y, out Cell cell))
                        continue;

                    AddTileToCell(cell, ROOM_TILE_NAME, true);
                }
            }
        }

        private void BuildGround()
        {
            var groundTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Grass");

            for (int x = 0; x < Grid.Width; x++)
            {
                for (int z = 0; z < Grid.Lenght; z++)
                {
                    if (!Grid.TryGetCellByCoordinates(x, z, out var cell))
                        continue;

                    GridGenerator.AddGridObjectToCell(cell, groundTemplate, false);
                }
            }
        }

        private void BuildCorridor()
        {
            if (_rooms.Count < 2)
                return;

            var corridorTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Corridor");

            for (int i = 1; i < _rooms.Count; i++)
            {
                RectInt start = _rooms[i - 1];
                RectInt end = _rooms[i];
                CreateDogLegCorridor(start, end);
            }
        }

        private void CreateDogLegCorridor(RectInt start, RectInt end)
        {
            Vector2Int startCenter = Vector2Int.RoundToInt(start.center);
            Vector2Int endCenter = Vector2Int.RoundToInt(end.center);

            bool horizontalFirst = RandomService.Range(0, 2) == 0;

            if (horizontalFirst)
            {
                CreateStraightCorridor(startCenter.x, endCenter.x, startCenter.y, true);
                CreateStraightCorridor(startCenter.y, endCenter.y, endCenter.x, false);
            }
            else
            {
                CreateStraightCorridor(startCenter.y, endCenter.y, startCenter.x, false);
                CreateStraightCorridor(startCenter.x, endCenter.x, endCenter.y, true);
            }
        }
        
        private void CreateStraightCorridor(int from, int to, int fixedCoord, bool horizontal)
        {
            int start = Mathf.Min(from, to);
            int end = Mathf.Max(from, to);

            for (int i = start; i <= end; i++)
            {
                int x = horizontal ? i : fixedCoord;
                int y = horizontal ? fixedCoord : i;

                if (Grid.TryGetCellByCoordinates(x, y, out var cell))
                {
                    AddTileToCell(cell, CORRIDOR_TILE_NAME, true);
                }
            }
        }
    }
}
