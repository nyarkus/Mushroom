using System;
using Mushroom.Data;
using Godot;
using Mushroom.Mushroom.Data;

namespace Mushroom.Ceils;

[Spawnable]
public class Air : CellBase
{
    public override Action Do(Vector2I vector2) => null;

    public override Color GetColor(Vector2I vector2)
        => GetUiColor();

    public override Color GetUiColor()
        => new Color(0, 0, 0);
    
    public static Air Instance { get; } = new();
}