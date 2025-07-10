using System;
using Mushroom.Data;

namespace Mushroom.Ceils;

public class Air : ICell
{
    public Action Do(Position position) => null;

    public string GetColor(Position position)
        => "#000000";
    public char Symbol { get; } = ' ';

    public static Air Instance { get; } = new();
}