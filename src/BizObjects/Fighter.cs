using System.Collections.Generic;
using System.Linq;

namespace BizObjects
{
    public class Fighter
    {
        public Character Character { get; }
        public FightStatistics OffensiveStatistics { get; } = new FightStatistics();
        public FightStatistics DefensiveStatistics { get; } = new FightStatistics();

        public Fighter(Character character)
        {
            Character = character;
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
