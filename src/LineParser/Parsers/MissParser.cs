using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using BizObjects;
using LogObjects;

namespace LineParser.Parsers
{
    public class MissParser : IParser
    {
        private readonly Regex RxYouMiss;
        private readonly Regex RxOtherMiss;
        private readonly string regexYouMiss = @"You try to (.*?) (.+), but (.+)[!](?: \((.+)\))?"; // https://regex101.com/r/qwjLjz/1/
        private readonly string regexOtherMiss = @"(.+) tries to (.*?) (.+), but (?:YOU |YOUR )?(.+)[!](?: \((.+)\))?"; // https://regex101.com/r/qwjLjz/1/

        public MissParser()
        {
            RxYouMiss = new Regex(regexYouMiss, RegexOptions.Compiled);
            RxOtherMiss = new Regex(regexOtherMiss, RegexOptions.Compiled);
        }

        public bool TryParse(LogDatum logDatum, out ILine lineEntry)
        {
            if (TryParseYouMiss(logDatum, out lineEntry))
                return true;
            if (TryParseOtherMiss(logDatum, out lineEntry))
                return true;

            return false;
        }
        private bool TryParseYouMiss(LogDatum logDatum, out ILine lineEntry)
        {
            var match = RxYouMiss.Match(logDatum.LogMessage);

            if (!match.Success)
            {
                lineEntry = null;
                return false;
            }

            var attacker = Attack.You;
            var attackVerb = match.Groups[1].Value;
            var defender = match.Groups[2].Value;
            var defense = match.Groups[3].Value;
            var qualifier = match.Groups[4].Success ? match.Groups[4].Value : null;

            lineEntry = new Miss(logDatum, attacker, defender, attackVerb, defense, qualifier);

            return true;
        }

        private bool TryParseOtherMiss(LogDatum logDatum, out ILine lineEntry)
        {
            var match = RxOtherMiss.Match(logDatum.LogMessage);

            if (!match.Success)
            {
                lineEntry = null;
                return false;
            }

            var attacker = match.Groups[1].Value;
            var attackVerb = match.Groups[2].Value;
            var defender = match.Groups[3].Value;
            var defense = match.Groups[4].Value;
            var qualifier = match.Groups[5].Success ? match.Groups[5].Value : null;

            lineEntry = new Miss(logDatum, attacker, defender, attackVerb, defense, qualifier);

            return true;
        }
    }
}
