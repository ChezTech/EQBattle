using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Core.Extensions
{
    public static class MemoizeExtension
    {
        public static Func<T, TResult> Memoize<T, TResult>(this Func<T, TResult> f)
        {
            var cache = new ConcurrentDictionary<T, TResult>();
            return a => cache.GetOrAdd(a, f);
        }
    }
}
