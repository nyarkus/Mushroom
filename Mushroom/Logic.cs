using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Godot;
using Mushroom.Data;

namespace Mushroom;

public static class Logic
{
    public static void Calculate()
    {
        var queue = new ConcurrentQueue<Action>();
        
        Parallel.For(0, Grid.Size.Y, y =>
        {
            for (int x = 0; x < Grid.Size.X; x++)
            {
                var ceil = Grid.Get(x, y);
                
                var vector = new Vector2I(x, y);
                var action = ceil.Do(vector);
                ceil.GetAndCacheColor(vector);
                
                if(action != null)
                    queue.Enqueue(action);
            }
        });
        
        foreach (var action in queue)
            action();
    }
}