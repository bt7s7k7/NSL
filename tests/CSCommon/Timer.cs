using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CSCommon
{
    public class Timer
    {
        protected List<double> times = new List<double>();
        protected Stopwatch stopwatch = new Stopwatch();

        public void Start()
        {
            stopwatch.Restart();
        }

        public void End()
        {
            stopwatch.Stop();
            times.Add(stopwatch.Elapsed.TotalMilliseconds);
        }

        public void Reset()
        {
            times.Clear();
        }

        protected double Max() => times.Max();
        protected double Min() => times.Min();
        protected double Avg() => times.Average();
        protected double Med()
        {
            var count = times.Count();
            var orderedPersons = times.OrderBy(p => p);
            var median = (double)orderedPersons.ElementAt(count / 2) + (double)orderedPersons.ElementAt((count - 1) / 2);
            median /= 2;

            return median;
        }

        override public string ToString()
        {
            return String.Format("MAX: {0:N2}ms MED: {1:N2}ms AVG: {2:N2}ms MIN: {3:N2}ms", Max(), Med(), Avg(), Min());//  $"MAX: {Max()}ms MED: {Med()}ms AVG: {Avg()}ms MIN: {Min()}ms";
        }

        public static string GetTotal(IEnumerable<Timer> timers)
        {
            var max = timers.Select(v => v.Max()).Sum();
            var min = timers.Select(v => v.Min()).Sum();
            var med = timers.Select(v => v.Med()).Sum();
            var avg = timers.Select(v => v.Avg()).Sum();

            return String.Format("MAX: {0:N2}ms MED: {1:N2}ms AVG: {2:N2}ms MIN: {3:N2}ms", max, med, avg, min);
        }

    }
}