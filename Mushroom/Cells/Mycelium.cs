using System;
using System.Collections.Generic;
using Mushroom.Data;
using Godot;

namespace Mushroom.Ceils;

public class Mycelium : CellBase
{
    public float Water { get; set; } = 100f;
    public float Energy { get; set; } = 100f;
    public bool Main { get; set; }
    private int dies = 0;
    public Vector2I MainPosition { get; set; }
    
    public float _maxDistance { get; }= new Vector2I(1,0).DistanceSquaredTo(new Vector2I(5,0));
    
    public override Action? Do(Vector2I vector2)
    {
        float nextEnergy = Energy;
        float nextWater = Water;
        int nextDies = dies;
        
        if (Main)
        {
            if (Energy > 0.42f && Water > 0.47f)
            {
                if (Grid.GetNeighbor(vector2, Direction.Down) is Dirt)
                {
                    return () =>
                    {
                        Energy -= 0.42f;
                        Water -= 0.47f;
                        Grid.Set(new Vector2I(vector2.X, vector2.Y + 1), new Mycelium()
                        {
                            Energy = 0.2f, Water = 0.25f, MainPosition = vector2
                        });
                    };
                }   
            }
            
            var neighbor = Grid.GetNeighbor(vector2, Direction.Up);
            if (neighbor is Air && Energy > 0.8f && Water > 0.85f)
            {
                var targetY = Grid.GroundLevel - Random.Shared.Next(2, 6);
                return () =>
                {
                    Grid.Set(vector2.X, vector2.Y - 1, new Stalk() { TargetY = targetY });
                    Water -= 0.65f;
                    Energy -= 0.35f;
                };
            }
            else if (neighbor is Stalk stalk)
            {
                if ((stalk.Energy < 0.7f || stalk.Water < 0.8f) && (Water > 10f && Energy > 10f))
                {
                    return () =>
                    {
                        stalk.Energy += 0.2f;
                        stalk.Water += 0.2f;
                        Water -= 0.2f;
                        Energy -= 0.2f;
                    };
                }
            }
        }

        if (nextEnergy > 0.0005f) 
            nextEnergy -= 0.0005f; 
        else nextDies++;
        if (nextWater > 0.0001f) 
            nextWater -= 0.0001f; 
        else nextDies++;

        if (nextDies > 0 && nextEnergy > 0.015f && nextWater > 0.025f)
        {
            nextEnergy -= 0.015f; 
            nextWater -= 0.025f; 
            nextDies--;
        }

        if (nextDies >= 100)
            return () => 
                Grid.Set(vector2, new RottingMatter());

        var down = Grid.GetNeighbor(vector2, Direction.Down);
        var up = Grid.GetNeighbor(vector2, Direction.Up);
        var left = Grid.GetNeighbor(vector2, Direction.Left);
        var right = Grid.GetNeighbor(vector2, Direction.Right);
        
        Mycelium mUp = up as Mycelium;
        Mycelium mDown = down as Mycelium;
        Mycelium mLeft = left as Mycelium;
        Mycelium mRight = right as Mycelium;

        if (!Main)
        {
            if (vector2.DistanceSquaredTo(MainPosition) < _maxDistance 
                && nextEnergy > 0.85f && nextWater > 0.95f 
                && down is not Mycelium)
            {
                var direction = (Direction)Random.Shared.Next(0, 4); 
                Vector2I neighborVector2 = vector2.GetNeighborPosition(direction);
                
                if (Grid.Get(neighborVector2) is Dirt)
                {
                    nextEnergy -= 0.75f;
                    nextWater -= 0.85f;
                    
                    return () =>
                    {
                        Energy = nextEnergy; 
                        Water = nextWater; 
                        dies = nextDies;
                        
                        ProcessDirt(down as Dirt); 
                        ProcessDirt(up as Dirt);
                        ProcessDirt(left as Dirt); 
                        ProcessDirt(right as Dirt);
                        
                        Grid.Set(neighborVector2, new Mycelium()
                        {
                            Energy = 0.5f, Water = 0.25f, MainPosition = MainPosition
                        });
                    };
                }
            }

            if (nextEnergy > 0.3f && nextWater > 0.3f)
            {
                int mCount = (mUp != null ? 1 : 0) + (mDown != null ? 1 : 0) + 
                             (mLeft != null ? 1 : 0) + (mRight != null ? 1 : 0);

                if (mCount > 0)
                {
                    nextEnergy -= 0.1f;
                    nextWater -= 0.1f;
                    
                    return () =>
                    {
                        Energy = nextEnergy; 
                        Water = nextWater; 
                        dies = nextDies;
                        
                        ProcessDirt(down as Dirt); 
                        ProcessDirt(up as Dirt);
                        ProcessDirt(left as Dirt); 
                        ProcessDirt(right as Dirt);
                        
                        int target = Random.Shared.Next(0, mCount);
                        Mycelium selected = null;
                        
                        if (mUp != null && target-- == 0) selected = mUp;
                        else if (mDown != null && target-- == 0) selected = mDown;
                        else if (mLeft != null && target-- == 0) selected = mLeft;
                        else if (mRight != null && target-- == 0) selected = mRight;

                        if (selected != null)
                        {
                            selected.Energy += 0.1f;
                            selected.Water += 0.1f;
                        }
                    };
                }
            }
        }
        

        return () =>
        {
            Energy = nextEnergy; 
            Water = nextWater; 
            dies = nextDies;
            
            ProcessDirt(down as Dirt); 
            ProcessDirt(up as Dirt);
            ProcessDirt(left as Dirt); 
            ProcessDirt(right as Dirt);
        };
    }
    
    private void ProcessDirt(Dirt dirt)
    {
        if (dirt == null) return;

        if (dirt.Dampness >= 0.05f)
        {
            dirt.Dampness -= 0.05f;
            Water += 0.2f;  
        }
        if (dirt.Nutrients >= 0.05f)
        {
            dirt.Nutrients -= 0.05f;
            Energy += 0.15f;
        }
    }

    public override Color GetColor(Vector2I vector2)
    {
        if (Energy == 0)
            return new Color(0.39f, 0.39f, 0.39f);
        if (Energy > 0.1 && Energy < 0.3)
            return new Color(0.56f, 0.56f, 0.56f);
        if (Energy > 0.3 && Energy < 0.6)
            return new Color(0.72f, 0.72f, 0.72f);

        return new Color(0.81f, 0.81f, 0.81f);
    }

    public char Symbol { get; } = '#';
}