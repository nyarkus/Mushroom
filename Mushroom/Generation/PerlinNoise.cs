namespace Mushroom.Generation;

using System;

public class PerlinNoise
{
    // Размер таблицы перестановок
    private const int TableSize = 256;
    private const int TableMask = TableSize - 1;

    // Таблица перестановок (основа "псевдослучайности")
    private readonly int[] _p;
    
    // Градиенты для 1D шума (просто -1 и 1)
    private readonly float[] _gradients = { 1f, -1f };

    /// <summary>
    /// Создает генератор шума с указанным сидом (seed).
    /// </summary>
    public PerlinNoise(int seed)
    {
        var rand = new Random(seed);
        
        // Создаем временный массив [0, 1, 2, ..., 255]
        var permutation = new int[TableSize];
        for (int i = 0; i < TableSize; i++)
        {
            permutation[i] = i;
        }

        // Перемешиваем его (алгоритм Фишера-Йетса)
        for (int i = TableSize - 1; i > 0; i--)
        {
            int j = rand.Next(i + 1);
            (permutation[i], permutation[j]) = (permutation[j], permutation[i]);
        }

        // Удваиваем таблицу перестановок, чтобы избежать проверок на выход за границы
        // Это стандартный трюк для ускорения
        _p = new int[TableSize * 2];
        for (int i = 0; i < TableSize; i++)
        {
            _p[i] = _p[i + TableSize] = permutation[i];
        }
    }

    /// <summary>
    /// Функция сглаживания (quintic curve) для устранения резких переходов.
    /// </summary>
    private static float Fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    /// <summary>
    /// Линейная интерполяция.
    /// </summary>
    private static float Lerp(float a, float b, float t)
    {
        return a + t * (b - a);
    }
    
    private float GetGradient(int hash, float dist)
    {
        // Используем хэш, чтобы выбрать один из двух градиентов
        return _gradients[hash & 1] * dist;
    }

    /// <summary>
    /// Генерирует 1D шум Перлина для заданной координаты.
    /// Возвращает значение в диапазоне примерно [-0.7, 0.7].
    /// </summary>
    public float GetNoise(float x)
    {
        // Находим целочисленную и дробную части координаты
        int x0 = (int)Math.Floor(x);
        float xf = x - x0;

        // Обертываем целочисленную часть, чтобы она всегда была в пределах [0, 255]
        int ix0 = x0 & TableMask;
        int ix1 = (ix0 + 1) & TableMask;

        // Получаем "случайные" градиенты для левой и правой точек сетки
        int g0 = _p[ix0];
        int g1 = _p[ix1];

        // Вычисляем влияние каждого градиента
        // Расстояние от точки до левой границы - это xf
        // Расстояние от точки до правой границы - это xf - 1
        float n0 = GetGradient(g0, xf);
        float n1 = GetGradient(g1, xf - 1);
        
        // Сглаживаем дробную часть
        float u = Fade(xf);

        // Интерполируем между двумя значениями и возвращаем результат
        return Lerp(n0, n1, u) * 1.4f; // Умножаем на ~1.4, чтобы приблизить диапазон к [-1, 1]
    }
}