using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using LogObjects;

namespace BizObjects.Parsers
{
    public class HitParser : IParser
    {
        private readonly Regex RxYouHit; // https://regex101.com/r/tall6K/1
        private readonly Regex RxOtherHitsYou; // https://regex101.com/r/OqdZme/2
        private readonly Regex RxYourDamageShield; // https://regex101.com/r/7iMeAc/1
        private readonly Regex RxDamageShieldOnYou; // https://regex101.com/r/jMfkL3/1
        private readonly Regex RxPet; // https://regex101.com/r/DHZqDT/1


        //private readonly IList<string> VerbList = new List<string>() {
        //    "hit", "hits",
        //    "pierce", "pierces",
        //    "kick", "kicks",
        //    "strike", "strikes"
        //};

        public HitParser()
        {
            RxYouHit = new Regex(@"(You) \b(\w+)\b (.*) for (\d+) points? of( (.*))? damage( by (.*))?\.( \((.*)\))?", RegexOptions.Compiled);
            RxOtherHitsYou = new Regex(@"(.*) \b(\w+)\b YOU for (\d+) points? of( (.*))? damage\.( \((.*)\))?", RegexOptions.Compiled);
            RxYourDamageShield = new Regex(@"(.*) is (.*) by (.*) (.*) for (\d+) points of (.*) damage.", RegexOptions.Compiled);
            RxDamageShieldOnYou = new Regex(@"(.*) are (.*) by (.*)'s (.*) for (\d+) points of (.*) damage!", RegexOptions.Compiled);

            // Note: possesive is written using a back-tick, *not* an apostrophe (as should be)
            RxPet = new Regex(@"(.*)`s pet \b(\w+)\b (.*) for (\d+) points? of( (.*))? damage( by (.*))?\.( \((.*)\))?", RegexOptions.Compiled);
        }

        public bool TryParse(LogDatum logDatum, out ILine lineEntry)
        {
            if (TryParseYouHit(logDatum, out lineEntry))
                return true;
            if (TryParseOtherHitsYou(logDatum, out lineEntry))
                return true;
            if (TryParseYourDamageShield(logDatum, out lineEntry))
                return true;
            if (TryParseDamageShieldOnYou(logDatum, out lineEntry))
                return true;
            if (TryParsePet(logDatum, out lineEntry))
                return true;
            return false;
        }

        private bool TryParseYouHit(LogDatum logDatum, out ILine lineEntry)
        {
            var match = RxYouHit.Match(logDatum.LogMessage);

            if (!match.Success)
            {
                lineEntry = null;
                return false;
            }

            var attacker = Attack.You;
            var attackVerb = match.Groups[2].Value;
            var defender = match.Groups[3].Value;
            var damage = int.Parse(match.Groups[4].Value);
            var damageType = match.Groups[6].Success ? match.Groups[6].Value : null;
            var damageBy = match.Groups[8].Success ? match.Groups[8].Value : null;
            var damageQualifier = match.Groups[10].Success ? match.Groups[10].Value : null;

            lineEntry = new Hit(logDatum, attacker, defender, attackVerb, damage, damageType, damageBy, damageQualifier);

            return true;
        }

        private bool TryParseOtherHitsYou(LogDatum logDatum, out ILine lineEntry)
        {
            var match = RxOtherHitsYou.Match(logDatum.LogMessage);

            if (!match.Success)
            {
                lineEntry = null;
                return false;
            }

            var attacker = match.Groups[1].Value;
            var attackVerb = match.Groups[2].Value;
            var defender = Attack.You;
            var damage = int.Parse(match.Groups[3].Value);
            string damageType = match.Groups[5].Success ? match.Groups[5].Value : null;
            string damageBy = null;
            var damageQualifier = match.Groups[7].Success ? match.Groups[7].Value : null;

            lineEntry = new Hit(logDatum, attacker, defender, attackVerb, damage, damageType, damageBy, damageQualifier);

            return true;
        }

        private bool TryParseYourDamageShield(LogDatum logDatum, out ILine lineEntry)
        {
            var match = RxYourDamageShield.Match(logDatum.LogMessage);

            if (!match.Success)
            {
                lineEntry = null;
                return false;
            }

            var defender = match.Groups[1].Value;
            var attackVerb = match.Groups[2].Value;
            var attacker = Attack.You;
            var damageBy = match.Groups[4].Value;
            var damage = int.Parse(match.Groups[5].Value);
            var damageType = match.Groups[6].Value;
            string damageQualifier = null;

            lineEntry = new Hit(logDatum, attacker, defender, attackVerb, damage, damageType, damageBy, damageQualifier);

            return true;
        }

        private bool TryParseDamageShieldOnYou(LogDatum logDatum, out ILine lineEntry)
        {
            var match = RxDamageShieldOnYou.Match(logDatum.LogMessage);

            if (!match.Success)
            {
                lineEntry = null;
                return false;
            }

            var defender = Attack.You;
            var attackVerb = match.Groups[2].Value;
            var attacker = match.Groups[3].Value;
            var damageBy = match.Groups[4].Value;
            var damage = int.Parse(match.Groups[5].Value);
            var damageType = match.Groups[6].Value;
            string damageQualifier = null;

            lineEntry = new Hit(logDatum, attacker, defender, attackVerb, damage, damageType, damageBy, damageQualifier);

            return true;
        }

        private bool TryParsePet(LogDatum logDatum, out ILine lineEntry)
        {
            var match = RxPet.Match(logDatum.LogMessage);

            if (!match.Success)
            {
                lineEntry = null;
                return false;
            }

            var attacker = match.Groups[1].Value;
            var isPet = true;
            var attackVerb = match.Groups[2].Value;
            var defender = match.Groups[3].Value;
            var damage = int.Parse(match.Groups[4].Value);
            var damageType = match.Groups[6].Success ? match.Groups[6].Value : null;
            var damageBy = match.Groups[8].Success ? match.Groups[8].Value : null;
            var damageQualifier = match.Groups[10].Success ? match.Groups[10].Value : null;

            lineEntry = new Hit(logDatum, attacker, defender, attackVerb, damage, damageType, damageBy, damageQualifier, isPet);

            return true;
        }

    }
}
