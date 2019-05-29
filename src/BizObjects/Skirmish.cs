using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace BizObjects
{
    public class Skirmish : IFight
    {
        public Skirmish(YouResolver youAre)
        {
        }

        public IList<IFight> Fights { get; }

        public bool IsFightOver => Fights.All(x => x.IsFightOver);

        private ConcurrentDictionary<Character, Fighter> _fighters = new ConcurrentDictionary<Character, Fighter>();
        public IEnumerable<Fighter> Fighters => _fighters.Values;

        public FightStatistics OffensiveStatistics { get; } = new FightStatistics();
        public FightStatistics DefensiveStatistics { get; } = new FightStatistics();

        public void AddLine(Attack line)
        {
            // Find which fight this Attack belongs to and put it in there...


            var attackChar = _fighters.GetOrAdd(line.Attacker, new Fighter(line.Attacker, this));
            attackChar.AddOffense(line);
            OffensiveStatistics.AddLine(line);

            var defendChar = _fighters.GetOrAdd(line.Defender, new Fighter(line.Defender, this));
            defendChar.AddDefense(line);
            DefensiveStatistics.AddLine(line);
        }

        public void AddLine(Heal line)
        {
            var healerChar = _fighters.GetOrAdd(line.Healer, new Fighter(line.Healer));
            healerChar.AddOffense(line);
            OffensiveStatistics.AddLine(line);

            var patientChar = _fighters.GetOrAdd(line.Patient, new Fighter(line.Patient));
            patientChar.AddDefense(line);
            DefensiveStatistics.AddLine(line);
        }

        public void AddLine(ILine line)
        {
        }
    }
}
