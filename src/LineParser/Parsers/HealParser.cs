using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using BizObjects.Converters;
using BizObjects.Lines;
using LogObjects;

namespace LineParser.Parsers
{
    public class HealParser : IParser
    {
        private readonly Regex RxHeal;
        private readonly string rgexHeal = @"^(.+\. )?(.*?)(?: (has been))? healed (.*?)(?: (over time))? for (\d+)(?: \((\d+)\))? hit points by (.+)\.(?: \((.+)\))?$"; // https://regex101.com/r/eBfX2c/3
        private readonly Regex RxYouAreHealed = new Regex(@"^You have been healed for (\d+) points of damage\.$", RegexOptions.Compiled); // https://regex101.com/r/2Stapt/1
        private readonly Regex RxCompleteHeal = new Regex(@"^(.+) (is|are) completely healed\.$", RegexOptions.Compiled); // https://regex101.com/r/2CHSPg/2/
        private readonly YouResolver YouAre;

        public HealParser(YouResolver youAre)
        {
            YouAre = youAre;
            RxHeal = new Regex(rgexHeal, RegexOptions.Compiled);
        }

        public bool TryParse(LogDatum logDatum, out ILine lineEntry)
        {
            lineEntry = null;

            if (EarlyExit(logDatum))
                return false;

            if (TryParseHeal(logDatum, out lineEntry))
                return true;
            if (TryParseYouAreHealed(logDatum, out lineEntry))
                return true;
            if (TryParseCompleteHeal(logDatum, out lineEntry))
                return true;

            return false;
        }

        private bool EarlyExit(LogDatum logDatum)
        {
            if (logDatum.LogMessage.Contains("healed"))
                return false;

            return true;
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
            var patient = match.Groups[4].Value;
            // Mordsith has been healed ...
            if (match.Groups[3].Success)
            {
                patient = healer;
                healer = null;
            }

            // Khronick healed you over time ...
            // Mordsith has been healed over time ...
            var isHot = match.Groups[5].Success || match.Groups[4].Value == "over time";

            var amount = int.Parse(match.Groups[6].Value);
            var maxAmount = match.Groups[7].Success ? int.Parse(match.Groups[7].Value) : -1;
            var spellName = match.Groups[8].Value;
            var qualifier = match.Groups[9].Success ? match.Groups[9].Value : null;

            lineEntry = new Heal(logDatum, YouAre.WhoAreYou(healer), YouAre.WhoAreYou(patient), amount, maxAmount, spellName, isHot, qualifier);

            return true;
        }

        private bool TryParseYouAreHealed(LogDatum logDatum, out ILine lineEntry)
        {
            var match = RxYouAreHealed.Match(logDatum.LogMessage);

            if (!match.Success)
            {
                lineEntry = null;
                return false;
            }

            string healer = null;
            var amount = int.Parse(match.Groups[1].Value);
            var isHot = false;
            var maxAmount = -1;
            string spellName = null;
            string qualifier = null;

            lineEntry = new Heal(logDatum, healer, YouAre.Name, amount, maxAmount, spellName, isHot, qualifier);

            return true;
        }

        private bool TryParseCompleteHeal(LogDatum logDatum, out ILine lineEntry)
        {
            var match = RxCompleteHeal.Match(logDatum.LogMessage);

            if (!match.Success)
            {
                lineEntry = null;
                return false;
            }

            var patient = match.Groups[1].Value;
            string healer = null;
            var amount = 0;
            var isHot = false;
            var maxAmount = -1;
            string spellName = "Complete Heal";
            string qualifier = null;

            lineEntry = new Heal(logDatum, healer, YouAre.WhoAreYou(patient), amount, maxAmount, spellName, isHot, qualifier);

            return true;
        }    }
}
