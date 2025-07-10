using System;
using System.Collections.Generic;
using System.Globalization;
using Mushroom.Data; // Предполагается, что здесь Position, Grid, Direction и т.д.

namespace Mushroom.Ceils;

public class Dirt : ICell
{
    // --- Константы для гибкой настройки симуляции ---
    private const float MAX_DAMPNESS = 1.0f;
    private const float MIN_DAMPNESS = 0.0f;
    private const float EVAPORATION_RATE = 0.005f; // Скорость испарения в воздух
    private const float GRAVITY_FLOW_RATE = 0.2f;  // Скорость протекания вниз (выравнивание)
    private const float HORIZONTAL_FLOW_RATE = 0.1f; // Скорость растекания в стороны

    // --- Цвета для градиента ---
    private static readonly (byte R, byte G, byte B) DRY_COLOR = (0xA0, 0x82, 0x61); // #a08261
    private static readonly (byte R, byte G, byte B) WET_COLOR = (0x38, 0x25, 0x11); // #382511

    public float Dampness { get; set; } = 0.9f;
    public float Nutrients { get; set; } = 1.0f;
    
    public char Symbol { get; } = '#';

    public Action? Do(Position position)
    {
        // Список изменений, которые нужно применить к соседям.
        // Мы не можем изменять соседей напрямую на этапе расчета,
        // чтобы не повлиять на их собственный расчет в этом же тике.
        var pendingChanges = new List<(ICell cell, float dampnessDelta)>();
        
        float currentDampnessChange = 0;

        // 1. Испарение (если сверху воздух)
        var up = Grid.GetNeighbor(position, Direction.Up);
        if (up is Air && Dampness > MIN_DAMPNESS)
        {
            currentDampnessChange -= EVAPORATION_RATE;
        }

        // 2. Гравитация (протекание вниз)
        var down = Grid.GetNeighbor(position, Direction.Down);
        if (down is Dirt dirtDown && Dampness > dirtDown.Dampness)
        {
            // Рассчитываем количество влаги для выравнивания уровней
            float amountToFlow = (Dampness - dirtDown.Dampness) / 2 * GRAVITY_FLOW_RATE;
            currentDampnessChange -= amountToFlow;
            pendingChanges.Add((dirtDown, amountToFlow));
        }
        
        // 3. Горизонтальное растекание (влево и вправо)
        // Обрабатываем только соседей справа, чтобы избежать двойного расчета (A->B и B->A)
        var right = Grid.GetNeighbor(position, Direction.Right);
        if (right is Dirt dirtRight && Dampness > dirtRight.Dampness)
        {
            float amountToFlow = (Dampness - dirtRight.Dampness) / 2 * HORIZONTAL_FLOW_RATE;
            currentDampnessChange -= amountToFlow;
            pendingChanges.Add((dirtRight, amountToFlow));
        }

        var left = Grid.GetNeighbor(position, Direction.Left);
        if (left is Dirt dirtLeft && Dampness < dirtLeft.Dampness)
        {
             // А здесь, наоборот, забираем влагу от более мокрого соседа слева
             // Это вторая часть симметричного расчета
            float amountToFlow = (dirtLeft.Dampness - Dampness) / 2 * HORIZONTAL_FLOW_RATE;
            currentDampnessChange += amountToFlow;
            // Действие по отъему влаги будет сгенерировано левой клеткой
        }


        // Если есть какие-либо изменения, создаем одно общее действие
        if (Math.Abs(currentDampnessChange) > 0.0001f || pendingChanges.Count > 0)
        {
            return new Action(() =>
            {
                // Применяем изменения к себе
                Dampness += currentDampnessChange;

                // Применяем отложенные изменения к соседям
                foreach (var change in pendingChanges)
                {
                    if (change.cell is Dirt targetDirt)
                    {
                        targetDirt.Dampness += change.dampnessDelta;
                    }
                }

                // Ограничиваем влажность в пределах [0, 1]
                Dampness = Math.Clamp(Dampness, MIN_DAMPNESS, MAX_DAMPNESS);
            });
        }
        
        return null;
    }

    public string GetColor(Position position)
    {
        // Нормализуем влажность от 0 до 1, чтобы использовать как коэффициент для интерполяции
        float t = Math.Clamp(Dampness / MAX_DAMPNESS, 0.0f, 1.0f);
        return LerpHexColor(DRY_COLOR, WET_COLOR, t);
    }
    
    /// <summary>
    /// Линейно интерполирует между двумя цветами.
    /// </summary>
    /// <param name="colorA">Начальный цвет (кортеж R, G, B).</param>
    /// <param name="colorB">Конечный цвет (кортеж R, G, B).</param>
    /// <param name="t">Коэффициент интерполяции (от 0 до 1).</param>
    /// <returns>Цвет в формате HEX-строки (#RRGGBB).</returns>
    private static string LerpHexColor((byte R, byte G, byte B) colorA, (byte R, byte G, byte B) colorB, float t)
    {
        byte r = (byte)(colorA.R + (colorB.R - colorA.R) * t);
        byte g = (byte)(colorA.G + (colorB.G - colorA.G) * t);
        byte b = (byte)(colorA.B + (colorB.B - colorA.B) * t);
        return $"#{r:X2}{g:X2}{b:X2}";
    }
}