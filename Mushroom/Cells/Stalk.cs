using System;
using Mushroom.Data;

namespace Mushroom.Ceils;

public class Stalk : ICell
{
    public float Water { get; set; } = 0.6f;
    public float Energy { get; set; } = 0.3f;
    private int dies { get; set; } = 0;

    private Random _rand = new();
    public int TargetY { get; set; } = Grid.GroundLevel - 4;
    public Action? Do(Position position)
    {
        if (Energy > 0.0001f)
            Energy -= 0.0001f;
        else
            dies++;
        if (Water > 0.001f)
            Water -= 0.001f;
        else
            dies++;

        if (dies > 0 && Energy > 0.2f && Water > 0.2f)
        {
            Energy -= 0.1f;
            Water -= 0.1f;
            dies--;
        }

        if (dies > 50)
        {
            var down = Grid.GetNeighbor(position, Direction.Down);
            if (down is Stalk stalkDown)
            {
                stalkDown.Energy += 0.1f;
                stalkDown.Water += 0.1f;
                
                Grid.Set(position, Air.Instance);
            }
            else if (down is Mycelium mycelium)
            {
                mycelium.Energy += 0.1f;
                mycelium.Water += 0.1f;
                
                Grid.Set(position, Air.Instance);
            }
        }
        
        var up = Grid.GetNeighbor(position, Direction.Up);
        if (position.Y > TargetY && Water >= 0.6f && Energy >= 0.3f)
        {
            if (up is Air)
            {
                return new Action(() =>
                {
                    Water -= 0.6f;
                    Energy -= 0.3f;

                    Grid.Set(position.X, position.Y - 1, new Stalk() { TargetY = TargetY });
                });   
            }
        }
        else if (position.Y == TargetY)
        {
            if (up is Cap cap && (cap.Water < 0.5f || cap.Energy < 0.5f))
            {
                return new Action(() =>
                {
                    float waterToGive = 0.5f - cap.Water;
                    cap.Water += waterToGive;
                    Water -= waterToGive;

                    float energyToGive = 0.5f - cap.Energy;
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
                    Grid.Set(position.X, position.Y - 1, new Cap() { StalkPosition = position, 
                        MaxDistance = new Position(1,1).DistanceSquared(new Position(capSize,capSize)) 
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

    public string GetColor(Position position)
    {
        return "#f7f8f9";
    }

    public char Symbol { get; } = '#';
}