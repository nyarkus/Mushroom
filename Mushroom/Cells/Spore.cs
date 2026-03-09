using System;
using Mushroom.Data;
using Godot;

namespace Mushroom.Ceils;

public class Spore : ICell
{
    private int _ticksOnDirt = 0;
    private int _ticksOnAir = 0;
    private int _airTicks = 0;
    private int _windDirection;
    private Random _rand = new();
    
    public Spore()
    {
        _windDirection = _rand.Next(0, 2) == 0 ? -1 : 1;
    }
    
    public Action? Do(Vector2I position)
    {
        var down = Grid.GetNeighbor(position, Direction.Down);
        
        if (down is Dirt)
        {
            _ticksOnDirt += 1;

            if (_ticksOnDirt > 50)
            {
                if (Raycast.Cast(position + new Vector2I(0, -1), new Vector2I(0, -1)).IsHit)
                    return new Action(() => Grid.Set(position, Air.Instance));
                
                bool isTooClose = false;
                for (int x = -2; x <= 2; x++)
                {
                    var checkPos = new Vector2I(position.X + x, position.Y + 1);
                    if (Grid.Get(checkPos) is Mycelium) isTooClose = true;
                    
                    var checkPosStalk = new Vector2I(position.X + x, position.Y);
                    if (Grid.Get(checkPosStalk) is Stalk) isTooClose = true;
                }

                if (isTooClose)
                    return new Action(() => Grid.Set(position, Air.Instance));
                
                return new Action(() =>
                {
                    Grid.Set(new Vector2I(position.X, position.Y + 1), new Mycelium() { Main = true });
                    Grid.Set(position, new RottingMatter());
                });
            }
        }
        else
        {
            _ticksOnAir++;
            if (_ticksOnAir > 200)
                return new Action(() => Grid.Set(position, new RottingMatter()));

            _airTicks++;
            
            if (_airTicks >= 3)
            {
                var targetPos = new Vector2I(position.X + _windDirection, position.Y + 1);
                
                if (Grid.Get(targetPos) is Air)
                {
                    return new Action(() => 
                    {
                        _airTicks = 0;
                        Grid.Move(position, targetPos);
                    });
                }
                _windDirection *= -1; 
                _airTicks = 0;
                
            }
            else if (Grid.GetNeighbor(position, Direction.Down) is Air)
                 return new Action(() => Grid.Move(position, new Vector2I(position.X, position.Y + 1)));
        }

        return null;
    }

    public string GetColor(Vector2I position)
        => "#f7f7f7";

    public char Symbol { get; } = '~';
}