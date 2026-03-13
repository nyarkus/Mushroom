using Mushroom.Data;
using System;
using System.Collections.Generic;
using Godot;

namespace Mushroom.Ceils;

public class Worm : CellBase
{
    private Vector2I[] positions = Array.Empty<Vector2I>();
    private bool canReproduce = true;
    
    public Guid WormId { get; set; } = Guid.NewGuid(); 
    public CellBase Underneath { get; set; } = Air.Instance;
    public bool Main { get; set; } 
    public int Age { get; set; }
    public int TargetLifeTime { get; set; } = 1000;
    
    public override Action? Do(Vector2I headPos)
    {
        int nextAge = Age;
        nextAge++;
        
        if (!Main)
        {
            if (Age >= TargetLifeTime)
                return () => Grid.Set(headPos, new RottingMatter());

            return () => Age = nextAge;
        }
        
        Vector2I[] nextPositions = positions;
        if (positions.Length > 0)
        {
            var validSegments = new List<Vector2I>();
            foreach (var p in positions)
            {
                if (Grid.Get(p) is Worm w && w.WormId == this.WormId) validSegments.Add(p);
                else break;
            }
            nextPositions = validSegments.ToArray();
        }
        
        if (Age >= TargetLifeTime)
            return () => { positions = nextPositions; Grid.Set(headPos, new RottingMatter()); };
        
        if (canReproduce && Age >= TargetLifeTime / 2)
        {
            return () =>
            {
                Age = nextAge;
                positions = nextPositions;
                canReproduce = false;
                
                Vector2I[] potentialSpots = 
                [
                    new Vector2I(headPos.X - 1, headPos.Y), new Vector2I(headPos.X + 1, headPos.Y), 
                    new Vector2I(headPos.X, headPos.Y - 1), new Vector2I(headPos.X, headPos.Y + 1)
                ];
                
                foreach (var pos in potentialSpots)
                {
                    if (Grid.IsInBounds(pos) && Grid.Get(pos) is Dirt)
                    {
                        Grid.Set(pos, new WormCocoon());
                        break;
                    }
                }
            };
        }
        
        var directions = new List<Vector2I>
        {
            new(headPos.X, headPos.Y + 1), new(headPos.X, headPos.Y - 1), 
            new(headPos.X + 1, headPos.Y), new(headPos.X - 1, headPos.Y)  
        };

        Vector2I bestMove = default;
        bool foundMove = false;
        float bestScore = float.MinValue;

        foreach (var target in directions)
        {
            if (Grid.IsInBounds(target))
            {
                var neighbor = Grid.Get(target);
                float score = 0f;
                bool validMove = false; 

                if (neighbor is Worm otherWorm)
                {
                    if (otherWorm.WormId == this.WormId)
                    {
                        if (nextPositions.Length > 0 && target == nextPositions[^1])
                        {
                            score = 0f; 
                            validMove = true;
                        }
                    }
                }
                else if (neighbor is Dirt dirt)
                {
                    if (dirt.Dampness > 0.85f)
                    {
                        score -= 50f; 
                        if (target.Y < headPos.Y) score += 60f; 
                    }
                    score += dirt.Nutrients * 10f;
                    score += Random.Shared.NextSingle() * 15f;   
                    validMove = true;
                }
                else if (neighbor is RottingMatter)
                {
                    score = 200f;
                    validMove = true;
                }
                else if (neighbor is Air)
                {
                    var below = Grid.GetNeighbor(target, Direction.Down);
                    if (below is Dirt || below is RottingMatter) 
                        score = 5f; 
                    else
                        continue; 
                        
                    validMove = true;
                }

                if (validMove)
                {
                    if (score > bestScore || !foundMove)
                    {
                        bestScore = score;
                        bestMove = target;
                        foundMove = true;
                    }
                }
            }
        }
        
        if (foundMove)
        {
            var moveAction = Move(headPos, bestMove);
            return () =>
            {
                Age = nextAge;
                positions = nextPositions;
                moveAction();
            };
        }
        
        if (nextPositions.Length > 0)
            return () =>
            {
                Age = nextAge;   
                positions = nextPositions;
                ReverseDirection(headPos);
            };
        
        return () => { Age = nextAge; positions = nextPositions; };
    }
    
