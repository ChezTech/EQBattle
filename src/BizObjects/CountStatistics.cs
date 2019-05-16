using System.Collections.Generic;
using System.Linq;

namespace BizObjects
{
    public class CountStatistics<T>
    {
        public CountStatistics(IList<ILine> lines)
        {
            Lines = lines;
        }
        public int Count { get => Lines.Where(x => x is T).Count(); }
        public IList<ILine> Lines { get; }
    }
}
