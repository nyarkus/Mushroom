using System;
using System.Collections.Generic;
using Godot;
using Mushroom.Ceils;
using Mushroom.Data;

namespace Mushroom.Cells.Organics;

public class Root : CellBase, IOrganic
{
    public float Water { get; set; }
    public float MaxWater { get; set; } = 1f;
    public float Energy { get; set; }
    public float MaxEnergy { get; set; } = 1f;

    public Vector2I ParentPosition;

    private Color _dryColor = new Color(0.26f, 0.23f, 0.20f);
    private Color _wetColor = new Color(0.13f, 0.11f, 0.10f);
    
    public override Action Do(Vector2I position)
    {
        var dirts = new List<Dirt>();
        foreach (var dir in Enum.GetValues<Direction>())
        {
            if(Grid.GetNeighbor(position, dir) is not Dirt dirt)
                continue;
            
            dirts.Add(dirt); 
        }

        return () =>
        {
            foreach (var dirt in dirts)
            {
                float needWater = Mathf.Min(MaxWater - Water, 0.05f);
                float needEnergy = Mathf.Min(MaxEnergy - Energy, 0.05f);
                
                if (needWater > 0 && dirt.Dampness > needWater)
                {
                    dirt.Dampness -= needWater;
                    Water += needWater;
                }
                if(needEnergy > 0 && dirt.Nutrients > needEnergy)
                {
                    dirt.Nutrients -= needEnergy;
                    Energy += needEnergy;
                }
            }

            Water -= 0.001f;
            Energy -= 0.001f;

            float waterToTransfer = Water - 0.1f;
            float energyToTransfer = Energy - 0.1f;

            if (Grid.Get(ParentPosition) is IOrganic parent)
            {
                if (waterToTransfer > 0)
                {
                    waterToTransfer = Mathf.Min(parent.MaxWater - parent.Water, waterToTransfer);
                    parent.Water += waterToTransfer;
                    Water -= waterToTransfer;
                }
            
                if(energyToTransfer > 0)
                {
                    energyToTransfer = Mathf.Min(parent.MaxEnergy - parent.Energy, energyToTransfer);
                    parent.Energy += energyToTransfer;
                    Energy -= energyToTransfer;
                }   
            }
        };
    }

    public override Color GetColor(Vector2I position)
    {
        float k = Mathf.Clamp(Water / MaxWater, 0f, 1f);
        return _dryColor.Lerp(_wetColor, k);
    }

    public override Color GetUiColor()
        => _dryColor;
}