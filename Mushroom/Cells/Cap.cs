using System;
using Mushroom.Data;
using Godot;

namespace Mushroom.Ceils;

public class Cap : ICell
{
    public float Water { get; set; } = 1f;
    public float Energy { get; set; } = 1f;
    private int dies = 0;
    
    private int _sporeTicks = 0;
    private const int _targetSporeTicks = 500;
    
    public Vector2I StalkVector2 { get; set; }
    public float MaxDistance { get; set; } = new Vector2I(1, 0).DistanceSquaredTo(new Vector2I(2, 0));

    public Action? Do(Vector2I vector2)
    {
        if (Grid.Get(StalkVector2) is not Stalk)
        {
            dies += 1; 
            Energy = 0;
            Water = 0;
        }
        
        if (Energy >= 0.00005f)
            Energy -= 0.00005f;
        else
            dies++;
            
        if (Water >= 0.00005f)
            Water -= 0.00005f;
        else
            dies++;
        
        if (dies > 0 && Energy > 0.02f && Water > 0.02f)
        {
            Energy -= 0.01f;
            Water -= 0.01f;
            dies--;
        }
        
        if (dies > 10)
        {
            return new Action(() =>
            {
                Grid.Set(vector2, new RottingMatter() { BecomeAir = true } );
            });
        }
        
        var directions = new[] { Direction.Up, Direction.Left, Direction.Right };
        foreach (var dir in directions)
        {
            var neighborPos = vector2.GetNeighborPosition(dir); 
            if (StalkVector2.DistanceSquaredTo(neighborPos) < MaxDistance)
            {
                if (Grid.Get(neighborPos) is Air && Water > 0.4f && Energy > 0.4f)
                {
                    return new Action(() =>
                    {
                        Water -= 0.2f;
                        Energy -= 0.2f;
                        Grid.Set(neighborPos, new Cap() { StalkVector2 = StalkVector2, MaxDistance = MaxDistance });
                    });
                }
            }
        }

        var up = Grid.GetNeighbor(vector2, Direction.Up);
        var down = Grid.GetNeighbor(vector2, Direction.Down);
        var left = Grid.GetNeighbor(vector2, Direction.Left);
        var right = Grid.GetNeighbor(vector2, Direction.Right);
        
        if (down is Air && left is not Air && right is not Air)
        {
            if (Water > 0.2f && Energy > 0.2f)
            {
                Water -= 0.05f;
                Energy -= 0.05f;
                _sporeTicks++;
            }

            if (_sporeTicks >= _targetSporeTicks)
            {
                Grid.Set(vector2.X, vector2.Y + 1, new Spore());
                _sporeTicks = 0;
            }
        }
        
        var actionUp = TryGiveResource(up);
        if (actionUp != null) return actionUp;

        var actionDown = TryGiveResource(down);
        if (actionDown != null) return actionDown;

        var actionLeft = TryGiveResource(left);
        if (actionLeft != null) return actionLeft;
        
        var actionRight = TryGiveResource(right);
        if (actionRight != null) return actionRight;

        return null;
    }
    
    private Action? TryGiveResource(ICell neighbor)
    {
        if (Energy <= 0.5f && Water <= 0.5f)
            return null;
        
        if (neighbor is Cap cap && (cap.Energy < 0.5f || cap.Water < 0.5f))
        {
            float waterNeeded = 0.5f - cap.Water;
            float energyNeeded = 0.5f - cap.Energy;
            
            float waterToGive = Math.Max(0, Math.Min(waterNeeded, this.Water));
            float energyToGive = Math.Max(0, Math.Min(energyNeeded, this.Energy));
            
            if (waterToGive <= 0 && energyToGive <= 0) 
                return null;

            return new Action(() =>
            {
                cap.Water += waterToGive;
                this.Water -= waterToGive;

                cap.Energy += energyToGive;
                this.Energy -= energyToGive;
            });
        }
        return null;
    }

    public Color GetColor(Vector2I vector2)
        => new Color(0.3f, 0.23f, 0.1f);

    public char Symbol { get; } = '#';
}