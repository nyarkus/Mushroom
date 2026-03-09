using System;
using Mushroom.Data;
using Godot;

namespace Mushroom.Ceils;

public class WormCocoon : ICell
{
    private int _ticks = 0;
    private Random _rand = new();
    public Action? Do(Vector2I vector2)
    {
        _ticks++;
        if (_ticks >= 350)
        {
            var length = _rand.Next(2, 5);
            for (int i = 1; i < length; i++)
            {
                var tailPos = new Vector2I(vector2.X - i, vector2.Y);
                
                if(Grid.Get(tailPos) is not Dirt)
                    return null;
            }
            Grid.Set(vector2, new Dirt());
            Worm.Spawn(vector2, length);
        }
        
        return null;
    }

    public Color GetColor(Vector2I vector2)
        => new Color(0.89f, 0.89f, 0.8f);

    public char Symbol { get; } = '*';
}