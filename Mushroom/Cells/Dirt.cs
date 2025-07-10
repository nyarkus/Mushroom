using System;
using System.Collections.Generic;
using System.Globalization;
using Mushroom.Data; // Предполагается, что здесь Position, Grid, Direction и т.д.

namespace Mushroom.Ceils;

public class Dirt : ICell
{
    private const float MAX_DAMPNESS = 1.0f;
    private const float MIN_DAMPNESS = 0.0f;
    private const float EVAPORATION_RATE = 0.005f;
    private const float GRAVITY_FLOW_RATE = 0.2f; 
    private const float HORIZONTAL_FLOW_RATE = 0.1f;

    // --- Цвета для градиента ---
    private static readonly (byte R, byte G, byte B) DRY_COLOR = (0xA0, 0x82, 0x61); // #a08261
    private static readonly (byte R, byte G, byte B) WET_COLOR = (0x38, 0x25, 0x11); // #382511

    public float Dampness { get; set; } = 0.9f;
    public float Nutrients { get; set; } = 1.0f;
    
    public char Symbol { get; } = '#';

    public Action? Do(Position position)
    {
        var pendingChanges = new List<(ICell cell, float dampnessDelta)>();
        
        float currentDampnessChange = 0;
        
        var up = Grid.GetNeighbor(position, Direction.Up);
        if (up is Air && Dampness > MIN_DAMPNESS)
        {
            currentDampnessChange -= EVAPORATION_RATE;
        }
        
        var down = Grid.GetNeighbor(position, Direction.Down);
        if (down is Dirt dirtDown && Dampness > dirtDown.Dampness)
        {
            float amountToFlow = (Dampness - dirtDown.Dampness) / 2 * GRAVITY_FLOW_RATE;
            currentDampnessChange -= amountToFlow;
            pendingChanges.Add((dirtDown, amountToFlow));
        }
        
        var right = Grid.GetNeighbor(position, Direction.Right);
        if (right is Dirt dirtRight && Dampness > dirtRight.Dampness)
        {
            float amountToFlow = (Dampness - dirtRight.Dampness) / 2 * HORIZONTAL_FLOW_RATE;
            currentDampnessChange -= amountToFlow;
            pendingChanges.Add((dirtRight, amountToFlow));
        }

        var left = Grid.GetNeighbor(position, Direction.Left);
        if (left is Dirt dirtLeft && Dampness < dirtLeft.Dampness)
        {
            float amountToFlow = (dirtLeft.Dampness - Dampness) / 2 * HORIZONTAL_FLOW_RATE;
            currentDampnessChange += amountToFlow;
        }

        
        if (Math.Abs(currentDampnessChange) > 0.0001f || pendingChanges.Count > 0)
        {
            return new Action(() =>
            {
                Dampness += currentDampnessChange;
                
                foreach (var change in pendingChanges)
                {
                    if (change.cell is Dirt targetDirt)
                    {
                        targetDirt.Dampness += change.dampnessDelta;
                    }
                }
                
                Dampness = Math.Clamp(Dampness, MIN_DAMPNESS, MAX_DAMPNESS);
            });
        }
        
        return null;
    }

    public string GetColor(Position position)
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