using System;
using Mushroom.Data;
using Godot;

namespace Mushroom.Ceils;

public class Air : ICell
{
    public Action Do(Vector2I vector2) => null;

    public string GetColor(Vector2I vector2)
        => "#000000";
    public char Symbol { get; } = ' ';

    public static Air Instance { get; } = new();
}