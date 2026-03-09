using System;
using Mushroom.Data;
using Godot;

namespace Mushroom.Ceils;

public class Stalk : ICell
{
    public float Water { get; set; } = 0.6f;
    public float Energy { get; set; } = 0.3f;
    private int dies { get; set; } = 0;

    private Random _rand = new();
    public int TargetY { get; set; } = Grid.GroundLevel - 4;
    
    public Action? Do(Vector2I vector2)
    {
        var down = Grid.GetNeighbor(vector2, Direction.Down);
        if (down is not Stalk && down is not Mycelium)
        {
            dies += 1; 
            Energy = 0;
            Water = 0;
        }

        if (Energy >= 0.0001f)
            Energy -= 0.0001f;
        else
            dies++;
        if (Water >= 0.0001f)
            Water -= 0.0001f;
        else
            dies++;

        if (dies > 0 && Energy > 0.02f && Water > 0.02f)
        {
            Energy -= 0.01f;
            Water -= 0.01f;
            dies--;
        }

        if (dies > 20)
        {
            if (down is Stalk stalkDown)
            {
                stalkDown.Energy += Math.Max(0, Energy);
                stalkDown.Water += Math.Max(0, Water);
                Grid.Set(vector2, new RottingMatter() { BecomeAir = true } );
            }
            else if (down is Mycelium mycelium)
            {
                mycelium.Energy += Math.Max(0, Energy);
                mycelium.Water += Math.Max(0, Water);
                Grid.Set(vector2, new RottingMatter() { BecomeAir = true } );
            }
            else 
                Grid.Set(vector2, new RottingMatter() { BecomeAir = true } );
        }
        
        var up = Grid.GetNeighbor(vector2, Direction.Up);
        if (vector2.Y > TargetY && Water >= 0.6f && Energy >= 0.3f)
        {
            if (up is Air)
            {
                return new Action(() =>
                {
                    Water -= 0.6f;
                    Energy -= 0.3f;
                    
                    Grid.Set(vector2.X, vector2.Y - 1, new Stalk() { TargetY = TargetY });
                });   
            }
        }
        else if (vector2.Y == TargetY)
        {
            if (up is Cap cap && (cap.Water < 0.5f || cap.Energy < 0.5f))
            {
                return new Action(() =>
                {
                    float waterToGive = Math.Min(0.5f - cap.Water, Math.Max(0, Water));
                    cap.Water += waterToGive;
                    Water -= waterToGive;

                    float energyToGive = Math.Min(0.5f - cap.Energy, Math.Max(0, Energy));
                    cap.Energy += energyToGive;
                    Energy -= energyToGive;
                });
            }
            if (up is Air && Energy > 0.5f && Water > 0.5f)
            {
                var capSize = _rand.Next(3, 6);
                return new Action(() =>
                {
                    Water -= 0.4f;
                    Energy -= 0.4f;
                    Grid.Set(vector2.X, vector2.Y - 1, new Cap() { 
                        StalkVector2 = vector2, 
                        MaxDistance = new Vector2I(1,0).DistanceSquaredTo(new Vector2I(capSize,0)),
                        Water = 0.4f,
                        Energy = 0.4f
                    });
                });
            }
        }

        if (up is Stalk stalk && Water > 0.2f && Energy > 0.2f)
        {
            return new Action(() =>
            {
                stalk.Water += 0.1f;
                stalk.Energy += 0.1f;
                
                Water -= 0.1f;
                Energy -= 0.1f;
            });
        }

        return null;
    }

    public Color GetColor(Vector2I vector2) => new Color(0.97f, 0.97f, 0.97f);
    public char Symbol { get; } = '#';
}