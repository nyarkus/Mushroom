using System;
using Godot;
using Mushroom.Data;
using Mushroom.Mushroom.Data;

namespace Mushroom.Ceils;

[Spawnable]
public class FlowerSeed : CellBase
{
    public override Action Do(Vector2I position)
    {
        throw new NotImplementedException();
    }

    public override Color GetColor(Vector2I position)
        => GetUiColor();

    public override Color GetUiColor()
        => new Color(0.31f, 0.29f, 0.22f);
}