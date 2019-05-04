using BizObjects;
using BizObjects.Parsers;
using LogObjects;
using System;
using System.Collections.Generic;

namespace LineParser
{
    public class LineParserFactory
    {
        // If we parse a Zone object, save that info here
        private static Zone CurrentZone = null;

        private class ParserAction
        {
            public IParser Parser { get; set; }
            public Action<ILine> OnCreated { get; set; }
        }

        private IList<ParserAction> _parsers = new List<ParserAction>();

        public event Action<Unknown> UnknownCreated;

        public ILine ParseLine(LogDatum logLine)
        {
            // Need to figure out what to do if there are multiple parsers that can parse a line.
            // Should we take the first, a priority number?
            // I would like to automatically adjust the order of the parsers such that the ones that usually parse a line are evaluated first (assuming evaluation stops with the first match)

            // We're just taking the first valid parsed line here
            foreach (var parser in _parsers)
            {
                if (parser.Parser.TryParse(logLine, out ILine lineEntry))
                {
                    parser.OnCreated(lineEntry);
                    return lineEntry;
                }
            }

            var unknownLine = new Unknown(logLine, CurrentZone);
            UnknownCreated?.Invoke(unknownLine);
            return unknownLine;
        }

        public void AddParser(IParser parser, Action<ILine> createAction)
        {
            _parsers.Add(new ParserAction() {Parser = parser, OnCreated = createAction });
        }
    }
}
