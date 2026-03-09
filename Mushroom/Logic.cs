using System;
using System.Collections.Generic;
using Godot;
using Mushroom.Data;

namespace Mushroom;

public static class Logic
{
    public static void Calculate()
    {
        var queue = new Stack<Action>();
        
        for (int y = 0; y < Grid.Size.Y; y++)
        {
            for (int x = 0; x < Grid.Size.X; x++)
            {
                var ceil = Grid.Get(x, y);
                var action = ceil.Do(new Vector2I(x, y));
                if(action != null)
                    queue.Push(action);
            }
        }
        
        foreach (var action in queue)
            action();
    }
}