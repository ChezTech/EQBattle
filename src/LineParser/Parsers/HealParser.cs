using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using BizObjects;
using LogObjects;

namespace LineParser.Parsers
{
    public class HealParser : IParser
    {
        private readonly Regex RxHeal;
        private readonly string rgexHeal = @"(.+\. )?(.+) healed (.*?)( over time)? for (\d+)(?: \((\d+)\))? hit points by (.+)\.(?: \((.+)\))?"; // https://regex101.com/r/eBfX2c/2

        public HealParser()
        {
            RxHeal = new Regex(rgexHeal, RegexOptions.Compiled);
        }

        public bool TryParse(LogDatum logDatum, out ILine lineEntry)
        {
            if (TryParseHeal(logDatum, out lineEntry))
                return true;

            return false;
        }
        private bool TryParseHeal(LogDatum logDatum, out ILine lineEntry)
        {
            var match = RxHeal.Match(logDatum.LogMessage);

            if (!match.Success)
            {
                lineEntry = null;
                return false;
            }

            var flavor = match.Groups[1].Value;
            var healer = match.Groups[2].Value;
            var patient = match.Groups[3].Value;
            var isHot = match.Groups[4].Success;
            var amount = int.Parse(match.Groups[5].Value);
            var maxAmount = match.Groups[6].Success ? int.Parse(match.Groups[6].Value) : -1;
            var spellName = match.Groups[7].Value;
            var qualifier = match.Groups[8].Success ? match.Groups[8].Value : null;

            lineEntry = new Heal(logDatum, healer, patient, amount, maxAmount, spellName, isHot, qualifier);

            return true;
        }
    }
}