    private Action Move(Vector2I oldHeadPos, Vector2I newHeadPos)
    {
        return () =>
        {
            var targetCell = Grid.Get(newHeadPos);
            bool bitingTail = (positions.Length > 0 && newHeadPos == positions[^1]);
            
            var headWorm = this;
            var tailWorms = new Worm[positions.Length];
            for (int i = 0; i < positions.Length; i++) 
                tailWorms[i] = Grid.Get(positions[i]) as Worm;
            
            if (bitingTail && tailWorms.Length > 0 && tailWorms[^1] != null) 
                targetCell = tailWorms[^1].Underneath;
            else if (targetCell is Worm) 
                targetCell = new Dirt() { Dampness = 0.5f, Nutrients = 0.5f };
            
            CellBase cellToRestore = (tailWorms.Length > 0 ? tailWorms[^1]?.Underneath : headWorm.Underneath) ?? new Air();
            
            if (cellToRestore is Dirt dirt) 
                dirt.Nutrients = 1f;
            else if (cellToRestore is RottingMatter rot)
            {
                if(rot.BecomeAir)
                    cellToRestore = Air.Instance;
                else
                    cellToRestore = new Dirt() { Dampness = 1f, Nutrients = 1f };
            }
            
            for (int i = tailWorms.Length - 1; i > 0; i--) 
                if (tailWorms[i] != null && tailWorms[i - 1] != null)
                    tailWorms[i].Underneath = tailWorms[i - 1].Underneath;
            
            if (tailWorms.Length > 0 && tailWorms[0] != null)
                tailWorms[0].Underneath = headWorm.Underneath;
            
            headWorm.Underneath = targetCell;
            
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    var surroundPos = new Vector2I(newHeadPos.X + x, newHeadPos.Y + y);
                    if (Grid.IsInBounds(surroundPos) && Grid.Get(surroundPos) is Dirt surroundDirt)
                        surroundDirt.Nutrients = Math.Clamp(surroundDirt.Nutrients + 0.1f, 0f, 1f);
                }
            }
            
            Grid.Move(oldHeadPos, newHeadPos);
            
            var previousPos = oldHeadPos;
            for (int i = 0; i < positions.Length; i++)
            {
                var currentPos = positions[i];
                
                if (bitingTail && i == positions.Length - 1)
                    Grid.Set(previousPos, tailWorms[i]);
                else
                    Grid.Move(currentPos, previousPos);
                    
                positions[i] = previousPos;
                previousPos = currentPos;
            }
            
            if (!bitingTail) 
                Grid.Set(previousPos, cellToRestore);
        };
    }
    
    private void ReverseDirection(Vector2I currentHeadPos)
    {
        var tailPos = positions[^1];
        
        if (Grid.Get(tailPos) is Worm tailWorm)
        {
            Main = false;
            tailWorm.Main = true;
            
            tailWorm.Age = Age;
            tailWorm.TargetLifeTime = TargetLifeTime;
            tailWorm.canReproduce = canReproduce;
            tailWorm.WormId = WormId;
            
            var newPositions = new List<Vector2I>();
            for (int i = positions.Length - 2; i >= 0; i--)
                newPositions.Add(positions[i]);
            
            newPositions.Add(currentHeadPos); 
            
            tailWorm.positions = newPositions.ToArray();
            positions = Array.Empty<Vector2I>(); 
        }
    }

    public static void Spawn(Vector2I pos, int length)
    {
        var sharedId = Guid.NewGuid();
        var worm = new Worm { Main = true, WormId = sharedId };
        
        worm.Underneath = Grid.Get(pos) ?? new Air(); 
        Grid.Set(pos, worm);
     
        var tailPositions = new List<Vector2I>();
        for (int i = 1; i < length; i++)
        {
            var tailPos = new Vector2I(pos.X - i, pos.Y);
            if (Grid.IsInBounds(tailPos)) 
            {
                var tailWorm = new Worm() { TargetLifeTime = worm.TargetLifeTime, WormId = sharedId };
                tailWorm.Underneath = Grid.Get(tailPos) ?? new Air();
                Grid.Set(tailPos, tailWorm);
                tailPositions.Add(tailPos);
            }
        }
        worm.positions = tailPositions.ToArray();
    }

    private Worm() { }
    public Worm(Vector2I position, int length)
    {
        Spawn(position, length);
    }

    public override Color GetColor(Vector2I pos) 
        => Main ? new Color(0.6f, 0.25f, 0.44f) : new Color(0.8f, 0.35f, 0.6f);

    public override Color GetUiColor()
        => new Color(0.6f, 0.25f, 0.44f);
}