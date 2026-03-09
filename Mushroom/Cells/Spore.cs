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
    
    public Spore()
    {
        _windDirection = Random.Shared.Next(0, 2) == 0 ? -1 : 1;
    }
    
    public Action? Do(Vector2I position)
    {
        int nextTickDirt = _ticksOnDirt;
        int nextTickAir = _ticksOnAir;
        int nextAirTicks = _airTicks;
        int nextWindDir = _windDirection;
        
        var down = Grid.GetNeighbor(position, Direction.Down);
        
        if (down is Dirt)
        {
            nextTickDirt++;
            if (_ticksOnDirt > 50)
            {
                if (Raycast.Cast(position + new Vector2I(0, -1), new Vector2I(0, -1)).IsHit)
                    return () =>
                        Grid.Set(position, Air.Instance);
                
                bool isTooClose = false;
                for (int x = -2; x <= 2; x++)
                {
                    var checkPos = new Vector2I(position.X + x, position.Y + 1);
                    if (Grid.Get(checkPos) is Mycelium) isTooClose = true;
                    
                    var checkPosStalk = new Vector2I(position.X + x, position.Y);
                    if (Grid.Get(checkPosStalk) is Stalk) isTooClose = true;
                }

                if (isTooClose)
                    return () =>
                        Grid.Set(position, Air.Instance);
                
                return () =>
                {
                    Grid.Set(new Vector2I(position.X, position.Y + 1), new Mycelium() { Main = true });
                    Grid.Set(position, Air.Instance);
                };
            }
        }
        else
        {
            nextTickAir++;
            if (nextTickAir > 200)
                return () => Grid.Set(position, new RottingMatter() { BecomeAir = true } );

            nextAirTicks++;
            
            if (nextAirTicks >= 3)
            {
                var targetPos = new Vector2I(position.X + _windDirection, position.Y + 1);
                
                if (Grid.Get(targetPos) is Air)
                {
                    return new Action(() => 
                    {
                        _airTicks = 0;
                        _ticksOnDirt = nextTickDirt;
                        _ticksOnAir = nextTickAir;
                        _windDirection = nextWindDir;
                        
                        Grid.Move(position, targetPos);
                    });
                }
                
                nextWindDir *= -1; 
                nextAirTicks = 0;
                
            }
            else if (Grid.GetNeighbor(position, Direction.Down) is Air)
                 return () =>
                 {
                     _ticksOnDirt = nextTickDirt;
                     _ticksOnAir = nextTickAir;
                     _airTicks = nextAirTicks;
                     _windDirection = nextWindDir;
                     
                     Grid.Move(position, new Vector2I(position.X, position.Y + 1));
                 };
        }

        return () =>
        {
            _ticksOnDirt = nextTickDirt;
            _ticksOnAir = nextTickAir;
            _airTicks = nextAirTicks;
            _windDirection = nextWindDir;
        };
    }

    public Color GetColor(Vector2I position)
        => new Color(0.97f, 0.97f, 0.97f);

    public char Symbol { get; } = '~';
}