using System;
using System.Collections.Generic;
using System.Linq;

namespace BizObjects
{
    public class HitPointStatistics<T> : CountStatistics<T> where T : class
    {
        public HitPointStatistics(IList<ILine> lines, Func<T, int> valueFunc) : base(lines)
        {
            ValueFunc = valueFunc;
        }

        public int Total { get => Lines.Where(x => x is T).Select(x => x as T).Sum(x => ValueFunc(x)); }
        public int Min { get => Lines.Where(x => x is T).Select(x => x as T).Min(x => ValueFunc(x)); }
        public int Max { get => Lines.Where(x => x is T).Select(x => x as T).Max(x => ValueFunc(x)); }
        public double Average { get => Lines.Where(x => x is T).Select(x => x as T).Average(x => ValueFunc(x)); }
        public Func<T, int> ValueFunc { get; }
    }
}
