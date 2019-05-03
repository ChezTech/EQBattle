using BizObjects;
using BizObjects.Parsers;
using LogObjects;
using System;

namespace LineParser
{
    public class LineParserFactory
    {
        // If we parse a Zone object, save that info here
        private static Zone CurrentZone = null;
        private Publisher _publisher;

        private KillParser _killParser = new KillParser();

        public LineParserFactory(Publisher publisher)
        {
            _publisher = publisher;
        }

        public Line ParseLine(LogDatum logLine)
        {
            var lineEntry = FigureOutLineDatum(logLine);
            _publisher.RaiseCreated((dynamic)lineEntry);

            return lineEntry;
        }

        private Line FigureOutLineDatum(LogDatum logLine)
        {
            Line lineEntry;
            if (_killParser.TryParse(logLine, out lineEntry))
                return lineEntry;

            return new Unknown(logLine, CurrentZone);
        }
    }
}
