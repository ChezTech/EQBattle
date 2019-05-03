using BizObjects;
using BizObjects.Parsers;
using LogObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LineParser
{
    public class LineParserFactory
    {
        // If we parse a Zone object, save that info here
        private static Zone CurrentZone = null;

        private IList<IParser> _parsers = new List<IParser>();

        private Publisher _publisher;

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
            // Need to figure out what to do if there are multiple parsers that can parse a line.
            // Should we take the first, a priority number?
            // I would like to automatically adjust the order of the parsers such that the ones that usually parse a line are evaluated first (assuming evaluation stops with the first match)

            IList<Line> parsedLines = new List<Line>();
            foreach (var parser in _parsers)
            {
                if (parser.TryParse(logLine, out Line lineEntry))
                    parsedLines.Add(lineEntry);
            }

            return parsedLines
                .DefaultIfEmpty(new Unknown(logLine, CurrentZone))
                .FirstOrDefault();
        }

        public void AddParser(IParser parser)
        {
            _parsers.Add(parser);
        }
    }
}
