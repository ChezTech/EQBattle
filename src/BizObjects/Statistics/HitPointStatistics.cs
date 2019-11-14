using BizObjects.Lines;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BizObjects.Statistics
{
    public class HitPointStatistics<T> : CountStatistics<T> where T : class, ILine
    {
        private readonly TimeSpan SixSeconds = new TimeSpan(0, 0, 6);

        public HitPointStatistics(IEnumerable<ILine> lines, Func<T, int> valueFunc) : base(lines)
        {
            ValueFunc = valueFunc;
        }

        public int Total { get => Lines.Sum(x => ValueFunc(x)); }
        public int LastSixTotal
        {
            get => Lines
                .Where(x => x.Time > Lines.LastOrDefault()?.Time - SixSeconds)
                .Sum(x => ValueFunc(x));
        }

        public int Min { get => Lines.Any() ? Lines.Min(x => ValueFunc(x)) : 0; }
        public int Max { get => Lines.Any() ? Lines.Max(x => ValueFunc(x)) : 0; }
        public double Average { get => Lines.Any() ? Lines.Average(x => ValueFunc(x)) : 0; }
        public Func<T, int> ValueFunc { get; }
    }
}
