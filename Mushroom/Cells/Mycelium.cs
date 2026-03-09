using System;
using Mushroom.Data;
using Godot;

namespace Mushroom.Ceils;

public class Mycelium : ICell
{
    public float Water { get; set; } = 100f;
    public float Energy { get; set; } = 100f;
    public bool Main { get; set; }
    private int dies = 0;
    public Vector2I MainVector2 { get; set; }
    
    public float _maxDistance { get; }= new Vector2I(1,0).DistanceSquaredTo(new Vector2I(5,0));
    private Random rand = new();
    public Action? Do(Vector2I vector2)
    {
        if (Main)
        {
            if (Energy > 0.42f && Water > 0.47f)
            {
                if (Grid.GetNeighbor(vector2, Direction.Down) is Dirt)
                {
                    return new Action(() =>
                    {
                        Energy -= 0.42f;
                        Water -= 0.47f;
                        Grid.Set(new Vector2I(vector2.X, vector2.Y + 1), new Mycelium()
                        {
                            Energy = 0.2f,
                            Water = 0.25f,
                            MainVector2 = vector2
                        });
                    });
                }   
            }
            var neighbor = Grid.GetNeighbor(vector2, Direction.Up);

            if (neighbor is Air && Energy > 0.8f && Water > 0.85f)
            {
                var targetY = Grid.GroundLevel - rand.Next(2, 6);
                return new Action(() =>
                {
                    Grid.Set(vector2.X, vector2.Y - 1, new Stalk() { TargetY = targetY });

                    Water -= 0.65f;
                    Energy -= 0.35f;
                });
            }
            else if (neighbor is Stalk stalk)
            {
                if ((stalk.Energy < 0.7f || stalk.Water < 0.8f) && (Water > 10f && Energy > 10f))
                {
                    return new Action(() =>
                    {
                        stalk.Energy += 0.2f;
                        stalk.Water += 0.2f;

                        Water -= 0.2f;
                        Energy -= 0.2f;
                    });
                }
            }
                
            
        }

        if (Energy > 0.0005f)
            Energy -= 0.0005f;
        else
            dies++;
        if (Water > 0.0001f)
            Water -= 0.0001f;
        else
            dies++;

        if (dies > 0 && Energy > 0.015f && Water > 0.025f)
        {
            Energy -= 0.015f;
            Water -= 0.025f;
            dies--;
        }

        if (dies >= 100)
            return new Action(() =>
            {
                Grid.Set(vector2, new Dirt() { Dampness = 0.5f, Nutrients = 0.5f } );
            });
        
        var down = Grid.GetNeighbor(vector2, Direction.Down);
        var up = Grid.GetNeighbor(vector2, Direction.Up);
        var left = Grid.GetNeighbor(vector2, Direction.Left);
        var right = Grid.GetNeighbor(vector2, Direction.Right);

        if (down is Dirt dirtDown)
            Dirt(dirtDown);
        if (up is Dirt dirtUp)
            Dirt(dirtUp);
        if (left is Dirt dirtLeft)
            Dirt(dirtLeft);
        if (right is Dirt dirtRight)
            Dirt(dirtRight);

        if (!Main)
        {
            if (vector2.DistanceSquaredTo(MainVector2) < _maxDistance 
                && Energy > 0.85f && Water > 0.95f 
                && Grid.GetNeighbor(vector2, Direction.Down) is not Mycelium)
            {
                var direction = (Direction)rand.Next(0, 4); 
                
                Vector2I neighborVector2 = vector2.GetNeighborPosition(direction);
                
                var neighbor = Grid.Get(neighborVector2);

                if (neighbor is Dirt)
                {
                    return new Action(() =>
                    {
                        Energy -= 0.75f;
                        Water -= 0.85f;
                        Grid.Set(neighborVector2, new Mycelium()
                        {
                            Energy = 0.5f,
                            Water = 0.25f,
                            MainVector2 = this.MainVector2
                        });
                    });
                }
            }

            if (Energy > 0.6f && Water > 0.95f)
            {
                Mycelium mycelium;
                while (true)
                {
                    var direction = (Direction)rand.Next(0, 5);
                    var neighbor = Grid.GetNeighbor(vector2, direction);

                    if (neighbor is Mycelium m)
                    {
                        mycelium = m;
                        break;
                    }
                }

                mycelium.Energy += 0.1f;
                mycelium.Water += 0.1f;
                
                Energy -= 0.1f;
                Water -= 0.1f;
            }
        }

        return null;
    }

    private void Dirt(Dirt dirt)
    {
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

    public string GetColor(Vector2I vector2)
    {
        if (Energy == 0)
            return "#636363";
        if (Energy > 0.1 && Energy < 0.3)
            return "#8e8e8e";
        if (Energy > 0.3 && Energy < 0.6)
            return "#b7b7b7";

        return "#cecece";
    }

    public char Symbol { get; } = '#';
}