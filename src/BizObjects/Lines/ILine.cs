using System;
using LogObjects;

namespace BizObjects.Lines
{
    public interface ILine
    {
        DateTime Time { get; }
        LogDatum LogLine { get; }
    }
}
