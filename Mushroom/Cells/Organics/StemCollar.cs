using System;
using Godot;
using Mushroom.Data;

namespace Mushroom.Cells.Organics;

public class StemCollar : CellBase, IOrganic
{
    public float Water { get; set; }
    public float MaxWater { get; set; } = 5f;
    public float Energy { get; set; }
    public float MaxEnergy { get; set; } = 5f;

    private float _stemEnergyCost = 1f;
    private float _stemWaterCost = 1f;

    public override Action Do(Vector2I position)
    {
        throw new NotImplementedException();
    }

    public override Color GetColor(Vector2I position)
    {
        throw new NotImplementedException();
    }

    public override Color GetUiColor()
    {
        throw new NotImplementedException();
    }
}