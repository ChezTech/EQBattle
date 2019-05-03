using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using LogObjects;

namespace BizObjects.Parsers
{
    public class HitParser : IParser
    {





        //private const string zzzz = ""; // [Fri Apr 26 09:26:33 2019] You kick a cliknar adept for 1016 points of damage.
        //private const string zzzz = ""; // [Fri Apr 26 09:26:33 2019] You punch a cliknar adept for 1277 points of damage.
        //private const string zzzz = ""; // [Fri Apr 26 09:26:34 2019] You hit a cliknar adept for 1180 points of chromatic damage by Lynx Maw. (Critical)
        //private const string zzzz = ""; // [Fri Apr 26 09:26:36 2019] A cliknar adept is burned by YOUR flames for 896 points of non-melee damage.
        //private const string zzzz = ""; // [Fri Apr 26 09:26:36 2019] A cliknar adept pierces YOU for 865 points of damage. (Strikethrough)
        //private const string zzzz = ""; // [Fri Apr 26 09:26:36 2019] YOU are pierced by a cliknar adept's thorns for 70 points of non-melee damage!
        //private const string zzzz = ""; // [Fri Apr 26 09:26:37 2019] You strike a cliknar adept for 1219 points of damage.


        private readonly Regex RxYouHit; // https://regex101.com/r/tall6K/1


        //private readonly IList<string> VerbList = new List<string>() {
        //    "hit", "hits",
        //    "pierce", "pierces",
        //    "kick", "kicks",
        //    "strike", "strikes"
        //};

        public HitParser()
        {
            RxYouHit = new Regex(@"(You) \b(\w+)\b (.*) for (\d+) points? of( (.*))? damage( by (.*))?\.( \((.*)\))?", RegexOptions.Compiled);
        }

        public bool TryParse(LogDatum logDatum, out Line lineEntry)
        {
            if (TryParseYouHit(logDatum, out lineEntry))
                return true;
            return false;
        }

        private bool TryParseYouHit(LogDatum logDatum, out Line lineEntry)
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
    }
}
