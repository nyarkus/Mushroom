using System;
using Godot;
using Mushroom.Ceils;
using Mushroom.Data;

namespace Mushroom;

public static class Raycast
{
    public static RaycastResult Cast(Vector2I from, Vector2I direction, int maxDistance = 1000)
    {
        GD.Print("Raycast call");
        for (int i = 0; i < maxDistance; i++)
        {
            Vector2I position = from + direction * i;
            if (!Grid.IsInBounds(position))
                return new RaycastResult(false, default, default);
            

            CellBase result = Grid.Get(position);
            if (result is not Air)
                return new RaycastResult(true, position, result);
        }
        
        return new RaycastResult(false, default, default);
    }
    
    public static RaycastResult Cast(Vector2I from, Vector2I direction, int maxDistance, params ReadOnlySpan<Type> exclude)
    {
        GD.Print("Raycast2 call");
        for (int i = 0; i < maxDistance; i++)
        {
            Vector2I position = from + direction * i;
            if (!Grid.IsInBounds(position))
                return new RaycastResult(false, default, default);

            CellBase result = Grid.Get(position);

            // ReadOnlySpan don't have LINQ :/
            bool skip = false;
            foreach (var type in exclude)
            {
                if (result.GetType() == type)
                {
                    skip = true;
                    break;
                }
            }
            if(skip) continue;
            
            return new RaycastResult(true, position, result);
        }
        
        return new RaycastResult(false, default, default);
    }
}

public record RaycastResult(bool IsHit, Vector2I Position, CellBase Cell);