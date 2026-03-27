using System;
using Mushroom.Data;
using Godot;

namespace Mushroom.Ceils;

public class Cap : CellBase
{
    public float Water { get; set; } = 1f;
    public float Energy { get; set; } = 1f;
    private int _dies = 0;
    private int _sporeTicks = 0;
    private const int _targetSporeTicks = 500;
    
    public Vector2I StalkVector2 { get; set; }
    public float MaxDistance { get; set; } = new Vector2I(1, 0).DistanceSquaredTo(new Vector2I(2, 0));

    public override Action? Do(Vector2I vector2)
    {
        float nextEnergy = Energy;
        float nextWater = Water;
        int nextDies = _dies;
        int nextSporeTicks = _sporeTicks;
        
        if (Grid.Get(StalkVector2) is not Stalk)
        {
            nextDies += 1; 
            nextEnergy = 0;
            nextWater = 0;
        }
        
        if (nextEnergy >= 0.00005f) nextEnergy -= 0.00005f; else nextDies++;
        if (nextWater >= 0.00005f) nextWater -= 0.00005f; else nextDies++;
        
        if (nextDies > 0 && nextEnergy > 0.02f && nextWater > 0.02f)
        {
            nextEnergy -= 0.01f;
            nextWater -= 0.01f;
            nextDies--;
        }
        
        if (nextDies > 10)
            return () => Grid.Set(vector2, new RottingMatter() { BecomeAir = true });
        
        var directions = new[] { Direction.Up, Direction.Left, Direction.Right };
        foreach (var dir in directions)
        {
            var neighborPos = vector2.GetNeighborPosition(dir); 
            if (StalkVector2.DistanceSquaredTo(neighborPos) < MaxDistance)
            {
                if (Grid.Get(neighborPos) is Air && nextWater > 0.4f && nextEnergy > 0.4f)
                {
                    return () =>
                    {
                        Water = nextWater - 0.2f;
                        Energy = nextEnergy - 0.2f;
                        _dies = nextDies;
                        Grid.Set(neighborPos, new Cap() { StalkVector2 = StalkVector2, MaxDistance = MaxDistance });
                    };
                }
            }
        }

        var up = Grid.GetNeighbor(vector2, Direction.Up);
        var down = Grid.GetNeighbor(vector2, Direction.Down);
        var left = Grid.GetNeighbor(vector2, Direction.Left);
        var right = Grid.GetNeighbor(vector2, Direction.Right);
        
        if (down is Air && left is not Air && right is not Air)
        {
            if (nextWater > 0.2f && nextEnergy > 0.2f)
            {
                nextWater -= 0.05f;
                nextEnergy -= 0.05f;
                nextSporeTicks++;
            }

            if (nextSporeTicks >= _targetSporeTicks)
            {
                return () =>
                {
                    Water = nextWater; Energy = nextEnergy; _dies = nextDies; _sporeTicks = 0;
                    Grid.Set(vector2.X, vector2.Y + 1, new Spore());
                };
            }
        }
        
        var resourceAction = TryGiveResource(up) ?? TryGiveResource(down) ?? TryGiveResource(left) ?? TryGiveResource(right);
        if (resourceAction != null)
        {
            return () => {
                Water = nextWater; Energy = nextEnergy; _dies = nextDies; _sporeTicks = nextSporeTicks;
                resourceAction();
            };
        }
        
        if (nextEnergy != Energy || nextWater != Water || nextDies != _dies || nextSporeTicks != _sporeTicks)
            return () => { Water = nextWater; Energy = nextEnergy; _dies = nextDies; _sporeTicks = nextSporeTicks; };

        return null;
    }
    
    private Action? TryGiveResource(CellBase neighbor)
    {
        if (Energy <= 0.5f && Water <= 0.5f) return null;
        
        if (neighbor is Cap cap && (cap.Energy < 0.5f || cap.Water < 0.5f))
        {
            return () =>
            {
                float waterNeeded = 0.5f - cap.Water;
                float energyNeeded = 0.5f - cap.Energy;
                
                float waterToGive = Math.Max(0, Math.Min(waterNeeded, this.Water));
                float energyToGive = Math.Max(0, Math.Min(energyNeeded, this.Energy));
                
                if (waterToGive > 0)
                {
                    cap.Water += waterToGive;
                    this.Water -= waterToGive;
                }
                if (energyToGive > 0)
                {
                    cap.Energy += energyToGive;
                    this.Energy -= energyToGive;
                }
            };
        }
        return null;
    }

    public override Color GetColor(Vector2I vector2) 
        => GetUiColor();

    public override Color GetUiColor()
        => new Color(0.3f, 0.23f, 0.1f);
}