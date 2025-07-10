using System.Diagnostics;
using System.Threading;

namespace Mushroom;

public static class AccurateWait
{
    public static void Wait(double milliseconds)
    {
        if (milliseconds <= 0) return;

        var stopwatch = Stopwatch.StartNew();

        // Порог, ниже которого мы используем активное ожидание.
        // 15-16 мс - это типичная точность системного таймера Windows.
        // Ждать через Sleep() меньшие интервалы бессмысленно.
        const double sleepThresholdMs = 16; 

        while (stopwatch.Elapsed.TotalMilliseconds < milliseconds)
        {
            double remainingMs = milliseconds - stopwatch.Elapsed.TotalMilliseconds;

            // Если осталось много времени, отдаем управление ОС.
            // Это эффективно и не грузит процессор.
            if (remainingMs > sleepThresholdMs)
            {
                Thread.Sleep(1); 
            }
            else
            {
                // Для последних миллисекунд используем SpinWait для максимальной точности.
                // Это немного нагрузит ЦП, но лишь на очень короткое время.
                Thread.SpinWait(100); 
            }
        }
    }
}