using System;
using Godot;
using Mushroom.Ceils;
using Mushroom.Data;

namespace Mushroom.Generation;

public static class Generator
{
    public static IProgress<float> Progress { get; set; }
    /// <summary>
    /// Creates new world from 2 points - 0x0 and <paramref name="size"/>
    /// <param name="groundLevel">If 0 it's will be set as <paramref name="size"/>.y / 3</param>
    /// </summary>
    public static void Generate(Vector2I size, int groundLevel = 0, int seed = 0)
    {
        Grid.Initialize(size);
        var rand = new Random(seed);

        var noise = new FastNoiseLite();
        noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
        noise.Seed = seed;
        noise.Frequency = 0.03f;
        
        noise.FractalType = FastNoiseLite.FractalTypeEnum.Fbm;
        noise.FractalOctaves = 3; 
        
        float amplitude = 15f; // Height

        int baseGroundLevel = (groundLevel == 0) ? size.Y / 2 : groundLevel;
        Grid.GroundLevel = baseGroundLevel;

        
        int myceliumX = rand.Next(0, size.X);

        int[] heights = new int[size.X];
        for (int x = 0; x < size.X; x++)
        {
            float noiseValue = noise.GetNoise1D(x);
            int heightOffset = (int)(noiseValue * amplitude);
            heights[x] = Math.Clamp(baseGroundLevel + heightOffset, 5, size.Y - 3);
        }
        
        int progress = 0;
        for (int y = 0; y < size.Y; y++)
        {
            for (int x = 0; x < size.X; x++)
            {
                int currentGroundLevel = heights[x]; 
                
                if (y == currentGroundLevel && x == myceliumX)
                {
                    Grid.Set(x, y, new Mycelium() { Main = true });
                    continue;
                }

                if (y >= currentGroundLevel)
                {
                    bool canBeSand = true;
                    if (currentGroundLevel > baseGroundLevel) 
                    {
                        for (int Y = y; Y > 0; --Y)
                        {
                            if (Grid.Get(x, Y) is Sand)
                            {
                                canBeSand = false;
                                break;
                            }
                        }
                    }
                    else
                        canBeSand = false;

                    if (canBeSand)
                        Grid.Set(x, y, new Sand()); 
                    else
                    {
                        float dampness = Math.Clamp(0.1f + (y - currentGroundLevel) * 0.1f, 0.1f, 1f);
                        float nutrients = Math.Clamp(1f - (y - currentGroundLevel) * 0.05f, 0.5f, 1f);
                        Grid.Set(x, y, new Dirt() { Dampness = dampness, Nutrients = nutrients });
                    }
                }
                else if (y >= baseGroundLevel)
                    Grid.Set(x, y, new Water());
            }
            
            progress++;
            Progress.Report(100f / size.Y * progress);
        }
        
        Worm.Spawn(new Vector2I(size.X / 2, Grid.GroundLevel), 3);
        //Worm.Spawn(new Vector2I(size.X / 2, Grid.GroundLevel + 3), 3);
    }
}