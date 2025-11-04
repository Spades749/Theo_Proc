using System;
using System.Collections.Generic;
using System.Threading;
using Components.ProceduralGeneration;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VTools.Grid;
using VTools.RandomService;
using VTools.ScriptableObjectDatabase;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Procedural Generation Method/BSP Room Placement")]
public class BspRoomPlacement : ProceduralGenerationMethod
{
    [Header("BSP Parameters")]
    [SerializeField] private int numberOfSplits = 4;
    [SerializeField] private Vector2Int roomMinSize = new(4, 4);
    [SerializeField] private Vector2Int roomMaxSize = new(8, 8);
    [NonSerialized] private BspNode _root;

    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        _root = null;
        BuildGround();

        var allGrid = new RectInt(0, 0, Grid.Width, Grid.Lenght);
        _root = new BspNode(allGrid);

        _root.Split(numberOfSplits);

        foreach (var leaf in _root.GetLeaves())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var room = CreateRoomWithin(leaf.Bounds);
            leaf.Room = room;
            PlaceRoom(room);

            await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
        }

        _root.ConnectNodes(this);
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

    private RectInt CreateRoomWithin(RectInt bounds)
    {
        int roomWidth = Mathf.Clamp(
            RandomService.Range(roomMinSize.x, Mathf.Min(bounds.width, roomMaxSize.x)),
            1, bounds.width
        );
        int roomHeight = Mathf.Clamp(
            RandomService.Range(roomMinSize.y, Mathf.Min(bounds.height, roomMaxSize.y)),
            1, bounds.height
        );

        int x = RandomService.Range(bounds.xMin, Mathf.Max(bounds.xMin, bounds.xMax - roomWidth));
        int y = RandomService.Range(bounds.yMin, Mathf.Max(bounds.yMin, bounds.yMax - roomHeight));

        return new RectInt(x, y, roomWidth, roomHeight);
    }

    public void PlaceRoom(RectInt room)
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

    public void CreateCorridor(Vector2Int from, Vector2Int to)
    {
        int startX = Mathf.Min(from.x, to.x);
        int endX = Mathf.Max(from.x, to.x);
        int startY = Mathf.Min(from.y, to.y);
        int endY = Mathf.Max(from.y, to.y);

        bool horizontalFirst = RandomService.Range(0, 2) == 0;

        if (horizontalFirst)
        {
            CreateStraightCorridor(startX, endX, from.y, true);
            CreateStraightCorridor(startY, endY, to.x, false);
        }
        else
        {
            CreateStraightCorridor(startY, endY, from.x, false);
            CreateStraightCorridor(startX, endX, to.y, true);
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

public class BspNode
{
    private RectInt _bounds;
    private BspNode _child1, _child2;

    public RectInt Bounds => _bounds;
    public RectInt Room { get; set; }
    public Vector2Int RoomCenter => Vector2Int.RoundToInt(Room.center);

    public BspNode(RectInt bounds)
    {
        _bounds = bounds;
    }

    public void Split(int depth)
    {
        if (depth <= 0)
            return;

        var containers = SplitContainer(_bounds);
        _child1 = new BspNode(containers[0]);
        _child2 = new BspNode(containers[1]);

        _child1.Split(depth - 1);
        _child2.Split(depth - 1);
    }

    public IEnumerable<BspNode> GetLeaves()
    {
        if (_child1 == null && _child2 == null)
            yield return this;
        else
        {
            if (_child1 != null)
                foreach (var n in _child1.GetLeaves())
                    yield return n;
            if (_child2 != null)
                foreach (var n in _child2.GetLeaves())
                    yield return n;
        }
    }

    public void ConnectNodes(BspRoomPlacement placer)
    {
        if (_child1 != null && _child2 != null)
        {
            _child1.ConnectNodes(placer);
            _child2.ConnectNodes(placer);

            Vector2Int p1 = _child1.GetClosestRoomCenter();
            Vector2Int p2 = _child2.GetClosestRoomCenter();

            placer.CreateCorridor(p1, p2);
        }
    }

    private Vector2Int GetClosestRoomCenter()
    {
        if (_child1 == null && _child2 == null)
            return RoomCenter;

        if (_child1 != null)
            return _child1.GetClosestRoomCenter();
        else
            return _child2.GetClosestRoomCenter();
    }

    private static RectInt[] SplitContainer(RectInt container)
    {
        RectInt c1, c2;
        bool horizontal = Random.Range(0f, 1f) > 0.5f;

        if (horizontal)
        {
            int splitWidth = Mathf.RoundToInt(container.width * Random.Range(0.4f, 0.6f));
            c1 = new RectInt(container.x, container.y, splitWidth, container.height);
            c2 = new RectInt(container.x + splitWidth, container.y, container.width - splitWidth, container.height);
        }
        else
        {
            int splitHeight = Mathf.RoundToInt(container.height * Random.Range(0.4f, 0.6f));
            c1 = new RectInt(container.x, container.y, container.width, splitHeight);
            c2 = new RectInt(container.x, container.y + splitHeight, container.width, container.height - splitHeight);
        }

        return new[] { c1, c2 };
    }
}


