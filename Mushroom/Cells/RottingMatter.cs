using System;
using System.Collections.Generic;
using Godot;
using Mushroom.Data;

namespace Mushroom.Ceils;

public class RottingMatter : ICell
{
    public bool BecomeAir { get; set; }
    private int _age = 0;
    
    public Action? Do(Vector2I pos)
    {
        _age++;
        
        var downPos = pos + new Vector2I(0, 1);
        if (Grid.IsInBounds(downPos) && Grid.Get(downPos) is Air)
        {
            BecomeAir = true;
            return new Action(() => Grid.Move(pos, downPos));
        }
        
        if (_age > 500)
        {
            if (BecomeAir)
            {
                return new Action(() =>
                {
                    if(Grid.Get(downPos) is Dirt dirt)
                        dirt.Nutrients = Math.Clamp(dirt.Nutrients + 0.3f, 0f, 1f);
                    Grid.Set(pos, Air.Instance);
                });
            }
            
            return new Action(() =>
            {
                Grid.Set(pos, new Dirt() { Dampness = 0.5f, Nutrients = 1f });
            });

        }
        
        
        if (Grid.Get(downPos) is not Dirt && _age % 10 == 0)
        {
            List<Vector2I> availableDirs = new();
            if(Grid.GetNeighbor(pos, Direction.Left) is Air)
                availableDirs.Add(pos + new Vector2I(-1, 0));
            if(Grid.GetNeighbor(pos, Direction.Right) is Air)
                availableDirs.Add(pos + new Vector2I(1, 0));

            if (availableDirs.Count == 0)
                return null;

            BecomeAir = true;
            
            return new Action(() => { Grid.Move(pos, availableDirs[GD.RandRange(0, availableDirs.Count - 1)]); });
        }

        return null;
    }

    public string GetColor(Vector2I pos) => "#26221d";

    public char Symbol { get; } = 'H';
}