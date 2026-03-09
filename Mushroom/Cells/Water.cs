using System;
using Mushroom.Data;
using Godot;

namespace Mushroom.Ceils;

public class Water : ICell
{
    private static readonly Random _rand = new Random();
    private int lifeTime = 0;

    public Action? Do(Vector2I vector2)
    {
        {
            Direction[] directions = { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
            int waterCeils = 0;
            bool isAir = false;
            foreach (var dir in directions)
            {
                var neighbor = Grid.GetNeighbor(vector2, dir);
                if (neighbor is Water)
                    waterCeils++;
                if (neighbor is Air)
                    isAir = true;

                if (neighbor is Dirt dirt && dirt.Dampness < 1f)
                {
                    return new Action(() =>
                    {
                        dirt.Dampness = Math.Clamp(dirt.Dampness + 0.3f, 0f, 1f);
                        Grid.Set(vector2, Air.Instance);
                    });
                }
            }

            if (isAir && waterCeils < 3)
                lifeTime++;
        }

        if (lifeTime > 300)
        {
            return new Action(() => Grid.Set(vector2, Air.Instance));
        }

        var downPosition = new Vector2I(vector2.X, vector2.Y + 1);
        if (Grid.Get(downPosition) is Air)
        {
            return new Action(() => Grid.Move(vector2, downPosition));
        }
        
        var downLeftPosition = new Vector2I(vector2.X - 1, vector2.Y + 1);
        var downRightPosition = new Vector2I(vector2.X + 1, vector2.Y + 1);

        bool canFlowLeftDown = Grid.Get(downLeftPosition) is Air;
        bool canFlowRightDown = Grid.Get(downRightPosition) is Air;
        
        if (canFlowLeftDown && canFlowRightDown)
        {
            var targetPosition = _rand.Next(0, 2) == 0 ? downLeftPosition : downRightPosition;
            return new Action(() => Grid.Move(vector2, targetPosition));
        }
        if (canFlowLeftDown)
        {
            return new Action(() => Grid.Move(vector2, downLeftPosition));
        }
        if (canFlowRightDown)
        {
            return new Action(() => Grid.Move(vector2, downRightPosition));
        }
        
        var leftPosition = new Vector2I(vector2.X - 1, vector2.Y);
        var rightPosition = new Vector2I(vector2.X + 1, vector2.Y);

        bool canMoveLeft = Grid.Get(leftPosition) is Air;
        bool canMoveRight = Grid.Get(rightPosition) is Air;
        
        if (canMoveLeft && canMoveRight)
        {
            var targetPosition = _rand.Next(0, 2) == 0 ? leftPosition : rightPosition;
            return new Action(() => Grid.Move(vector2, targetPosition));
        }

        if (canMoveLeft)
        {
            return new Action(() => Grid.Move(vector2, leftPosition));
        }
        if (canMoveRight)
        {
            return new Action(() => Grid.Move(vector2, rightPosition));
        }
        
        return null;
    }

    public Color GetColor(Vector2I vector2)
    {
        int depth = 0;
        var currentPos = new Vector2I(vector2.X, vector2.Y + 1);

        while (Grid.Get(currentPos) is Water && depth < 10)
        {
            depth++;
            currentPos = new Vector2I(currentPos.X, currentPos.Y + 1);
        }

        Color baseColor = new Color(0.33f, 0.66f, 0.9f);

        //baseColor.R = Math.Max(0, baseColor.R - depth * 15);
        //baseColor.G = Math.Max(0, baseColor.G - depth * 15);
        //baseColor.B = Math.Max(0, baseColor.B - depth * 10);

        return baseColor;
    }

    public char Symbol { get; } = '~';
}