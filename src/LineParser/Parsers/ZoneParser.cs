using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using BizObjects.Lines;
using LogObjects;

namespace LineParser.Parsers
{
    public class ZoneParser : IParser
    {
        private readonly Regex RxZone = new Regex("You have entered(?! the| an) (.+).", RegexOptions.Compiled); // https://regex101.com/r/ra7H0n/2

        public bool TryParse(LogDatum logDatum, out ILine lineEntry)
        {
            if (TryParseZone(logDatum, out lineEntry))
                return true;

            return false;
        }
        private bool TryParseZone(LogDatum logDatum, out ILine lineEntry)
        {
            var match = RxZone.Match(logDatum.LogMessage);

            if (!match.Success)
            {
                lineEntry = null;
                return false;
            }

            var zoneName = match.Groups[1].Value;

            lineEntry = new Zone(logDatum, zoneName);

            return true;
        }
    }
}
