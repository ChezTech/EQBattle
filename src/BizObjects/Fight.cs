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
        public ICollection<Fighter> Fighters { get { return _fighters.Values; } }
        public string Zone { get; }
        private ConcurrentDictionary<Character, Fighter> _fighters = new ConcurrentDictionary<Character, Fighter>();

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
    }
}
