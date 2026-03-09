using System.Collections.Generic;
using Godot;
using Mushroom.Ceils;
using Mushroom.Data;

namespace Mushroom;

public static class Grid
{
    private static CellBase[] _cells;
    public static Vector2I Size { get; private set; }
    public static int GroundLevel { get; internal set; }

    public static void Initialize(Vector2I size)
    {
        Size = size;
        _cells = new CellBase[size.X * size.Y];
        for (int i = 0; i < _cells.Length; i++)
            _cells[i] = Air.Instance;
    }

    public static void Set(Vector2I vector2, CellBase cell) => Set(vector2.X, vector2.Y, cell);
    
    public static void Set(int x, int y, CellBase cell)
    {
        if (IsInBounds(x, y))
            _cells[y * Size.X + x] = cell;
    }

    public static CellBase Get(Vector2I vector2) => Get(vector2.X, vector2.Y);
    
    public static CellBase Get(int x, int y)
    {
        if (IsInBounds(x, y))
            return _cells[y * Size.X + x] ?? Air.Instance;
        return Air.Instance;
    }

    public static void Move(Vector2I from, Vector2I to)
    {
        var target = Get(from.X, from.Y);
        Set(from, Get(to.X, to.Y));
        Set(to, target);
    }

    public static CellBase GetNeighbor(Vector2I vector2, Direction direction)
    {
        return direction switch
        {
            Direction.Up => Get(vector2.X, vector2.Y - 1),
            Direction.Down => Get(vector2.X, vector2.Y + 1),
            Direction.Left => Get(vector2.X - 1, vector2.Y),
            Direction.Right => Get(vector2.X + 1, vector2.Y),
            _ => Air.Instance
        };
    }

    public static bool IsInBounds(Vector2I vector2) => IsInBounds(vector2.X, vector2.Y);
    public static bool IsInBounds(int x, int y) => x >= 0 && x < Size.X && y >= 0 && y < Size.Y;
}