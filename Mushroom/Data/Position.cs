using System;

namespace Mushroom.Data;

public struct Position(int x, int y)
{
    public int X = x;
    public int Y = y;

    public static implicit operator (int x, int y)(Position pos)
        => (pos.X, pos.Y);

    public static implicit operator Position((int x, int y) tuple) 
        => new Position(tuple.x, tuple.y);

    public float DistanceSquared(Position position)
    {
        int dx = X - position.X;
        int dy = Y - position.Y;
        
        return dx * dx + dy * dy;
    }
    
    public float Distance(Position position)
    {
        int dx = X - position.X;
        int dy = Y - position.Y;
        
        return (float)Math.Sqrt(dx * dx + dy * dy);
    }

    public Position GetNeighborPosition(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return new Position(X, Y - 1);
            case Direction.Down:
                return new Position(X, Y + 1);
            case Direction.Left:
                return new Position(X - 1, Y);
            case Direction.Right:
                return new Position(X + 1, Y);
        }

        return this;
    }
}