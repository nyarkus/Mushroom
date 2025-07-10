using System;
using Godot;
using Mushroom.Ceils;
using Mushroom.Data;

namespace Mushroom.Generation;

public static class Generator
{
    public static IProgress<float> Progress { get; set; }
    /// <summary>
    /// Creates new world from 2 points - 0x0 and <paramref name="position"/>
    /// <param name="groundLevel">If 0 it's will be set as <paramref name="position"/>.y / 3</param>
    /// </summary>
    public static void Generate(Position position, int groundLevel = 0, int seed = 0)
    {
        Grid.Size = position;
        var rand = new Random(seed);
        
        var noise = new PerlinNoise(seed);
        
        float frequency = 0.05f; // Smooth
        float amplitude = 5f; // Height

        int baseGroundLevel = (groundLevel == 0) ? position.Y / 2 : groundLevel;
        Grid.GroundLevel = baseGroundLevel;

        
        int myceliumX = rand.Next(0, position.X);

        int progress = 0;
        for (int y = 0; y < position.Y; y++)
        {
            for (int x = 0; x < position.X; x++)
            {
                float noiseValue = noise.GetNoise(x * frequency);
                
                int heightOffset = (int)(noiseValue * amplitude);
                int currentGroundLevel = baseGroundLevel + heightOffset;
                
                currentGroundLevel = Math.Clamp(currentGroundLevel, 5, position.Y - 3);

                if (y == currentGroundLevel && x == myceliumX)
                {
                    Grid.Set(x, y, new Mycelium() { Main = true });
                }
                else if (y >= currentGroundLevel)
                {

                    bool canBeSand = true;
                    for (int Y = y; Y > 0; --Y)
                    {
                        if (Grid.Get(x, Y) is Sand)
                        {
                            canBeSand = false;
                            break;
                        }
                    }
                    if (currentGroundLevel > baseGroundLevel && canBeSand)
                    {
                        Grid.Set(x, y, new Sand()); 
                    }
                    else
                    {
                        float dampness = Math.Clamp(0.5f + (y - currentGroundLevel) * 0.1f, 0.5f, 1f);
                        float nutrients = Math.Clamp(1f - (y - currentGroundLevel) * 0.05f, 0.5f, 1f);
                        Grid.Set(x, y, new Dirt() { Dampness = dampness, Nutrients = nutrients });
                    }
                }
                else if (y >= baseGroundLevel)
                {
                    Grid.Set(x, y, new Water());
                }

                progress++;
                Progress.Report(100f / (position.X * position.Y) * progress);
            }
        }
        
        Worm.Spawn(new Position(position.X / 2, position.Y / 2), 3);
    }
}