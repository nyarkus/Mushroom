using Mushroom.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mushroom.Ceils;

public class Worm : ICell
{
    private Position[] positions;
    private Random rand = new();
    private bool isStuck = false;
    private bool canReproduce = true;
    
    public bool Main { get; set; } 
    public int Age { get; set; }
    public int TargetLifeTime { get; set; } = 1000;
    
    public Action? Do(Position position)
{
    if (!Main)
        return null;

    Age++;
    if (Age >= TargetLifeTime)
        return new Action(() =>
        {
            Grid.Set(position, new Dirt() { Dampness = 1f, Nutrients = 1f });
            foreach (var pos in positions)
            {
                Grid.Set(pos, new Dirt() { Dampness = 0.5f, Nutrients = 1f });
            }
        });
    if (canReproduce && Age >= TargetLifeTime / 2)
    {
        return new Action(() =>
        {
            canReproduce = false;
            Position[] potentialSpots = 
            [
                new Position(position.X - 1, position.Y), 
                new Position(position.X + 1, position.Y), 
                new Position(position.X, position.Y - 1), 
                new Position(position.X, position.Y + 1)
            ];
            foreach (var pos in potentialSpots)
            {
                if (Grid.Get(pos) is Dirt)
                {
                    Grid.Set(pos, new WormCocoon());
                    break;
                }
            }
        });
    }
    
    var directions = new List<Position>
    {
        new(position.X, position.Y + 1),
        new(position.X, position.Y - 1), 
        new(position.X + 1, position.Y), 
        new(position.X - 1, position.Y)  
    };
    
    var possibleMoves = new List<Position>();
    foreach (var target in directions)
    {
        if (Grid.IsInBounds(target) && Grid.Get(target) is Dirt dirt)
        {
            if (dirt.Dampness < 0.85f || isStuck)
            {
                possibleMoves.Add(target);
            }
        }
    }
    
    if (possibleMoves.Any())
    {
        isStuck = false;
        
        var newPosition = possibleMoves[rand.Next(possibleMoves.Count)];
        return Move(position, newPosition);
    }
    
    isStuck = true;
    return null;
}
    
    private Action Move(Position oldHeadPosition, Position newHeadPosition)
    {
        return () =>
        {
            if (Grid.Get(newHeadPosition) is Dirt dirt)
                dirt.Nutrients = 1f;
            
            Grid.Move(oldHeadPosition, newHeadPosition);
            
            var previousSegmentOldPos = oldHeadPosition;
            for (int i = 0; i < positions.Length; i++)
            {
                var currentSegmentOldPos = positions[i];
                Grid.Move(currentSegmentOldPos, previousSegmentOldPos);
                positions[i] = previousSegmentOldPos;
                previousSegmentOldPos = currentSegmentOldPos;
            }
        };
    }

    public static void Spawn(Position position, int length)
    {
        var worm = new Worm { Main = true };
        Grid.Set(position, worm);
     
        var tailPositions = new List<Position>();
        for (int i = 1; i < length; i++)
        {
            var tailPos = new Position(position.X - i, position.Y);
            Grid.Set(tailPos, new Worm());
            tailPositions.Add(tailPos);
        }
        worm.positions = tailPositions.ToArray();
    }

    public string GetColor(Position position) => Main ? "#994170" : "#ce5a98";
    public char Symbol { get; } = '#';
}