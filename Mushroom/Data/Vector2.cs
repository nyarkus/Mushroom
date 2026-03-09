using System;
using Mushroom.Data;

namespace Godot;

public static class Vector2Extensions
{
    public static Vector2I GetNeighborPosition(this Vector2I vector, Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return new Vector2I(vector.X, vector.Y - 1);
            case Direction.Down:
                return new Vector2I(vector.X, vector.Y + 1);
            case Direction.Left:
                return new Vector2I(vector.X - 1, vector.Y);
            case Direction.Right:
                return new Vector2I(vector.X + 1, vector.Y);
        }

        return vector;
    }
}