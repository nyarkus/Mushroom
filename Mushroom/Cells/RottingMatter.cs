using System;
using System.Collections.Generic;
using Godot;
using Mushroom.Data;
using Mushroom.Mushroom.Data;

namespace Mushroom.Ceils;

[Spawnable]
public class RottingMatter : CellBase
{
    public bool BecomeAir { get; set; }
    private int _age = 0;
    
    public override Action? Do(Vector2I pos)
    {
        var downPos = pos + new Vector2I(0, 1);
        if (Grid.Get(downPos) is Air)
        {
            _age++;
            BecomeAir = true;
            return () => Grid.Move(pos, downPos);
        }
        
        if (_age > 500)
        {
            if (BecomeAir)
            {
                return () =>
                {
                    var result = Raycast.Cast(pos, new Vector2I(0, 1), 10, [typeof(RottingMatter)]);
                    GD.Print(result.Cell.GetType().FullName);
                    if(result.Cell is Dirt dirt)
                        dirt.Nutrients = Math.Clamp(dirt.Nutrients + 0.3f, 0f, 1f);
                    Grid.Set(pos, Air.Instance);
                };
            }
            
            return () =>
            {
                Grid.Set(pos, new Dirt() { Dampness = 0.5f, Nutrients = 1f });
            };

        }
        
        
        if (Grid.Get(downPos) is not Dirt && _age % 10 == 0)
        {
            List<Vector2I> availableDirs = new();
            if(Grid.GetNeighbor(pos, Direction.Left) is Air)
                availableDirs.Add(pos + new Vector2I(-1, 0));
            if(Grid.GetNeighbor(pos, Direction.Right) is Air)
                availableDirs.Add(pos + new Vector2I(1, 0));

            if (availableDirs.Count == 0)
                return () => _age++;

            
            
            return () =>
            {
                BecomeAir = true;
                _age++;
                Grid.Move(pos, availableDirs[Random.Shared.Next(0, availableDirs.Count)]);
            };
        }

        return () => _age++;
    }

    public override Color GetColor(Vector2I pos) 
        => GetUiColor();

    public override Color GetUiColor()
        => new Color(0.15f, 0.13f, 0.11f);
}