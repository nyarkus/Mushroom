using System;
using System.Collections.Generic;
using Mushroom.Ceils;

namespace Mushroom;

public static class Weather
{
    private static Random rand = new();
    
    public static void Rain()
    {
        var targetCount = rand.Next(4, Grid.Size.Y / 4);

        HashSet<int> X = new();

        for (int i = 0; i < targetCount; i++)
            X.Add(rand.Next(0, Grid.Size.X + 1));
        

        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < Grid.Size.X; x++)
            {
                if(X.Contains(x))
                    Grid.Set(x, y, new Water());
            }
        }
    }
}