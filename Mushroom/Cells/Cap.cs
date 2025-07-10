using System;
using Mushroom.Data;

namespace Mushroom.Ceils;

public class Cap : ICell
{
    public float Water { get; set; } = 1f;
    public float Energy { get; set; } = 1f;
    private int dies = 0;
    
    private int _sporeTicks = 0;
    private const int _targetSporeTicks = 500;
    
    public Position StalkPosition { get; set; }
    public float MaxDistance { get; set; } = new Position(1, 1).DistanceSquared(new Position(3, 3));
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
        
        var directions = new[] { Direction.Up, Direction.Left, Direction.Right };
        foreach (var dir in directions)
        {
            var neighborPos = position.GetNeighborPosition(dir); 
            if (StalkPosition.DistanceSquared(neighborPos) < MaxDistance)
            {
                if (Grid.Get(neighborPos) is Air && Water > 0.2f && Energy > 0.2f)
                {
                    return new Action(() =>
                    {
                        Water -= 0.1f;
                        Energy -= 0.1f;

                        Grid.Set(neighborPos, new Cap() { StalkPosition = StalkPosition });
                    });
                }
            }
        }

        

        var up = Grid.GetNeighbor(position, Direction.Up);
        var down = Grid.GetNeighbor(position, Direction.Down);
        var left = Grid.GetNeighbor(position, Direction.Left);
        var right = Grid.GetNeighbor(position, Direction.Right);

        if (down is Air && left is not Air && right is not Air)
        {
            if (Water > 0.2f && Energy > 0.2f)
            {
                Water -= 0.1f;
                Energy -= 0.1f;
                _sporeTicks++;
            }

            if (_sporeTicks >= _targetSporeTicks)
            {
                Grid.Set(position.X, position.Y + 1, new Spore());
                _sporeTicks = 0;
            }
        }

        if (up is Cap cap && (cap.Energy < 0.5f || cap.Water < 0.5f))
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
        if (down is Cap capDown && (capDown.Energy < 0.5f || capDown.Water < 0.5f))
        {
            return new Action(() =>
            {
                float waterToGive = 0.5f - capDown.Water;
                capDown.Water += waterToGive;
                Water -= waterToGive;

                float energyToGive = 0.5f - capDown.Energy;
                capDown.Energy += energyToGive;
                Energy -= energyToGive;
            });
        }
        if (left is Cap capLeft && (capLeft.Energy < 0.5f || capLeft.Water < 0.5f))
        {
            return new Action(() =>
            {
                float waterToGive = 0.5f - capLeft.Water;
                capLeft.Water += waterToGive;
                Water -= waterToGive;

                float energyToGive = 0.5f - capLeft.Energy;
                capLeft.Energy += energyToGive;
                Energy -= energyToGive;
            });
        }
        if (right is Cap capRight && (capRight.Energy < 0.5f || capRight.Water < 0.5f))
        {
            return new Action(() =>
            {
                float waterToGive = 0.5f - capRight.Water;
                capRight.Water += waterToGive;
                Water -= waterToGive;

                float energyToGive = 0.5f - capRight.Energy;
                capRight.Energy += energyToGive;
                Energy -= energyToGive;
            });
        }

        return null;
    }

    public string GetColor(Position position)
    {
        if (Grid.GetNeighbor(position, Direction.Down) is Air && Grid.GetNeighbor(position, Direction.Left) is not Air && Grid.GetNeighbor(position, Direction.Right) is not Air)
        {
            byte MinRGB = 219;
            byte MaxRGB = 109;
            
            byte Result = (byte)Math.Clamp(Math.Round(100f / _targetSporeTicks * _sporeTicks), MaxRGB, MinRGB);

            return $"#{Result:X2}{Result:X2}{Result:X2}";
        }
        if(Grid.GetNeighbor(position, Direction.Up) is Stalk stalk)
            return stalk.GetColor(new Position(position.X, position.Y + 1));
        
        return "#4f3b1a";
    }

    public char Symbol { get; } = '#';
}