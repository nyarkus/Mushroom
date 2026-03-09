using System;
using Mushroom.Data;
using Godot;

namespace Mushroom.Ceils;

public class Sand : ICell
{
    public bool IsWet { get; set; }
    private int _ticksInAir { get; set; }
    public Action? Do(Vector2I vector2)
    {
        int nextTicks = _ticksInAir;
        bool nextWet = IsWet;
        
        var down = Grid.GetNeighbor(vector2, Direction.Down);
        if (down is Air or Water)
            return () => 
            { 
                _ticksInAir = nextTicks; 
                IsWet = nextWet; 
                Grid.Move(vector2, new Vector2I(vector2.X, vector2.Y + 1)); 
            };
        
        Direction[] directions = [Direction.Up, Direction.Right, Direction.Left];

        foreach (var dir in directions)
        {
            var neighbor = Grid.GetNeighbor(vector2, dir);
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

    public Color GetColor(Vector2I vector2)
        => IsWet ? new Color(0.67f, 0.68f, 0.23f) : new Color(0.86f, 0.87f, 0.3f);

    public char Symbol { get; } = '#';
}