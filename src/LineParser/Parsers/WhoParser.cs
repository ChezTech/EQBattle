using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using BizObjects;
using LogObjects;

namespace LineParser.Parsers
{
    public class WhoParser : IParser
    {
        private readonly Regex RxWho = new Regex(@"(?:\[(ANONYMOUS)\] (.+) <(.+)>|\[(\d+) (.+) \((.+)\)\] (.+) \((.+)\) <(.+)> ZONE: (.+))", RegexOptions.Compiled); // https://regex101.com/r/Cp71GF/4

        private readonly YouResolver YouAre;

        public WhoParser(YouResolver youAre)
        {
            YouAre = youAre;
        }

        public bool TryParse(LogDatum logDatum, out ILine lineEntry)
        {
            if (TryParseWho(logDatum, out lineEntry))
                return true;

            return false;
        }

        private bool TryParseWho(LogDatum logDatum, out ILine lineEntry)
        {
            var match = RxWho.Match(logDatum.LogMessage);

            if (!match.Success)
            {
                lineEntry = null;
                return false;
            }

            // These first 3 groups are for the ANONYMOUS character
            var isAnon = match.Groups[1].Success ? true : false;
            if (isAnon)
            {
                var name = match.Groups[2].Success ? match.Groups[2].Value : null;
                var guild = match.Groups[3].Success ? match.Groups[3].Value : null;
                lineEntry = new Who(logDatum, YouAre.WhoAreYou(name), 0, null, null, null, guild, isAnon);
            }

            // These groups are for non-anonymous characters
            else
            {
                var level = match.Groups[4].Success ? int.Parse(match.Groups[4].Value) : 0;
                var title = match.Groups[5].Success ? match.Groups[5].Value : null;
                var @class = match.Groups[6].Success ? match.Groups[6].Value : null;
                var name = match.Groups[7].Success ? match.Groups[7].Value : null;
                var race = match.Groups[8].Success ? match.Groups[8].Value : null;
                var guild = match.Groups[9].Success ? match.Groups[9].Value : null;
                var zone = match.Groups[10].Success ? match.Groups[10].Value : null;
                lineEntry = new Who(logDatum, YouAre.WhoAreYou(name), level, title, @class, race, guild, isAnon, new Zone(logDatum, zone));
            }

            // What about chars who are /role playing? Is that the same as anonymous?

            return true;
        }
    }
}
