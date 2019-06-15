using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using BizObjects.Converters;
using BizObjects.Lines;
using LogObjects;

namespace LineParser.Parsers
{
    public class HitParser : IParser
    {
        private const string DamagePhrase = "damage";
        private readonly Regex RxHit;
        private readonly Regex RxDamageShield;
        private readonly Regex RxDot;
        private readonly Regex RxYourDot;
        private readonly Regex RxAnonymousDot;
        private readonly Regex RxSpellDamage = new Regex(@"^(.*)\.\s+You have taken (\d+) points of damage\.(?: \((.+)\))?$", RegexOptions.Compiled); // https://regex101.com/r/7g5Xfe/6
        private readonly Regex RxNonMeleeDamage = new Regex(@"^(.+) was (.+) by (.+) for (\d+) points of damage\.$", RegexOptions.Compiled); // https://regex101.com/r/y3K5qL/3/
        private readonly string regexHit = @"(.+) (**verbs**) (.+) for (\d+) points? of(?: (.+))? damage(?: by (.+))?\.(?: \((.+)\))?"; // https://regex101.com/r/bc2GRX/2
        private readonly string regexDamageShield = @"(.+) (?:is|are) (**verbs**) by (.+) (.+) for (\d+) points? of(?: (.+))? damage(?: by (.+))?[.!](?: \((.+)\))?"; // https://regex101.com/r/uerSMk/2/
        private readonly string regexDot = @"(.+) (?:has|have) taken (\d+) damage from (.+) by (.+)\.(?: \((.+)\))?"; // https://regex101.com/r/U4DUt4/2
        private readonly string regexYourDot = @"(.+) has taken (\d+) damage from your (.+)\.(?: \((.+)\))?"; // https://regex101.com/r/6f1Xfq/1

        // This happen's when a mob dies and a DoT is still on a PC. No more damage from "a generic mob's corpse"
        private readonly string regexAnonymousDot = @"(.+) (?:has|have) taken (\d+) damage (?:by|from) (.+)\.(?: \((.+)\))?"; // https://regex101.com/r/rqZ5qD/4

        private readonly YouResolver YouAre;

        public HitParser(YouResolver youAre)
        {
            YouAre = youAre;
            string verbs = string.Join('|', new AttackTypeConverter().Names);
            RxHit = new Regex(regexHit.Replace("**verbs**", verbs), RegexOptions.Compiled);
            RxDamageShield = new Regex(regexDamageShield.Replace("**verbs**", verbs), RegexOptions.Compiled);
            RxDot = new Regex(regexDot, RegexOptions.Compiled);
            RxYourDot = new Regex(regexYourDot, RegexOptions.Compiled);
            RxAnonymousDot = new Regex(regexAnonymousDot, RegexOptions.Compiled);
        }

        public bool TryParse(LogDatum logDatum, out ILine lineEntry)
        {
            lineEntry = null;

            if (EarlyExit(logDatum))
                return false;

            // Need to do Damage Shield before normal hit because normal hit will match a DS message, but won't group it correctly.
            if (TryParseDamageShield(logDatum, out lineEntry))
                return true;

            // Need to do non-melee damage before normal hit otherwise it will match incorrectly.
            if (TryParseNonMeleeDamage(logDatum, out lineEntry))
                return true;

            if (TryParseHit(logDatum, out lineEntry))
                return true;
            if (TryParseDot(logDatum, out lineEntry))
                return true;
            if (TryParseYourDot(logDatum, out lineEntry))
                return true;
            if (TryParseAnonymousDot(logDatum, out lineEntry))
                return true;
            if (TryParseSpellDamage(logDatum, out lineEntry))
                return true;

            return false;
        }

        private bool EarlyExit(LogDatum logDatum)
        {
            if (logDatum.LogMessage.Contains(DamagePhrase))
                return false;
            return true;
        }

        private bool TryParseHit(LogDatum logDatum, out ILine lineEntry)
        {
            var match = RxHit.Match(logDatum.LogMessage);

            if (!match.Success)
            {
                lineEntry = null;
                return false;
            }

            var attacker = match.Groups[1].Value;
            var attackVerb = match.Groups[2].Value;
            var defender = match.Groups[3].Value;
            var damage = int.Parse(match.Groups[4].Value);
            var damageType = match.Groups[5].Success ? match.Groups[5].Value : null;
            var damageBy = match.Groups[6].Success ? match.Groups[6].Value : null;
            var damageQualifier = match.Groups[7].Success ? match.Groups[7].Value : null;

            lineEntry = new Hit(logDatum, YouAre.WhoAreYou(attacker), YouAre.WhoAreYou(defender), attackVerb, damage, damageType, damageBy, damageQualifier);

            return true;
        }

