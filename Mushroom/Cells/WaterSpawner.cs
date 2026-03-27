using System;
using Godot;
using Mushroom.Data;
using Mushroom.Mushroom.Data;

namespace Mushroom.Ceils;

[Spawnable]
public class WaterSpawner : CellBase
{
    public override Action Do(Vector2I position)
    {
        return () =>
        {
            for (int y = -1; y < 1; y++)
            {
                for (int x = -1; x < 1; x++)
                {
                    var pos = position + new Vector2I(x, y);
                    var cell = Grid.Get(pos);
                    if(cell is Air)
                        Grid.Set(pos, new Water());
                    else if (cell is Dirt dirt)
                        dirt.Dampness = 1f;
                }
            }
        };
    }

    public override Color GetColor(Vector2I position)
        => GetUiColor();

    public override Color GetUiColor()
        => new Color(0.22f, 0.24f, 0.38f);
}