using System;
using Godot;

namespace Mushroom.Data;

public interface ICell
{
    /// <summary>
    /// You must return an action, which engine will do
    /// </summary>
    public Action Do(Vector2I position);
    
    // render
    /// <summary>
    /// HEX
    /// </summary>
    public string GetColor(Vector2I position);
    public char Symbol { get; }
}