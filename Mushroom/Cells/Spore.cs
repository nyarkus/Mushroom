using System;
using Mushroom.Data;

namespace Mushroom.Ceils;

public class Spore : ICell
{
    private int _ticksOnDirt = 0;
    private int _ticksOnAir = 0;
    private int _airTicks = 0;
    private Direction _direction = Direction.Right;
    private Random _rand = new();
    public Action? Do(Position position)
    {
        var down = Grid.GetNeighbor(position, Direction.Down);
        if (down is Dirt)
        {
            _ticksOnDirt += 1;

            if (_ticksOnDirt > 50)
            {
                Grid.Set(new Position(position.X, position.Y + 1), new Mycelium(){ Main = true } );
                Grid.Set(position, Air.Instance);
            }
        }
        else
        {
            _ticksOnAir++;
            if (_ticksOnAir > 200)
                return new Action(() =>
                {
                    Grid.Set(position, Air.Instance);
                });
            var leftPosition = new Position(position.X - 1, position.Y);
            var rightPosition = new Position(position.X + 1, position.Y);
            
            if(Grid.GetNeighbor(position, Direction.Down) is Air)
                _airTicks++;
            if (_airTicks >= 4)
            {
                return new Action(() =>
                {
                    _airTicks = 0;
                    Grid.Move(position, new Position(leftPosition.X, leftPosition.Y + 1));
                });
            }

            bool canMoveLeft = Grid.Get(leftPosition) is Air;
            bool canMoveRight = Grid.Get(rightPosition) is Air;

            if(!canMoveLeft)
                _direction = Direction.Right;
            if(!canMoveRight)
                _direction = Direction.Left;
            if (!canMoveLeft && !canMoveRight)
                return null;
            
            if (_direction == Direction.Left)
            {
                return new Action(() => Grid.Move(position, leftPosition));
            }
            if (_direction == Direction.Right)
            {
                return new Action(() => Grid.Move(position, rightPosition));
            }
            
        }

        return null;
    }

    public string GetColor(Position position)
        => "#f7f7f7";

    public char Symbol { get; } = '~';
}