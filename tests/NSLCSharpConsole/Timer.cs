using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NSLCSharpConsole
{
    public class Timer
    {
        protected List<long> times = new List<long>();
        protected Stopwatch stopwatch = new Stopwatch();

        public void Start()
        {
            stopwatch.Restart();
        }

        public void End()
        {
            stopwatch.Stop();
            times.Add(stopwatch.ElapsedMilliseconds);
        }

        override public string ToString()
        {
            var max = times.Max();
            var min = times.Min();
            var avg = times.Average();

            return $"MAX: {max}ms AVG: {avg}ms MIN: {min}ms";
        }

    }
}