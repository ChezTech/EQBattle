using System.Collections.Generic;
using System.Linq;

namespace BizObjects
{
    public class Fighter
    {
        private readonly Battle _fight;

        public Character Character { get; }
        public FightStatistics OffensiveStatistics { get; }
        public FightStatistics DefensiveStatistics { get; }

        public Fighter(Character character, Battle fight = null)
        {
            Character = character;
            _fight = fight;

            OffensiveStatistics = new FightStatistics(_fight);
            DefensiveStatistics  = new FightStatistics(_fight);
        }

        public void AddOffense(ILine line)
        {
            OffensiveStatistics.AddLine(line);
        }

        public void AddDefense(ILine line)
        {
            DefensiveStatistics.AddLine(line);
        }
    }
}
