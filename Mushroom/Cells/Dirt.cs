using System;
using System.Collections.Generic;
using Godot;
using Mushroom.Data;

namespace Mushroom.Ceils;

public class Dirt : ICell
{
    private const float MAX_DAMPNESS = 1.0f;
    private const float MIN_DAMPNESS = 0.0f;
    private const float EVAPORATION_RATE = 0.002f; 
    private const float GRAVITY_FLOW_RATE = 0.05f; 
    private const float HORIZONTAL_FLOW_RATE = 0.02f; 
    private const float RETENTION_CAPACITY = 0.45f; 
    private const float NUTRIENT_FLOW_RATE = 0.005f;

    private static readonly (byte R, byte G, byte B) DRY_COLOR = (0xA0, 0x82, 0x61); 
    private static readonly (byte R, byte G, byte B) WET_COLOR = (0x38, 0x25, 0x11); 

    public float Dampness { get; set; } = 0.9f;
    public float Nutrients { get; set; } = 1.0f;
    
    public char Symbol { get; } = '#';

    public Action? Do(Vector2I vector2)
    {
        var pendingChanges = new List<(Dirt cell, float dampnessDelta, float nutrientDelta)>();
        
        float currentDampnessChange = 0;
        float currentNutrientChange = 0;
        
        float excessWater = Math.Max(0, Dampness - RETENTION_CAPACITY);
        
        var directions = new[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
        
        foreach (var dir in directions)
        {
            var neighborCell = Grid.GetNeighbor(vector2, dir);
            
            if (dir == Direction.Up && neighborCell is Air && Dampness > MIN_DAMPNESS)
            {
                float evapMulti = Dampness > RETENTION_CAPACITY ? 1.0f : 0.2f;
                currentDampnessChange -= EVAPORATION_RATE * evapMulti;
                
                if (GD.Randi() < 0.001f) 
                    Nutrients = Math.Clamp(Nutrients + 0.01f, 0f, 1f);
            }
            
            if (neighborCell is Dirt dirtNeighbor)
            {
                float dampFlow = 0;
                float nutrFlow = 0;
                
                if (Dampness > dirtNeighbor.Dampness)
                {
                    if (dir == Direction.Down)
                    {
                        float desiredFlow = (Dampness - dirtNeighbor.Dampness) / 2 * GRAVITY_FLOW_RATE;
                        dampFlow = excessWater > 0 ? Math.Min(excessWater, desiredFlow) : desiredFlow / 10f;
                    }
                    else if ((dir == Direction.Left || dir == Direction.Right) && excessWater > 0)
                    {
                        dampFlow = Math.Min(excessWater, (Dampness - dirtNeighbor.Dampness) / 2 * HORIZONTAL_FLOW_RATE);
                    }
                }
                
                if (Nutrients > dirtNeighbor.Nutrients)
                {
                    nutrFlow = (Nutrients - dirtNeighbor.Nutrients) / 2 * NUTRIENT_FLOW_RATE * Math.Max(0.1f, Dampness);
                }

                if (dampFlow > 0 || nutrFlow > 0)
                {
                    currentDampnessChange -= dampFlow;
                    currentNutrientChange -= nutrFlow;
                    pendingChanges.Add((dirtNeighbor, dampFlow, nutrFlow));
                }
            }
        }
        
        if (Math.Abs(currentDampnessChange) > 0.0001f || Math.Abs(currentNutrientChange) > 0.0001f || pendingChanges.Count > 0)
        {
            return new Action(() =>
            {
                Dampness = Math.Clamp(Dampness + currentDampnessChange, MIN_DAMPNESS, MAX_DAMPNESS);
                Nutrients = Math.Clamp(Nutrients + currentNutrientChange, 0f, 1f);
                
                foreach (var change in pendingChanges)
                {
                    change.cell.Dampness = Math.Clamp(change.cell.Dampness + change.dampnessDelta, MIN_DAMPNESS, MAX_DAMPNESS);
                    change.cell.Nutrients = Math.Clamp(change.cell.Nutrients + change.nutrientDelta, 0f, 1f);
                }
            });
        }
        
        return null;
    }

    public string GetColor(Vector2I vector2)
    {
        float t = Math.Clamp(Dampness / MAX_DAMPNESS, 0.0f, 1.0f);
        return LerpHexColor(DRY_COLOR, WET_COLOR, t);
    }
    
    private static string LerpHexColor((byte R, byte G, byte B) colorA, (byte R, byte G, byte B) colorB, float t)
    {
        byte r = (byte)(colorA.R + (colorB.R - colorA.R) * t);
        byte g = (byte)(colorA.G + (colorB.G - colorA.G) * t);
        byte b = (byte)(colorA.B + (colorB.B - colorA.B) * t);
        return $"#{r:X2}{g:X2}{b:X2}";
    }
}