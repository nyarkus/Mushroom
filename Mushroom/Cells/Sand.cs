using System;
using Mushroom.Data;
using Godot;

namespace Mushroom.Ceils;

public class Sand : CellBase
{
    public bool IsWet { get; set; }
    private int _ticksInAir { get; set; }
    public override Action? Do(Vector2I position)
    {
        int nextTicks = _ticksInAir;
        bool nextWet = IsWet;
        
        var down = Grid.GetNeighbor(position, Direction.Down);
        if (down is Air or Water)
            return () => 
            { 
                _ticksInAir = nextTicks; 
                IsWet = nextWet; 
                Grid.Move(position, new Vector2I(position.X, position.Y + 1)); 
            };
        
        Direction[] directions = [Direction.Up, Direction.Right, Direction.Left];

        foreach (var dir in directions)
        {
            var neighbor = Grid.GetNeighbor(position, dir);
            if(neighbor is Water)
                nextWet = true;
            if (neighbor is Air)
                nextTicks++;

            if (nextTicks >= 10)
            {
                nextWet = false;
                nextTicks = 0;
            }
        }

        return () =>
        {
            _ticksInAir = nextTicks;
            IsWet = nextWet;
        };
    }

    public override Color GetColor(Vector2I position)
        => IsWet ? new Color(0.67f, 0.68f, 0.23f) : new Color(0.86f, 0.87f, 0.3f);

    public override Color GetUiColor()
        => new Color(0.86f, 0.87f, 0.3f);
}