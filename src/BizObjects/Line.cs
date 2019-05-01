
using LogObjects;
using System;

namespace BizObjects
{
    public abstract class Line
    {
        public DateTime Time { get; private set; }
        public LineDatum LogLine { get; private set; }
    }
}
