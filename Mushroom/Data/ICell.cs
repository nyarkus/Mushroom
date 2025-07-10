using System;

namespace Mushroom.Data;

public interface ICell
{
    /// <summary>
    /// You must return an action, which engine will do
    /// </summary>
    public Action Do(Position position);
    
    // render
    /// <summary>
    /// HEX
    /// </summary>
    public string GetColor(Position position);
    public char Symbol { get; }
}