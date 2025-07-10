using System;
using Mushroom.Data;

namespace Mushroom.Ceils;

public class Mycelium : ICell
{
    public float Water { get; set; } = 100f;
    public float Energy { get; set; } = 100f;
    public bool Main { get; set; }
    private int dies = 0;
    public Position MainPosition { get; set; }
    
    public float _maxDistance = new Position(1,1).DistanceSquared(new Position(5,5));
    private Random rand = new();
    public Action? Do(Position position)
    {
        if (Main)
        {
            if (Energy > 0.42f && Water > 0.47f)
            {
                if (Grid.GetNeighbor(position, Direction.Down) is not Mycelium)
                {
                    return new Action(() =>
                    {
                        Energy -= 0.42f;
                        Water -= 0.47f;
                        Grid.Set(new Position(position.X, position.Y + 1), new Mycelium()
                        {
                            Energy = 0.2f,
                            Water = 0.25f,
                            MainPosition = position
                        });
                    });
                }   
            }
            var neighbor = Grid.GetNeighbor(position, Direction.Up);

            if (neighbor is Air && Energy > 0.8f && Water > 0.85f)
            {
                var targetY = Grid.GroundLevel - rand.Next(2, 6);
                return new Action(() =>
                {
                    Grid.Set(position.X, position.Y - 1, new Stalk() { TargetY = targetY });

                    Water -= 0.65f;
                    Energy -= 0.35f;
                });
            }
            
            if (neighbor is Stalk stalk)
            {
                if ((stalk.Energy < 0.7f || stalk.Water < 0.8f) && (Water > 0.2f && Energy > 0.2f))
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

        if (Energy > 0)
            Energy -= 0.0001f;
        else
            dies++;
        if (Water > 0)
            Water -= 0.0001f;
        else
            dies++;

        if (dies > 0 && Energy > 0.15f && Water > 0.25f)
        {
            Energy -= 0.15f;
            Water -= 0.25f;
            dies--;
        }

        if (dies >= 100)
            return new Action(() =>
            {
                Grid.Set(position, new Dirt() { Dampness = 0f, Nutrients = 0.5f } );
            });
        
        var down = Grid.GetNeighbor(position, Direction.Down);
        var up = Grid.GetNeighbor(position, Direction.Up);
        var left = Grid.GetNeighbor(position, Direction.Left);
        var right = Grid.GetNeighbor(position, Direction.Right);

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
            // Ограничение на максимальную дистанцию от "корня" грибницы
            if (position.DistanceSquared(MainPosition) < _maxDistance && Energy > 0.85f && Water > 0.95f)
            {
                // Выбираем случайное направление. Убедитесь, что enum Direction имеет 4 значения (0-3).
                var direction = (Direction)rand.Next(0, 4); 
        
                // Определяем позицию соседа
                Position neighborPosition = position.GetNeighborPosition(direction); // Предполагая, что у вас есть такой хелпер, или вычислите вручную
        
                // Получаем соседа по вычисленной позиции
                var neighbor = Grid.Get(neighborPosition);

                if (neighbor is Dirt)
                {
                    return new Action(() =>
                    {
                        Energy -= 0.75f;
                        Water -= 0.85f;
                        // ПРАВИЛЬНО: создаем мицелий на месте соседа (земли)
                        Grid.Set(neighborPosition, new Mycelium()
                        {
                            Energy = 0.5f,
                            Water = 0.25f,
                            MainPosition = this.MainPosition // Передаем MainPosition дальше
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
                    var neighbor = Grid.GetNeighbor(position, direction);

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
        if (dirt.Dampness >= 0.1f)
        {
            dirt.Dampness -= 0.1f;
            Water += 0.1f;   
        }

        if (dirt.Nutrients >= 0.1f)
        {
            dirt.Nutrients -= 0.1f;
            Energy += 0.05f;
        }
    }

    public string GetColor(Position position)
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