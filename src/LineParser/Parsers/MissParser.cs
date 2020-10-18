using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using BizObjects.Converters;
using BizObjects.Lines;
using LogObjects;

namespace LineParser.Parsers
{
    public class MissParser : IParser
    {
        private readonly Regex RxYouMiss;
        private readonly Regex RxOtherMiss;
        private readonly string regexYouMiss = @"You try to (.*?) (.+), but (.+)[!](?: \((.+)\))?"; // https://regex101.com/r/0CWNBA/2
        private readonly string regexOtherMiss = @"(.+) tries to (.*?) (?:on )?(.+), but (?:YOU |YOUR )?(.+)[!](?: \((.+)\))?"; // https://regex101.com/r/qwjLjz/3

        private readonly YouResolver YouAre;

        public MissParser(YouResolver youAre)
        {
            YouAre = youAre;
            RxYouMiss = new Regex(regexYouMiss, RegexOptions.Compiled);
            RxOtherMiss = new Regex(regexOtherMiss, RegexOptions.Compiled);
        }

        public bool TryParse(LogDatum logDatum, out ILine lineEntry)
        {
            lineEntry = null;

            if (EarlyExit(logDatum))
                return false;

            if (TryParseYouMiss(logDatum, out lineEntry))
                return true;
            if (TryParseOtherMiss(logDatum, out lineEntry))
                return true;

            return false;
        }
        private bool EarlyExit(LogDatum logDatum)
        {
            if (logDatum.LogMessage.Contains("try"))
                return false;
            if (logDatum.LogMessage.Contains("tries"))
                return false;

            return true;
        }

        private bool TryParseYouMiss(LogDatum logDatum, out ILine lineEntry)
        {
            var match = RxYouMiss.Match(logDatum.LogMessage);

            if (!match.Success)
            {
                lineEntry = null;
                return false;
            }

            var attacker = YouAre.Name;
            var attackVerb = match.Groups[1].Value;
            var defender = match.Groups[2].Value;
            var defense = match.Groups[3].Value;
            var qualifier = match.Groups[4].Success ? match.Groups[4].Value : null;

            defense = ModifyDefense(defender, defense);

            lineEntry = new Miss(logDatum, attacker, YouAre.WhoAreYou(defender), attackVerb, defense, qualifier);

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

            defense = ModifyDefense(defender, defense);

            lineEntry = new Miss(logDatum, YouAre.WhoAreYou(attacker), YouAre.WhoAreYou(defender), attackVerb, defense, qualifier);

            return true;
        }

        private static string ModifyDefense(string defender, string defense)
        {
            // Check to see if a defender did some defense: "a gnome servant dodges"
            if (defense.StartsWith(defender))
            {
                defense = defense.Substring(defender.Length + 1);

                // Further check to see if it was the defender's skin: "a telmira disciple's magical skin absorbs the blow"
                if (defense.StartsWith("s "))
                    defense = defense.Substring(2);
            }

            return defense;
        }
    }
}
