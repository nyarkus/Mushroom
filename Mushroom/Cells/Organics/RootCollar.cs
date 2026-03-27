using System;
using System.Collections.Generic;
using Godot;
using Mushroom.Ceils;
using Mushroom.Data;

namespace Mushroom.Cells.Organics;

public class RootCollar : CellBase, IOrganic
{
    public RootType Type { get; set; }
    public int RootLength { get; set; }

    private List<RootNode> _roots = new();

    public float Water { get; set; } = 0.5f;
    public float MaxWater { get; set; } = 5f;
    public float Energy { get; set; } = 0.5f;
    public float MaxEnergy { get; set; } = 5f;

    private float _rootEnergyCost = 1f;
    private float _rootWaterCost = 1f;
    
    public override Action Do(Vector2I position)
    {
        if(_roots.Count == 0)
            RecalculateRoots(position);

        int targetIndex = -1;
        for (int i = 0; i < _roots.Count; i++)
        {
            var cell = Grid.Get(_roots[i].Position);
            if(cell is Root)
                continue;
        
            if (cell is not Air && cell is not Dirt && cell is not Ceils.Water)
            {
                RecalculateRoots(position);
                return null;
            }

            targetIndex = i;
            break;
        }
    
        return () =>
        {
            if (targetIndex >= 0 && Water >= _rootWaterCost && Energy >= _rootEnergyCost)
            {
                Water -= _rootWaterCost;
                Energy -= _rootEnergyCost;
            
                var target = _roots[targetIndex];
                Grid.Set(target.Position, new Root() { ParentPosition = target.Parent });
            }
        };
    }

    public void RecalculateRoots(Vector2I position)
    {
        _roots.Clear();

        if (Type == RootType.Taproot)
        {
            var pos = position + new Vector2I(Random.Shared.Next(-2, 3), RootLength);
            AddPathToRoots(position, pos);
        }
        else if (Type == RootType.Fibrous)
        {
            int fibrousLength = Mathf.Max(2, RootLength);

            Vector2I[] endpoints = 
            {
                position + new Vector2I(Random.Shared.Next(-fibrousLength, -1), fibrousLength),
                position + new Vector2I(Random.Shared.Next(-1, 2), fibrousLength),
                position + new Vector2I(Random.Shared.Next(2, fibrousLength + 1), fibrousLength)
            };

            foreach (var endPos in endpoints)
                AddPathToRoots(position, endPos);
            
            _roots.Sort((a, b) => 
                position.DistanceSquaredTo(a.Position).CompareTo(position.DistanceSquaredTo(b.Position)));
        }
    }
    
    private void AddPathToRoots(Vector2I startPos, Vector2I endPos)
    {
        List<Vector2I> path = AStarPathfinder.FindPath(startPos, endPos, 50, typeof(Dirt));
        
        if (path == null) return;

        for (int i = 0; i < path.Count; i++)
        {
            var currentPos = path[i];
            
            if (_roots.Exists(r => r.Position == currentPos))
                continue;

            var parentPos = i == 0 ? startPos : path[i - 1];
            _roots.Add(new RootNode 
            { 
                Position = currentPos, 
                Parent = parentPos 
            });
        }
    }

    public override Color GetColor(Vector2I position)
        => GetUiColor();

    public override Color GetUiColor()
        => new Color(0.13f, 0.12f, 0.1f);
    
    public enum RootType
    {
        Taproot, // 1 long root
        Fibrous // a lot of small roots
    }
    
    private struct RootNode
    {
        public Vector2I Position;
        public Vector2I Parent;
    }
}