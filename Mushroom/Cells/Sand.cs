using System;
using Mushroom.Data;
using Godot;

namespace Mushroom.Ceils;

public class Sand : ICell
{
    public bool Wet { get; set; }
    private int _ticksInAir { get; set; }
    public Action? Do(Vector2I vector2)
    {
        var down = Grid.GetNeighbor(vector2, Direction.Down);
        if (down is Air or Water)
            return new Action(() =>
            {
                Grid.Move(vector2, new Vector2I(vector2.X, vector2.Y + 1));
            });
        
        Direction[] directions = [Direction.Up, Direction.Right, Direction.Left];

        foreach (var dir in directions)
        {
            var neighbor = Grid.GetNeighbor(vector2, dir);
            if(neighbor is Water)
                Wet = true;
            if (neighbor is Air)
                _ticksInAir++;

            if (_ticksInAir >= 10)
            {
                Wet = false;
                _ticksInAir = 0;
            }
        }

        return null;
    }

    public string GetColor(Vector2I vector2)
        => Wet ? "#abad3a" : "#dde04c";

    public char Symbol { get; } = '#';
}