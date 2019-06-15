using LogObjects;
using System;

namespace BizObjects.Lines
{
    public abstract class Line : ILine
    {
        public DateTime Time { get { return LogLine.LogTime; } }
        public LogDatum LogLine { get; }
        public Zone Zone { get; }

        public Line(LogDatum logLine, Zone zone = null)
        {
            LogLine = logLine;
            Zone = zone;
        }
    }
}
