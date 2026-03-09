using System;
using Mushroom.Data;
using Godot;

namespace Mushroom.Ceils;

public class WormCocoon : CellBase
{
    private int _ticks = 0;
    public override Action? Do(Vector2I vector2)
    {
        if (_ticks >= 350)
        {
            var length = Random.Shared.Next(2, 5);
            for (int i = 1; i < length; i++)
            {
                var tailPos = new Vector2I(vector2.X - i, vector2.Y);
                
                if(Grid.Get(tailPos) is not Dirt)
                    return () => _ticks++;
            }

            return () =>
            {
                Grid.Set(vector2, new Dirt());
                Worm.Spawn(vector2, length);
            };
        }
        
        return () => _ticks++;
    }

    public override Color GetColor(Vector2I vector2)
        => new Color(0.89f, 0.89f, 0.8f);

    public char Symbol { get; } = '*';
}