using System;
using Mushroom.Data;

namespace Mushroom.Ceils;

public class WormCocoon : ICell
{
    private int _ticks = 0;
    private Random _rand = new();
    public Action? Do(Position position)
    {
        _ticks++;
        if (_ticks >= 350)
        {
            var length = _rand.Next(2, 5);
            for (int i = 1; i < length; i++)
            {
                var tailPos = new Position(position.X - i, position.Y);
                
                if(Grid.Get(tailPos) is not Dirt)
                    return null;
            }
            
            Worm.Spawn(position, length);
        }
        
        return null;
    }

    public string GetColor(Position position)
        => "#e5e5cc";

    public char Symbol { get; } = '*';
}