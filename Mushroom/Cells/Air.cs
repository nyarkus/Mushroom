using System;
using Mushroom.Data;
using Godot;

namespace Mushroom.Ceils;

public class Air : ICell
{
    public Action Do(Vector2I vector2) => null;

    public Color GetColor(Vector2I vector2)
        => new Color(0, 0, 0);
    public char Symbol { get; } = ' ';

    public static Air Instance { get; } = new();
}