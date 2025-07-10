using System;
using System.Drawing;
using Mushroom.Data;

namespace Mushroom.Ceils;

public class Water : ICell
{
    private static Random _rand = new Random();
    private int lifeTime = 0;
    public Action? Do(Position position)
    {
        var downPosition = new Position(position.X, position.Y + 1);
        var downCell = Grid.Get(downPosition);
        
        if (downCell is Air)
        {
            return new Action(() => Grid.Move(position, downPosition));
        }
        
        var leftPosition = new Position(position.X - 1, position.Y);
        var rightPosition = new Position(position.X + 1, position.Y);

        bool canMoveLeft = Grid.Get(leftPosition) is Air;
        bool canMoveRight = Grid.Get(rightPosition) is Air;

        {
            Direction[] directions = [Direction.Up, Direction.Down, Direction.Left, Direction.Right];
            int waterCeils = 0;
            bool isAir = false;
            foreach (var dir in directions)
            {
                if(Grid.GetNeighbor(position, dir) is Water)
                    waterCeils++;
                if(Grid.GetNeighbor(position, dir) is Air)
                    isAir = true;
                if(Grid.GetNeighbor(position, dir) is Dirt dirt && dirt.Dampness < 1f)
                    return new Action(() =>
                    {
                        dirt.Dampness = Math.Clamp(dirt.Dampness + 0.3f, 0f, 1f);
                        Grid.Set(position, Air.Instance);
                    });
            }
            
            if(isAir && waterCeils < 3)
                lifeTime++;
        }
        
        if (lifeTime > 300)
            return new Action(() =>
            {
                Grid.Set(position, Air.Instance);
            });
        
        if (canMoveLeft && canMoveRight)
        {
            var targetPosition = _rand.Next(0, 2) == 0 ? leftPosition : rightPosition;
            return new Action(() => Grid.Move(position, targetPosition));
        }
        else if (canMoveLeft)
        {
            return new Action(() => Grid.Move(position, leftPosition));
        }
        else if (canMoveRight)
        {
            return new Action(() => Grid.Move(position, rightPosition));
        }
        
        return null;
    }

    public string GetColor(Position position)
    {
        int depth = 0;
        var currentPos = new Position(position.X, position.Y + 1);
        
        while (Grid.Get(currentPos) is Water && depth < 10)
        {
            depth++;
            currentPos = new Position(currentPos.X, currentPos.Y + 1);
        }
        
        Color baseColor = ColorTranslator.FromHtml("#55a8e8");
        
        int r = Math.Max(0, baseColor.R - depth * 15);
        int g = Math.Max(0, baseColor.G - depth * 15);
        int b = Math.Max(0, baseColor.B - depth * 10);

        return ColorTranslator.ToHtml(Color.FromArgb(r, g, b));
    }

    public char Symbol { get; } = '~';
}