using System;
using Mushroom.Data;

namespace Mushroom.Ceils;

public class Sand : ICell
{
    public bool Wet { get; set; }
    private int _ticksInAir { get; set; }
    public Action? Do(Position position)
    {
        var down = Grid.GetNeighbor(position, Direction.Down);
        if (down is Air or Water)
            return new Action(() =>
            {
                Grid.Move(position, new Position(position.X, position.Y + 1));
            });
        
        Direction[] directions = [Direction.Up, Direction.Right, Direction.Left];

        foreach (var dir in directions)
        {
            var neighbor = Grid.GetNeighbor(position, dir);
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

    public string GetColor(Position position)
        => Wet ? "#abad3a" : "#dde04c";

    public char Symbol { get; } = '#';
}