        private bool TryParseDamageShield(LogDatum logDatum, out ILine lineEntry)
        {
            var match = RxDamageShield.Match(logDatum.LogMessage);

            if (!match.Success)
            {
                lineEntry = null;
                return false;
            }

            var attacker = match.Groups[3].Value;
            var attackVerb = match.Groups[2].Value;
            var defender = match.Groups[1].Value;
            var damage = int.Parse(match.Groups[5].Value);
            var damageType = match.Groups[6].Value;
            var damageBy = match.Groups[4].Value;
            string damageQualifier = null;

            lineEntry = new Hit(logDatum, YouAre.WhoAreYou(attacker), YouAre.WhoAreYou(defender), YouAre.WhoAreYou(attackVerb), damage, damageType, damageBy, damageQualifier);

            return true;
        }

        private bool TryParseDot(LogDatum logDatum, out ILine lineEntry)
        {
            var match = RxDot.Match(logDatum.LogMessage);

            if (!match.Success)
            {
                lineEntry = null;
                return false;
            }

            var attacker = match.Groups[4].Value;
            string attackVerb = AttackType.DamageOverTime.ToString();
            var defender = match.Groups[1].Value;
            var damage = int.Parse(match.Groups[2].Value);
            string damageType = null; // DoT ?
            var damageBy = match.Groups[3].Value;
            var damageQualifier = match.Groups[5].Success ? match.Groups[5].Value : null;

            lineEntry = new Hit(logDatum, YouAre.WhoAreYou(attacker), YouAre.WhoAreYou(defender), attackVerb, damage, damageType, damageBy, damageQualifier);

            return true;
        }

        private bool TryParseYourDot(LogDatum logDatum, out ILine lineEntry)
        {
            var match = RxYourDot.Match(logDatum.LogMessage);

            if (!match.Success)
            {
                lineEntry = null;
                return false;
            }

            var attacker = YouAre.Name;
            string attackVerb = AttackType.DamageOverTime.ToString();
            var defender = match.Groups[1].Value;
            var damage = int.Parse(match.Groups[2].Value);
            string damageType = null; // DoT ?
            var damageBy = match.Groups[3].Value;
            var damageQualifier = match.Groups[4].Success ? match.Groups[4].Value : null;

            lineEntry = new Hit(logDatum, attacker, YouAre.WhoAreYou(defender), attackVerb, damage, damageType, damageBy, damageQualifier);

            return true;
        }

        private bool TryParseAnonymousDot(LogDatum logDatum, out ILine lineEntry)
        {
            var match = RxAnonymousDot.Match(logDatum.LogMessage);

            if (!match.Success)
            {
                lineEntry = null;
                return false;
            }

            string attacker = null; // Anonymous
            string attackVerb = AttackType.DamageOverTime.ToString();
            var defender = match.Groups[1].Value;
            var damage = int.Parse(match.Groups[2].Value);
            string damageType = null; // DoT ?
            var damageBy = match.Groups[3].Value;
            var damageQualifier = match.Groups[4].Success ? match.Groups[4].Value : null;

            lineEntry = new Hit(logDatum, attacker, YouAre.WhoAreYou(defender), attackVerb, damage, damageType, damageBy, damageQualifier);

            return true;
        }

        private bool TryParseSpellDamage(LogDatum logDatum, out ILine lineEntry)
        {
            var match = RxSpellDamage.Match(logDatum.LogMessage);

            if (!match.Success)
            {
                lineEntry = null;
                return false;
            }

            var spellDescription = match.Groups[1].Value;
            string attacker = null; // Anonymous
            var defender = YouAre.Name;
            string attackVerb = null;
            var damage = int.Parse(match.Groups[2].Value);
            string damageType = null;
            string damageBy = null;
            var damageQualifier = match.Groups[3].Success ? match.Groups[3].Value : null;

            lineEntry = new Hit(logDatum, attacker, defender, attackVerb, damage, damageType, damageBy, damageQualifier);

            return true;
        }

        private bool TryParseNonMeleeDamage(LogDatum logDatum, out ILine lineEntry)
        {
            var match = RxNonMeleeDamage.Match(logDatum.LogMessage);

            if (!match.Success)
            {
                lineEntry = null;
                return false;
            }

            var spellDescription = match.Groups[1].Value;
            string attacker = null; // Anonymous
            var defender = match.Groups[1].Value;
            var attackVerb = match.Groups[2].Value;
            var damage = int.Parse(match.Groups[4].Value);
            var damageType = match.Groups[3].Value;
            string damageBy = null;
            string damageQualifier = null;

            lineEntry = new Hit(logDatum, attacker, defender, attackVerb, damage, damageType, damageBy, damageQualifier);

            return true;
        }
    }
}
