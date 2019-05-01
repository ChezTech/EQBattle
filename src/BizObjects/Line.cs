
using LogObjects;
using System;

namespace BizObjects
{
    public abstract class Line
    {
        public DateTime Time { get { return LogLine.LogTime; } }
        public LogDatum LogLine { get; private set; }
        public Zone Zone { get; private set; }

        public Line(LogDatum logLine, Zone zone = null)
        {
            LogLine = logLine;
            Zone = zone;
        }
    }
}
