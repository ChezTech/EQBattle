using System.Collections.Generic;
using System.Linq;

namespace BizObjects
{
    public class CountStatistics<T> where T : class, ILine
    {
        public CountStatistics(IEnumerable<ILine> lines)
        {
            // This is a lazy evaluation, which means it'll be re-evaluated each time it's called, which is what we want.
            Lines = lines.Where(x => x is T).Select(x => x as T);
        }
        public int Count { get => Lines.Count(); }
        public IEnumerable<T> Lines { get; }
    }
}
