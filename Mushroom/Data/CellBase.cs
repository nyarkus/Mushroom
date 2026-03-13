using System;
using Godot;

namespace Mushroom.Data;

public abstract class CellBase
{
    /// <summary>
    /// You must return an action, which engine will do
    /// </summary>
    public abstract Action Do(Vector2I position);
    
    // render
    public abstract Color GetColor(Vector2I position);
    public void GetAndCacheColor(Vector2I position)
        => CachedColor = GetColor(position);
    public Color CachedColor { get; protected set; }
    
    // ui
    public abstract Color GetUiColor();
}