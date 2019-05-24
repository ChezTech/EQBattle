using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace BizObjects
{
    public interface IFight
    {
        void AddLine(ILine line);
        void AddLine(Attack line);
        void AddLine(Heal line);
    }

    public class Fight : IFight
    {
        private readonly YouResolver YouAre;

        public ICollection<Fighter> Fighters { get { return _fighters.Values; } }
        public Character PrimaryMob { get; private set; } = Character.Unknown;
        public string Zone { get; }
        private ConcurrentDictionary<Character, Fighter> _fighters = new ConcurrentDictionary<Character, Fighter>();

        public Fight(YouResolver youAre)
        {
            YouAre = youAre;
        }

        public FightStatistics OffensiveStatistics { get; } = new FightStatistics();
        public FightStatistics DefensiveStatistics { get; } = new FightStatistics();

        public bool IsFightOver(ILine line)
        {

            // A fight is over if
            // - the main MOB is dead (what is the main mob? what about multiple mobs?)
            // - it's been too long since the last attack
            //   - it's not a loot line (we want to accept loot lines into this fight after the mob is dead .. as long as it's not a new attack before then)

            // Obviously, this won't work, but it's a start
            if (OffensiveStatistics.Lines.Any(x => x is Kill))
                return true;
            if (DefensiveStatistics.Lines.Any(x => x is Kill))
                return true;

            return false;
        }

        public virtual void AddLine(Attack line)
        {
            var attackChar = _fighters.GetOrAdd(line.Attacker, new Fighter(line.Attacker, this));
            attackChar.AddOffense(line);
            OffensiveStatistics.AddLine(line);

            var defendChar = _fighters.GetOrAdd(line.Defender, new Fighter(line.Defender, this));
            defendChar.AddDefense(line);
            DefensiveStatistics.AddLine(line);

            DeterminePrimaryMob(line);
        }

        public virtual void AddLine(Heal line)
        {
            var healerChar = _fighters.GetOrAdd(line.Healer, new Fighter(line.Healer));
            healerChar.AddOffense(line);
            OffensiveStatistics.AddLine(line);

            var patientChar = _fighters.GetOrAdd(line.Patient, new Fighter(line.Patient));
            patientChar.AddDefense(line);
            DefensiveStatistics.AddLine(line);
        }

        // public void AddLine(Zone line) { }
        // public void AddLine(Chat line) { }

        public virtual void AddLine(ILine line)
        {

        }

        private void DeterminePrimaryMob(Attack line)
        {
            // If it's You that is attacking of defending, then the other is the mob
            if (YouAre.IsThisYou(line.Attacker.Name))
            {
                PrimaryMob = line.Defender;
                return;
            }

            if (YouAre.IsThisYou(line.Defender.Name))
            {
                PrimaryMob = line.Attacker;
                return;
            }

            if (IsThisAMob(line.Attacker))
            {
                PrimaryMob = line.Attacker;
                return;
            }

            if (IsThisAMob(line.Defender))
            {
                PrimaryMob = line.Defender;
                return;
            }

            // If we've got multiple lines, we should be able to see who's the main target by the fact that everyone keeps beating them up
            if (!Fighters.Any())
                return;

            // Note: this finds the fighter who has the most combined attack lines
            // better would be to find the fight with the most distinct other fighters who have been attacking them
            var topFighter = Fighters.Aggregate((maxItem, nextItem) => (maxItem.DefensiveStatistics.Lines.Count + maxItem.OffensiveStatistics.Lines.Count) > (nextItem.DefensiveStatistics.Lines.Count + nextItem.OffensiveStatistics.Lines.Count) ? maxItem : nextItem);
            if (topFighter.DefensiveStatistics.Lines.Count + topFighter.OffensiveStatistics.Lines.Count >= 2)
                PrimaryMob = topFighter.Character;
        }

        private bool IsThisAMob(Character mob)
        {
            // Generic mob
            if (mob.Name.StartsWith("a "))
                return true;
            if (mob.Name.StartsWith("an "))
                return true;

            // How to tell if this is a named mob?
            // - a space in its name?
            if (mob.Name.Contains(' '))
                return true;

            return false;
        }
    }
}
