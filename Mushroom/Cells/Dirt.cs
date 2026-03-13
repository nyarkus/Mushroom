using System;
using System.Collections.Generic;
using Godot;
using Mushroom.Data;
using Mushroom.Mushroom.Data;

namespace Mushroom.Ceils;

[Spawnable]
public class Dirt : CellBase
{
    private const float MaxDampness = 1.0f;
    private const float MinDampness = 0.0f;
    private const float EvaporationRate = 0.002f; 
    private const float GravityFlowRate = 0.05f; 
    private const float HorizontalFlowRate = 0.02f; 
    private const float RetentionCapacity = 0.45f; 
    private const float NutrientFlowRate = 0.005f;

    private static readonly Color DryColor = new Color(0.63f, 0.5f, 0.38f); 
    private static readonly Color WetColor = new Color(0.22f, 0.15f, 0.06f);

    private static readonly Color GrassColor = new Color(0.27f, 0.55f, 0.25f);
    private static readonly Color DeadGrassColor = new Color(0.55f, 0.51f, 0.25f);
    private static readonly float GrassWaterConsumption = 0.00001f;
    private static readonly int DeadGrassTicks = 100;

    public float Dampness { get; set; } = 0.9f;
    public float Nutrients { get; set; } = 1.0f;

    private int _ticksWithoutWater = 0;
    private bool _isGrass = false;
    
    public override Action? Do(Vector2I vector2)
    {
        float nextDampness = Dampness;
        float nextNutrients = Nutrients;
        bool nextIsGrass = _isGrass;
        
        var pendingChanges = new List<(Dirt cell, float dampnessDelta, float nutrientDelta)>();
        
        float currentDampnessChange = 0;
        float currentNutrientChange = 0;
        
        float excessWater = Math.Max(0, nextDampness - RetentionCapacity);
        
        var directions = new[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
        
        foreach (var dir in directions)
        {
            var neighborCell = Grid.GetNeighbor(vector2, dir);
            
            if (dir == Direction.Up)
            {
                if (neighborCell is Air && nextDampness > MinDampness)
                {
                    nextIsGrass = true;

                    float evapMulti = Dampness > RetentionCapacity ? 1.0f : 0.2f;
                    currentDampnessChange -= EvaporationRate * evapMulti;

                    if (Random.Shared.NextDouble() < 0.001)
                        currentNutrientChange += 0.01f;
                }
                else if(neighborCell is not Air)
                    nextIsGrass = false;
            }
            
            if (neighborCell is Dirt dirtNeighbor)
            {
                float dampFlow = 0;
                float nutrFlow = 0;
                
                if (nextDampness > dirtNeighbor.Dampness)
                {
                    if (dir == Direction.Down)
                    {
                        float desiredFlow = (nextDampness - dirtNeighbor.Dampness) / 2 * GravityFlowRate;
                        dampFlow = excessWater > 0 ? Math.Min(excessWater, desiredFlow) : desiredFlow / 10f;
                    }
                    else if ((dir == Direction.Left || dir == Direction.Right) && excessWater > 0)
                        dampFlow = Math.Min(excessWater, (nextDampness - dirtNeighbor.Dampness) / 2 * HorizontalFlowRate);
                }
                
                if (nextNutrients > dirtNeighbor.Nutrients)
                {
                    nutrFlow = (nextNutrients - dirtNeighbor.Nutrients) / 2 * NutrientFlowRate * Math.Max(0.1f, nextDampness);
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
            return () =>
            {
                _isGrass = nextIsGrass;
                
                Dampness = Math.Clamp(Dampness + currentDampnessChange, MinDampness, MaxDampness);
                Nutrients = Math.Clamp(Nutrients + currentNutrientChange, 0f, 1f);
                
                foreach (var change in pendingChanges)
                {
                    change.cell.Dampness = Math.Clamp(change.cell.Dampness + change.dampnessDelta, MinDampness, MaxDampness);
                    change.cell.Nutrients = Math.Clamp(change.cell.Nutrients + change.nutrientDelta, 0f, 1f);
                }

                if (_isGrass && Grid.GetNeighbor(vector2, Direction.Down) is Dirt dirt && dirt.Dampness > MinDampness)
                {
                    dirt.Dampness -= GrassWaterConsumption;
                    _ticksWithoutWater--;
                }
                else
                    _ticksWithoutWater++;
            };
        }
        
        return () =>
        {
            _isGrass = nextIsGrass;
            
            if (_isGrass && Grid.GetNeighbor(vector2, Direction.Down) is Dirt dirt && dirt.Dampness > MinDampness)
            {
                dirt.Dampness -= GrassWaterConsumption;
                _ticksWithoutWater--;
            }
            else
                _ticksWithoutWater++;
        };
    }

    public override Color GetColor(Vector2I vector2)
    {
        if (_isGrass)
        {
            float k = Math.Clamp(_ticksWithoutWater / (float)DeadGrassTicks, 0f, 1f);
            return GrassColor.Lerp(WetColor, k);
        }
        float t = Math.Clamp(Dampness / MaxDampness, 0f, 1f);
        return DryColor.Lerp(WetColor, t);
    }

    public override Color GetUiColor()
    {
        return DryColor.Lerp(WetColor, 0.5f);
    }
}