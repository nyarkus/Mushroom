using System.Collections.Generic;
using Godot;
using Mushroom.Ceils;
using Mushroom.Data;
using Mushroom.Generation;

namespace Mushroom;

public static class Grid
{
    private static Dictionary<(int x, int y), ICell> _ceils = new();

    public static void Set(Vector2I vector2, ICell cell)
        => _ceils[(vector2.X, vector2.Y)] = cell;
    public static void Set(int x, int y, ICell cell)
        => _ceils[(x, y)] = cell;

    public static ICell Get(Vector2I vector2)
        => Get(vector2.X, vector2.Y);
    public static ICell Get(int x, int y)
    {
        try
        {
            return _ceils[(x, y)];
        }
        catch
        {
            return Air.Instance;
        }
    }

    public static void Move(Vector2I from, Vector2I to)
    {
        var target = Get(from.X, from.Y);
        Set(from, Get(to.X, to.Y));
        Set(to, target);
    }

    public static ICell GetNeighbor(Vector2I vector2, Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return Get(new(vector2.X, vector2.Y - 1));
            case Direction.Down:
                return Get(new(vector2.X, vector2.Y + 1));
            case Direction.Left:
                return Get(new(vector2.X - 1, vector2.Y));
            case Direction.Right:
                return Get(new(vector2.X + 1, vector2.Y));
        }
        
        return Air.Instance;
    }

    public static bool IsInBounds(Vector2I vector2)
        => vector2.X >= 0 && vector2.X < Size.X && vector2.Y >= 0 && vector2.Y < Size.Y;
    
    public static Vector2I Size { get; internal set; }
    public static int GroundLevel { get; internal set; }
}