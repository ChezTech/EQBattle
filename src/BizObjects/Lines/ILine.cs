using System;
using LogObjects;

namespace BizObjects
{
    public interface ILine
    {
        DateTime Time { get; }
        LogDatum LogLine { get; }
    }
}
