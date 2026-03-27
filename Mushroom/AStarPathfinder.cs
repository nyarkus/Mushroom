using System;
using System.Collections.Generic;
using Godot;
using Mushroom.Ceils;

namespace Mushroom;

public static class AStarPathfinder
{
    private static readonly Vector2I[] Directions =
    [
        Vector2I.Up, Vector2I.Down, Vector2I.Left, Vector2I.Right
    ];
    
    public static List<Vector2I>? FindPath(
        Vector2I start, 
        Vector2I target, 
        int maxIterations = 1000, 
        params Type[] walkableTypes)
    {
        if (start == target) 
            return new List<Vector2I>();
        
        var openQueue = new PriorityQueue<Vector2I, int>();
        var nodes = new Dictionary<Vector2I, PathNode>();
        var closedSet = new HashSet<Vector2I>();
        
        var startNode = new PathNode(start)
        {
            G = 0,
            H = GetManhattanDistance(start, target)
        };
        nodes[start] = startNode;
        openQueue.Enqueue(start, startNode.F);

        int iterations = 0;

        while (openQueue.Count > 0 && iterations < maxIterations)
        {
            iterations++;
            Vector2I currentPos = openQueue.Dequeue();
            
            if (closedSet.Contains(currentPos)) continue;
            closedSet.Add(currentPos);
            
            if (currentPos == target)
                return RetracePath(nodes, start, target);
            
            foreach (var dir in Directions)
            {
                Vector2I neighborPos = currentPos + dir;

                if (!Grid.IsInBounds(neighborPos)) continue;
                if (closedSet.Contains(neighborPos)) continue;
                
                if (neighborPos != target && !IsWalkable(neighborPos, walkableTypes))
                    continue;
                
                int tentativeG = nodes[currentPos].G + 1;

                if (!nodes.TryGetValue(neighborPos, out PathNode neighborNode))
                {
                    neighborNode = new PathNode(neighborPos);
                    nodes[neighborPos] = neighborNode;
                }
                else if (tentativeG >= neighborNode.G)
                    continue;
                
                neighborNode.Parent = currentPos;
                neighborNode.G = tentativeG;
                neighborNode.H = GetManhattanDistance(neighborPos, target);
                
                openQueue.Enqueue(neighborPos, neighborNode.F);
            }
        }
        
        return null;
    }
    
    private static List<Vector2I> RetracePath(Dictionary<Vector2I, PathNode> nodes, Vector2I start, Vector2I target)
    {
        var path = new List<Vector2I>();
        Vector2I current = target;

        while (current != start)
        {
            path.Add(current);
            current = nodes[current].Parent;
        }

        path.Reverse();
        return path;
    }
    
    private static int GetManhattanDistance(Vector2I a, Vector2I b)
        => Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);
    
    
    private static bool IsWalkable(Vector2I pos, Type[] walkableTypes)
    {
        var cell = Grid.Get(pos);
        
        if (cell is Air) return true;
        
        if (walkableTypes != null && walkableTypes.Length > 0)
        {
            Type cellType = cell.GetType();
            for (int i = 0; i < walkableTypes.Length; i++)
            {
                if (cellType == walkableTypes[i]) return true;
            }
        }

        return false;
    }
    
    private class PathNode
    {
        public Vector2I Position { get; }
        public Vector2I Parent { get; set; }
        
        public int G { get; set; }
        public int H { get; set; }
        public int F => G + H;

        public PathNode(Vector2I position)
        {
            Position = position;
        }
    }
}