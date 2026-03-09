using System.Diagnostics;
using System.Threading;

namespace Mushroom;

public static class AccurateWait
{
    public static void Wait(double milliseconds)
    {
        if (milliseconds <= 0) return;

        var stopwatch = Stopwatch.StartNew();
        
        const double sleepThresholdMs = 16; 

        while (stopwatch.Elapsed.TotalMilliseconds < milliseconds)
        {
            double remainingMs = milliseconds - stopwatch.Elapsed.TotalMilliseconds;
            
            if (remainingMs > sleepThresholdMs)
                Thread.Sleep(1); 
            else
                Thread.SpinWait(100); 
        }
    }
}