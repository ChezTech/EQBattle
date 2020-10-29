using BizObjects.Converters;
using BizObjects.Lines;
using BizObjects.Statistics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BizObjects.Battle
{
    public class Skirmish : FightBase, ISkirmish
    {
        private readonly YouResolver YouAre;
        private readonly CharacterResolver CharResolver;

        public Skirmish(YouResolver youAre, CharacterResolver charResolver)
        {
            YouAre = youAre;
            CharResolver = charResolver;
        }

        public ObservableCollection<IFight> Fights { get; } = new ObservableCollection<IFight>();
        public override bool IsFightOver => Fights.Any() && Fights.All(x => x.IsFightOver);
        public override DateTime LastAttackTime => Fights.Max(x => x.LastAttackTime);
        public override int LineCount => Fights.Sum(x => x.LineCount);
        public override string Title => string.Join(", ", Fights.Select(x => x.PrimaryMob.Name));

        public override void AddLine(Attack line)
        {
            base.AddLine((ILine)line);

            GetAppropriateFight(ref line).AddLine(line);
            Statistics.AddLine(line);

            AddFighterLine(line.Attacker, f => f.AddOffense(line));
            AddFighterLine(line.Defender, f => f.AddDefense(line));

            // A bit of a cop out. We're just saying that the entire Skirmish has changed with each new line
            OnPropertyChanged(nameof(Skirmish));
        }

        public override void AddLine(Heal line)
        {
            base.AddLine((ILine)line);

            GetAppropriateFight(line).AddLine(line);
            Statistics.AddLine(line);

            AddFighterLine(line.Healer, f => f.AddOffense(line));
            AddFighterLine(line.Patient, f => f.AddDefense(line));

            // A bit of a cop out. We're just saying that the entire Skirmish has changed with each new line
            OnPropertyChanged(nameof(Skirmish));
        }

        // This is the function we'll use to see if a fight is a valid match (mob match and fight not over)
        private readonly Func<IFight, Character, bool> IsValidFight = (f, c) => f.PrimaryMob == c && !f.IsFightOver;

        private IFight GetAppropriateFight(ref Attack line)
        {
            if (TryGetAppropriateFightDueToPosthumousDamageOverTimeFromAttacker((dynamic)line, out IFight fight))
                return fight;
            if (TryGetAppropriateFightFromAnonymousDamageOverTime((dynamic)line, out fight, out Attack newLineWithAttacker))
            {
                line = newLineWithAttacker;
                return fight;
            }

            return GetAppropriateFight(line.Attacker, line.Defender);
        }

        private IFight GetAppropriateFight(Heal line)
        {
            // A heal line just applies to the last fight
            // It's a PC/Merc that's healing another PC/Merc
            // Except when it's a mob healing itself ... or healing a companion fighter...
            // Would a PC ever heal a Mob or vice versa? Maybe for a charmed pet ... but that's going to be a pain in the ass anyway

            if (Fights.Any() && (IsCharacterPlayerOrMerc(line.Healer) || IsCharacterPlayerOrMerc(line.Patient)))
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
            if (!Fights.Any())
                CreateNewFight();

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

        private bool TryGetAppropriateFightDueToPosthumousDamageOverTimeFromAttacker(Attack line, out IFight deadFight)
        {
            deadFight = null;
            return false;
        }

        /// <summary>
        /// This method finds posthumous damage due to a DoT
        /// </summary>
        private bool TryGetAppropriateFightDueToPosthumousDamageOverTimeFromAttacker(Hit line, out IFight fight)
        {
            fight = null;

            if (line.Type != AttackType.DamageOverTime)
                return false;

            if (line.Attacker == Character.Unknown)
                return false;

            // If there is an attacker and it's not a corpse, then this isn't posthumous damage
            if (!line.Attacker.IsDead)
                return false;

            // If a mob casted a Dot on you before they died, it can still be causing damage after the mob itself dies.

            // This is indicated by taking damage from "a mob's corpse"
            //    "You have taken 57121 damage from Dread Admiral's Curse by Arisen Gloriant Kra`du's corpse."
            //    "Khadajitwo has taken 56242 damage from Dread Admiral's Curse by Arisen Gloriant Kra`du."


            // A complication to this however, is a merc's DoT damage line does not have the "attacker" either before nor after a mob dies.
            //    "Vryklak has taken 28350 damage by Aura of the Kar`Zok."

            // Yet a further complication arises when a DoT is cured and you take one last tick of damage but without the attacker named
            //    "You have taken 768 damage from Rabid Anklebite by Deathfang's corpse."
            //    "Jathenai begins casting Counteract Disease."
            //    "Jathenai begins casting Counteract Disease."
            //    "You feel better."
            //    "You have taken 768 damage from Rabid Anklebite."

            // So, this is now damage due to a dead attacker, let's try to find that last fight/attacker

            var mobFight = Fights.LastOrDefault(x => x.PrimaryMob == line.Attacker);

            // Check the line.DamageBy to find a DoT that matches

            // if we find a dead mob who has used that Dot in the fight, then we know we've got the right one
            // If a group member has the same type of DoT as we do, that's the same mob
            // If can happen that we don't take the DoT damage until after they die, in which case we're just matching the mob

            // Is it enough to just say it's the same mob?
            // - Yes


            if (mobFight != null)
            {
                fight = mobFight;
                return true;
            }

            return false;
        }

        private bool TryGetAppropriateFightFromAnonymousDamageOverTime(Attack line, out IFight fight, out Attack newLineWithAttacker)
        {
            fight = null;
            newLineWithAttacker = null;
            return false;
        }

        /// <summary>
        /// This method finds fight with similar damage due to an anonymous DoT
        /// </summary>
        private bool TryGetAppropriateFightFromAnonymousDamageOverTime(Hit line, out IFight fight, out Attack newLineWithAttacker)
        {
            fight = null;
            newLineWithAttacker = null;

            if (line.Type != AttackType.DamageOverTime)
                return false;

            if (line.Attacker != Character.Unknown)
                return false;

            // This is usually DoT damage on a merc, there is no attacker either while the mob is still alive or after they're dead
            //    "Vryklak has taken 28350 damage by Aura of the Kar`Zok."

            // It can also be DoT damage on you after a cure
            //    "You have taken 768 damage from Rabid Anklebite by Deathfang's corpse."
            //    "Jathenai begins casting Counteract Disease."
            //    "Jathenai begins casting Counteract Disease."
            //    "You feel better."
            //    "You have taken 768 damage from Rabid Anklebite."

            // Search through our fights to find one that has the same DamageBy
            var mobFight = Fights.LastOrDefault(x => x.Statistics.Hit.Lines.Any(y => y.DamageBy == line.DamageBy));

            // So, we didn't find any fight that matches the type of DoT damage for this line
            // We need to assign it to some fight, so let's assign it to the current fight (even if that fight is "over", i.e. mob is slain)
            if (mobFight == null)
                mobFight = Fights.LastOrDefault();

            if (mobFight != null)
            {
                fight = mobFight;
                newLineWithAttacker = new Hit(line.LogLine, mobFight.PrimaryMob.Name, line.Defender.Name, line.Verb, line.Damage, line.DamageType, line.DamageBy, line.DamageQualifier, line.Zone);
                return true;
            }

            return false;
        }
    }
}
