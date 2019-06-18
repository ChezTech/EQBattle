using BizObjects.Converters;
using BizObjects.Lines;
using BizObjects.Statistics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace BizObjects.Battle
{
    public class Skirmish : IFight
    {
        private readonly YouResolver YouAre;
        private readonly CharacterResolver CharResolver;

        public Skirmish(YouResolver youAre, CharacterResolver charResolver)
        {
            YouAre = youAre;
            CharResolver = charResolver;
            CreateNewFight();
        }

        public IList<IFight> Fights { get; } = new List<IFight>();

        public bool IsFightOver => Fights.All(x => x.IsFightOver);

        public DateTime LastAttackTime => Fights.Max(x => x.LastAttackTime);

        private ConcurrentDictionary<Character, Fighter> _fighters = new ConcurrentDictionary<Character, Fighter>();
        public IEnumerable<Fighter> Fighters => _fighters.Values;

        public FightStatistics Statistics { get; } = new FightStatistics();

        public Character PrimaryMob => Character.Unknown;
        public string Title => string.Join(", ", Fights.Select(x => x.PrimaryMob.Name));

        public void AddLine(Attack line)
        {
            GetAppropriateFight(ref line).AddLine(line);

            var attackChar = _fighters.GetOrAdd(line.Attacker, new Fighter(line.Attacker, this));
            attackChar.AddOffense(line);
            Statistics.AddLine(line);

            var defendChar = _fighters.GetOrAdd(line.Defender, new Fighter(line.Defender, this));
            defendChar.AddDefense(line);
        }

        public void AddLine(Heal line)
        {
            GetAppropriateFight(line).AddLine(line);

            var healerChar = _fighters.GetOrAdd(line.Healer, new Fighter(line.Healer));
            healerChar.AddOffense(line);
            Statistics.AddLine(line);

            var patientChar = _fighters.GetOrAdd(line.Patient, new Fighter(line.Patient));
            patientChar.AddDefense(line);
        }

        public void AddLine(ILine line)
        {
            // GetAppropriateFight(line).AddLine(line);
        }

        // This is the function we'll use to see if a fight is a valid match (mob match and fight not over)
        private readonly Func<IFight, Character, bool> IsValidFight = (f, c) => f.PrimaryMob == c && !f.IsFightOver;

        private IFight GetAppropriateFight(ref Attack line)
        {
            if (TryFindPostHumousFightDamage((dynamic)line, out IFight deadFight, out Attack newLineWithAttacker))
            {
                line = newLineWithAttacker;
                return deadFight;
            }
            return GetAppropriateFight(line.Attacker, line.Defender);
        }

        private IFight GetAppropriateFight(Heal line)
        {
            // A heal line just applies to the last fight
            // It's a PC/Merc that's healing another PC/Merc
            // Except when it's a mob healing itself ... or healing a companion fighter...
            // Would a PC ever heal a Mob or vice versa? Maybe for a charmed pet ... but that's going to be a pain in the ass anyway

            if (IsCharacterPlayerOrMerc(line.Healer) || IsCharacterPlayerOrMerc(line.Patient))
            {
                // Apply the heal to the first active fight, or the last fight (that's inactive)
                return Fights.FirstOrDefault(x => !x.IsFightOver)
                    ?? Fights.Last();
            }

            return GetAppropriateFight(line.Healer, line.Patient);
        }

        private bool IsCharacterPlayerOrMerc(Character c)
        {
            var charType = CharResolver.WhichType(c);

            switch (charType)
            {
                case CharacterResolver.Type.NonPlayerCharacter:
                    return false;

                case CharacterResolver.Type.Player:
                case CharacterResolver.Type.Mercenary:

                // It's more that this char is not an NPC
                default:
                    return true;
            }
        }

        private IFight GetAppropriateFight(Character char1, Character char2)
        {
            // If the primary mob isn't established yet, use the first fight
            var fight = Fights.First();
            if (IsValidFight(fight, Character.Unknown))
                return fight;

            var firstActiveFight = Fights.Where(x => !x.IsFightOver);

            // If either one of the characters is Unknown, just use the first active fight
            if ((char1 == Character.Unknown || char2 == Character.Unknown) && firstActiveFight.Any())
                return firstActiveFight.First();

            // If the char is a MOB, find the matching fight or create a new one
            if (CharResolver.WhichType(char1) == CharacterResolver.Type.NonPlayerCharacter)
                return GetOrAddFight(char1);
            if (CharResolver.WhichType(char2) == CharacterResolver.Type.NonPlayerCharacter)
                return GetOrAddFight(char2);

            // See if either character is already the primary mob (we can't tell if a named mob w/ a single name is a mob, so this may catch that)
            var primaryMobMatch = Fights.Where(x => IsValidFight(x, char1) || IsValidFight(x, char2));
            if (primaryMobMatch.Any())
                return primaryMobMatch.First();

            // Either the characters are not MOBs or one of them is a named Mob and we don't know it, just use the first fight that's still ongoing
            if (firstActiveFight.Any())
                return firstActiveFight.First();

            return CreateNewFight();
        }

        private IFight GetOrAddFight(Character mob)
        {
            return Fights
                .Where(x => IsValidFight(x, mob))
                .FirstOrDefault()
                ?? CreateNewFight();
        }

        private IFight CreateNewFight()
        {
            var fight = new Fight(YouAre, CharResolver);
            Fights.Add(fight);
            return fight;
        }

        private bool TryFindPostHumousFightDamage(Attack line, out IFight deadFight, out Attack newLineWithAttacker)
        {
            deadFight = null;
            newLineWithAttacker = null;
            return false;
        }
        private bool TryFindPostHumousFightDamage(Hit line, out IFight deadFight, out Attack newLineWithAttacker)
        {
            deadFight = null;
            newLineWithAttacker = null;

            if (line.Type != AttackType.DamageOverTime)
                return false;

            // This is to handle the case of a DOT still going after the attacker died and the log line no longer indicates the attacker name
            // It used to say " ... by mob's corpse", now it just doesn't say
            // "Movanna has taken 3000 damage by Noxious Visions."

            // Further complicating things is the fact that not all DOTs specify their attacker even if they are alive
            // It seems if a PC takes the damage, there is an Attacker in the message, while if a Merc takes the damage no Attacker is indicated

            // [Mon May 27 09:56:45 2019] You have taken 1950 damage from Noxious Visions by Gomphus.
            // [Mon May 27 09:56:45 2019] Khronick has taken 1950 damage from Noxious Visions by Gomphus.
            // [Mon May 27 09:56:47 2019] Movanna has taken 3000 damage by Noxious Visions.

            // Once it dies, it's still identified

            // [Mon May 27 09:58:40 2019] You have taken 1950 damage from Noxious Visions by Gomphus's corpse.
            // [Mon May 27 09:58:40 2019] Khronick has taken 1950 damage from Noxious Visions by Gomphus's corpse.
            // [Mon May 27 09:58:42 2019] Movanna has taken 3000 damage by Noxious Visions.

            // The complication is that I get this anonymous DoT both when the mob was alive and when it was dead...
            // [Mon May 27 06:57:03 2019] You have taken 2080 damage from Paralyzing Bite.
            // [Mon May 27 06:57:09 2019] You have taken 2080 damage from Paralyzing Bite by a sandspinner stalker.


            // Basically, if this is an Anoymous Dot ... assign the Attacker to one of the primary mob fights ... which one?
            // Let's see if a fight has had similar damage already ... same spell

            // If we have, remake the Attack line instance (since it's immutable)

            // So, find the most recent fight that has similar damage
            // Try a tight match first (Defender, DamageType, DamageAmount), then a loose match (DamageType)
            var fightsWithSimilarDamage =
                Fights.LastOrDefault(x => x.SimilarDamage(line)) // Tight match for similar damage (Defender, DamageType, DamageAmount)
                ?? Fights.LastOrDefault(x => x.SimilarDamage(line, true)) // Loose match for similar damage (DamageType)
                ?? Fights.LastOrDefault(x => line.Attacker != Character.Unknown && x.PrimaryMob == line.Attacker); // Plain match on attacker name (if you're still taking damage from a corpse)

            // If we do have such a fight, remake this Attack Line w/ the Primary Mob as the attacker
            if (fightsWithSimilarDamage != null)
            {
                deadFight = fightsWithSimilarDamage;
                newLineWithAttacker = new Hit(line.LogLine, fightsWithSimilarDamage.PrimaryMob.Name, line.Defender.Name, line.Verb, line.Damage, line.DamageType, line.DamageBy, line.DamageQualifier, line.Zone);
                return true;
            }

            // Otherwise, business as normal
            return false;
        }

        public bool SimilarDamage(Hit line, bool looseMatch = false)
        {
            throw new NotImplementedException();
        }
    }
}
