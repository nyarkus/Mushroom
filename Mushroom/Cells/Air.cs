using System;
using Mushroom.Data;
using Godot;

namespace Mushroom.Ceils;

public class Air : CellBase
{
    public override Action Do(Vector2I vector2) => null;

    public override Color GetColor(Vector2I vector2)
        => new Color(0, 0, 0);
    public char Symbol { get; } = ' ';

    public static Air Instance { get; } = new();
}