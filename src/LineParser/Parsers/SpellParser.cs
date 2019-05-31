using System;
using System.Text.RegularExpressions;
using BizObjects;
using LogObjects;

namespace LineParser.Parsers
{
    public class SpellParser : IParser
    {
        private readonly Regex RxSpell = new Regex(@"(.+) begin(?:s)? (?:casting|to cast a spell.) <?(.+)[>.]", RegexOptions.Compiled); // https://regex101.com/r/bIFFVy/2/

        private readonly YouResolver YouAre;

        public SpellParser(YouResolver youAre)
        {
            YouAre = youAre;
        }

        public bool TryParse(LogDatum logDatum, out ILine lineEntry)
        {
            if (TryParseSpell(logDatum, out lineEntry))
                return true;

            return false;
        }

        private bool TryParseSpell(LogDatum logDatum, out ILine lineEntry)
        {
            var match = RxSpell.Match(logDatum.LogMessage);

            if (!match.Success)
            {
                lineEntry = null;
                return false;
            }

            var name = match.Groups[1].Value;
            var spellName = match.Groups[2].Value;

            lineEntry = new Spell(logDatum, YouAre.WhoAreYou(name), spellName);

            return true;
        }
    }
}
