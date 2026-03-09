using System;
using Mushroom.Data;
using Godot;

namespace Mushroom.Ceils;

public class Stalk : ICell
{
    public float Water { get; set; } = 0.6f;
    public float Energy { get; set; } = 0.3f;
    private int dies { get; set; } = 0;
    public int TargetY { get; set; } = Grid.GroundLevel - 4;
    
    public Action? Do(Vector2I vector2)
    {
        float nextWater = Water;
        float nextEnergy = Energy;
        int nextDies = dies;
        
        var down = Grid.GetNeighbor(vector2, Direction.Down);
        if (down is not Stalk && down is not Mycelium)
        {
            nextDies += 1; 
            nextEnergy = 0;
            nextWater = 0;
        }

        if (nextEnergy >= 0.0001f)
            nextEnergy -= 0.0001f;
        else
            nextDies++;
        if (nextWater >= 0.0001f)
            nextWater -= 0.0001f;
        else
            nextDies++;

        if (nextDies > 0 && nextEnergy > 0.02f && nextWater > 0.02f)
        {
            nextEnergy -= 0.01f;
            nextWater -= 0.01f;
            nextDies--;
        }

        if (nextDies > 20)
        {
            if (down is Stalk stalkDown)
            {
                return () =>
                {
                    stalkDown.Energy += Math.Max(0, nextEnergy);
                    stalkDown.Water += Math.Max(0, nextWater);
                    Grid.Set(vector2, new RottingMatter() { BecomeAir = true } );
                };
            }
            else if (down is Mycelium mycelium)
            {
                return () =>
                {
                    mycelium.Energy += Math.Max(0, nextEnergy);
                    mycelium.Water += Math.Max(0, nextWater);
                    Grid.Set(vector2, new RottingMatter() { BecomeAir = true });
                };
            }
            else
                return () =>
                {
                    Grid.Set(vector2, new RottingMatter() { BecomeAir = true });
                };
        }
        
        var up = Grid.GetNeighbor(vector2, Direction.Up);
        if (vector2.Y > TargetY && nextWater >= 0.6f && nextEnergy >= 0.3f)
        {
            if (up is Air)
            {
                nextWater -= 0.6f;
                nextEnergy -= 0.3f;
                
                return () =>
                {
                    Water = nextWater;
                    Energy = nextEnergy;
                    
                    Grid.Set(vector2.X, vector2.Y - 1, new Stalk() { TargetY = TargetY });
                };   
            }
        }
        else if (vector2.Y == TargetY)
        {
            if (up is Cap cap && (cap.Water < 0.5f || cap.Energy < 0.5f))
            {
                float waterToGive = Math.Min(0.5f - cap.Water, Math.Max(0, Water));
                float energyToGive = Math.Min(0.5f - cap.Energy, Math.Max(0, Energy));
                nextWater -= waterToGive;
                nextEnergy -= energyToGive;
                
                return () =>
                {
                    Water = nextWater;
                    Energy = nextEnergy;
                    
                    cap.Water += waterToGive;
                    cap.Energy += energyToGive;
                };
            }
            if (up is Air && Energy > 0.5f && Water > 0.5f)
            {
                var capSize = Random.Shared.Next(3, 5);
                nextWater -= 0.4f;
                nextEnergy -= 0.4f;
                
                return () =>
                {
                    Water = nextWater;
                    Energy = nextEnergy;
                    
                    Grid.Set(vector2.X, vector2.Y - 1, new Cap() { 
                        StalkVector2 = vector2, 
                        MaxDistance = new Vector2I(1,0).DistanceSquaredTo(new Vector2I(capSize,0)),
                        Water = 0.4f,
                        Energy = 0.4f
                    });
                };
            }
        }

        if (up is Stalk stalk && Water > 0.2f && Energy > 0.2f)
        {
            nextWater -= 0.1f;
            nextEnergy -= 0.1f;
            return () =>
            {
                Water = nextWater;
                Energy = nextEnergy;
                
                stalk.Water += 0.1f;
                stalk.Energy += 0.1f;
            };
        }

        return () =>
        {
            Water = nextWater;
            Energy = nextEnergy;
        };
    }

    public Color GetColor(Vector2I vector2) => new Color(0.97f, 0.97f, 0.97f);
    public char Symbol { get; } = '#';
}