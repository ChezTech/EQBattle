using BizObjects.Lines;
using BizObjects.Statistics;
using Core;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BizObjects.Battle
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Fighter : PropertyChangeBase
    {
        private readonly IFight _fight;
        private Character character;

        public Character Character { get => character; private set => SetProperty(ref character, value); }
        public FightStatistics OffensiveStatistics { get; }
        public FightStatistics DefensiveStatistics { get; }

        // Is currently dead. In the case of a PC, if they get rez'd, this should change back to false. (rezone, is there a rezone if you die in zone you're bound to?, or subsequent attacks, what if there's still your DoT on a mob?)
        public bool IsDead { get => DefensiveStatistics.Kill.Count > 0; }

        public Fighter(Character character, IFight fight = null)
        {
            Character = character;
            _fight = fight;

            OffensiveStatistics = new FightStatistics(_fight);
            DefensiveStatistics = new FightStatistics(_fight);
        }

        public void AddOffense(ILine line)
        {
            OffensiveStatistics.AddLine(line);
            OnPropertyChanged(nameof(OffensiveStatistics));
        }

        public void AddDefense(ILine line)
        {
            DefensiveStatistics.AddLine(line);
            OnPropertyChanged(nameof(DefensiveStatistics));
        }

        private string DebuggerDisplay
        {
            get
            {
                return string.Format($"{Character.Name}{(IsDead ? " - dead" : "")}");
            }
        }
    }
}
