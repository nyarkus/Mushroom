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

    private static readonly Color DRY_COLOR = new Color(0.63f, 0.5f, 0.38f); 
    private static readonly Color WET_COLOR = new Color(0.22f, 0.15f, 0.06f); 

    public float Dampness { get; set; } = 0.9f;
    public float Nutrients { get; set; } = 1.0f;
    
    public char Symbol { get; } = '#';

    public Action? Do(Vector2I vector2)
    {
        float nextDampness = Dampness;
        float nextNutrients = Nutrients;
        
        var pendingChanges = new List<(Dirt cell, float dampnessDelta, float nutrientDelta)>();
        
        float currentDampnessChange = 0;
        float currentNutrientChange = 0;
        
        float excessWater = Math.Max(0, nextDampness - RETENTION_CAPACITY);
        
        var directions = new[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
        
        foreach (var dir in directions)
        {
            var neighborCell = Grid.GetNeighbor(vector2, dir);
            
            if (dir == Direction.Up && neighborCell is Air && nextDampness > MIN_DAMPNESS)
            {
                float evapMulti = Dampness > RETENTION_CAPACITY ? 1.0f : 0.2f;
                currentDampnessChange -= EVAPORATION_RATE * evapMulti;
                
                if (Random.Shared.NextDouble() < 0.001) 
                    currentNutrientChange += 0.01f;
            }
            
            if (neighborCell is Dirt dirtNeighbor)
            {
                float dampFlow = 0;
                float nutrFlow = 0;
                
                if (nextDampness > dirtNeighbor.Dampness)
                {
                    if (dir == Direction.Down)
                    {
                        float desiredFlow = (nextDampness - dirtNeighbor.Dampness) / 2 * GRAVITY_FLOW_RATE;
                        dampFlow = excessWater > 0 ? Math.Min(excessWater, desiredFlow) : desiredFlow / 10f;
                    }
                    else if ((dir == Direction.Left || dir == Direction.Right) && excessWater > 0)
                        dampFlow = Math.Min(excessWater, (nextDampness - dirtNeighbor.Dampness) / 2 * HORIZONTAL_FLOW_RATE);
                }
                
                if (nextNutrients > dirtNeighbor.Nutrients)
                {
                    nutrFlow = (nextNutrients - dirtNeighbor.Nutrients) / 2 * NUTRIENT_FLOW_RATE * Math.Max(0.1f, nextDampness);
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

    public Color GetColor(Vector2I vector2)
    {
        float t = Math.Clamp(Dampness / MAX_DAMPNESS, 0.0f, 1.0f);
        return DRY_COLOR.Lerp(WET_COLOR, t);
    }
}