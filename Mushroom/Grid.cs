using System.Collections.Generic;
using Mushroom.Ceils;
using Mushroom.Data;
using Mushroom.Generation;

namespace Mushroom;

public static class Grid
{
    private static Dictionary<(int x, int y), ICell> _ceils = new();

    public static void Set(Position position, ICell cell)
        => _ceils[(position.X, position.Y)] = cell;
    public static void Set(int x, int y, ICell cell)
        => _ceils[(x, y)] = cell;

    public static ICell Get(Position position)
        => Get(position.X, position.Y);
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

    public static void Move(Position from, Position to)
    {
        var target = Get(from.X, from.Y);
        Set(from, Get(to.X, to.Y));
        Set(to, target);
    }

    public static ICell GetNeighbor(Position position, Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return Get(new(position.X, position.Y - 1));
            case Direction.Down:
                return Get(new(position.X, position.Y + 1));
            case Direction.Left:
                return Get(new(position.X - 1, position.Y));
            case Direction.Right:
                return Get(new(position.X + 1, position.Y));
        }
        
        return Air.Instance;
    }

    public static bool IsInBounds(Position position)
        => position.X >= 0 || position.X <= Size.X || position.Y <= Size.Y;
    
    public static Position Size { get; internal set; }
    public static int GroundLevel { get; internal set; }
}