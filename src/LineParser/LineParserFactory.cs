using BizObjects;
using LogObjects;
using System;

namespace LineParser
{
    public class LineParserFactory
    {
        // If we parse a Zone object, save that info here
        private static Zone CurrentZone = null;

        public Line ParseLine(LogDatum logLine)
        {
            return new Unknown(logLine, CurrentZone);
        }
    }
}
