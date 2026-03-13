using System;
using Mushroom.Data;
using Godot;

namespace Mushroom.Ceils;

public class Water : CellBase
{
    private int lifeTime = 0;

    public override Action? Do(Vector2I position)
    {
        int nextLifeTime = lifeTime;
        
        Direction[] directions = { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
        int waterCeils = 0;
        bool isAir = false;
        
        foreach (var dir in directions)
        {
            var neighbor = Grid.GetNeighbor(position, dir);
            if (neighbor is Water) 
                waterCeils++;
            if (neighbor is Air) 
                isAir = true;

            if (neighbor is Dirt dirt && dirt.Dampness < 1f)
            {
                return () =>
                {
                    dirt.Dampness = Math.Clamp(dirt.Dampness + 0.3f, 0f, 1f);
                    Grid.Set(position, Air.Instance);
                };
            }
        }

        if (isAir && waterCeils < 3) 
            nextLifeTime++;

        if (nextLifeTime > 300)
            return () => Grid.Set(position, Air.Instance);

        var downPosition = new Vector2I(position.X, position.Y + 1);
        if (Grid.Get(downPosition) is Air)
            return () =>
            {
                lifeTime = nextLifeTime; 
                Grid.Move(position, downPosition);
            };
        
        var downLeftPosition = new Vector2I(position.X - 1, position.Y + 1);
        var downRightPosition = new Vector2I(position.X + 1, position.Y + 1);

        bool canFlowLeftDown = Grid.Get(downLeftPosition) is Air;
        bool canFlowRightDown = Grid.Get(downRightPosition) is Air;
        
        if (canFlowLeftDown && canFlowRightDown)
        {
            var targetPosition = Random.Shared.Next(0, 2) == 0 ? downLeftPosition : downRightPosition;
            return () =>
            {
                lifeTime = nextLifeTime; 
                Grid.Move(position, targetPosition);
            };
        }
        if (canFlowLeftDown) 
            return () =>
            {
                lifeTime = nextLifeTime; 
                Grid.Move(position, downLeftPosition);
            };
        if (canFlowRightDown) 
            return () => 
            { 
                lifeTime = nextLifeTime; 
                Grid.Move(position, downRightPosition);
            };
        
        var leftPosition = new Vector2I(position.X - 1, position.Y);
        var rightPosition = new Vector2I(position.X + 1, position.Y);

        bool canMoveLeft = Grid.Get(leftPosition) is Air;
        bool canMoveRight = Grid.Get(rightPosition) is Air;
        
        if (canMoveLeft && canMoveRight)
        {
            var targetPosition = Random.Shared.Next(0, 2) == 0 ? leftPosition : rightPosition;
            return () =>
            {
                lifeTime = nextLifeTime; 
                Grid.Move(position, targetPosition);
            };
        }

        if (canMoveLeft) 
            return () =>
            {
                lifeTime = nextLifeTime; 
                Grid.Move(position, leftPosition);
            };
        if (canMoveRight) 
            return () =>
            {
                lifeTime = nextLifeTime; 
                Grid.Move(position, rightPosition);
            };
        

        if (nextLifeTime != lifeTime)
            return () =>
            {
                lifeTime = nextLifeTime;
            };

        return null;
    }

    public override Color GetColor(Vector2I position)
    {
        //int depth = 0;
        //var currentPos = new Vector2I(position.X, position.Y + 1);

        // while (Grid.Get(currentPos) is Water && depth < 10)
        // {
        //     depth++;
        //     currentPos = new Vector2I(currentPos.X, currentPos.Y + 1);
        // }

        Color baseColor = new Color(0.33f, 0.66f, 0.9f);

        //baseColor.R = Math.Max(0, baseColor.R - depth * 15);
        //baseColor.G = Math.Max(0, baseColor.G - depth * 15);
        //baseColor.B = Math.Max(0, baseColor.B - depth * 10);

        return baseColor;
    }

    public override Color GetUiColor()
        => new Color(0.33f, 0.66f, 0.9f);
}