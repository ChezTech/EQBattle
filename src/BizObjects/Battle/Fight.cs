using BizObjects.Converters;
using BizObjects.Lines;
using BizObjects.Statistics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BizObjects.Battle
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Fight : FightBase
    {
        private readonly YouResolver YouAre;
        private readonly CharacterResolver CharResolver;

        public override string Title => PrimaryMob.Name;

        public string Zone { get; }
        public override int LineCount => Statistics.Lines.Count();

        public Fight(YouResolver youAre, CharacterResolver charResolver)
        {
            YouAre = youAre;
            CharResolver = charResolver;
        }

        public override bool IsFightOver
        {
            get
            {
                // A fight is over if
                // - the main MOB is dead (what is the main mob? what about multiple mobs?)
                // - it's been too long since the last attack
                //   - it's not a loot line (we want to accept loot lines into this fight after the mob is dead .. as long as it's not a new attack before then)

                if (PrimaryMobFighter.IsDead)
                    return true;

                return false;
            }
        }

        public override DateTime LastAttackTime => Statistics.Lines.LastOrDefault()?.Time ?? new DateTime();

        public override void AddLine(Attack line)
        {
            base.AddLine((ILine)line);
            Statistics.AddLine(line);

            AddFighterLine(line.Attacker, f => f.AddOffense(line));
            AddFighterLine(line.Defender, f => f.AddDefense(line));

            DeterminePrimaryMob(line);

            // A bit of a cop out. We're just saying that the entire Fight has changed with each new line
            OnPropertyChanged(nameof(Fight));
        }

        public override void AddLine(Heal line)
        {
            base.AddLine((ILine)line);
            Statistics.AddLine(line);

            AddFighterLine(line.Healer, f => f.AddOffense(line));
            AddFighterLine(line.Patient, f => f.AddDefense(line));

            // A bit of a cop out. We're just saying that the entire Fight has changed with each new line
            OnPropertyChanged(nameof(Fight));
        }

        // public void AddLine(Zone line) { }
        // public void AddLine(Chat line) { }

        private void DeterminePrimaryMob(Attack line)
        {
            // If mob already set, don't change it
            if (PrimaryMob != Character.Unknown)
                return;

            // If it's a mob, set it
            if (CharResolver.WhichType(line.Attacker) == CharacterResolver.Type.NonPlayerCharacter)
            {
                PrimaryMob = line.Attacker;
                return;
            }

            if (CharResolver.WhichType(line.Defender) == CharacterResolver.Type.NonPlayerCharacter)
            {
                PrimaryMob = line.Defender;
                return;
            }

            // If we've got multiple lines, we should be able to see who's the main target by the fact that everyone keeps beating them up
            if (!Fighters.Any())
                return;

            // Note: this finds the fighter who has the most combined attack lines
            // better would be to find the fight with the most distinct other fighters who have been attacking them
            var topFighter = Fighters.Aggregate((maxItem, nextItem) => maxItem.DefensiveStatistics.Lines.Count + maxItem.OffensiveStatistics.Lines.Count > nextItem.DefensiveStatistics.Lines.Count + nextItem.OffensiveStatistics.Lines.Count ? maxItem : nextItem);
            if (topFighter.DefensiveStatistics.Lines.Count + topFighter.OffensiveStatistics.Lines.Count >= 2)
                PrimaryMob = topFighter.Character;
        }

        private string DebuggerDisplay
        {
            get
            {
                return string.Format($"{PrimaryMob.Name}{(IsFightOver ? " - dead" : "")}");
            }
        }
    }
}
