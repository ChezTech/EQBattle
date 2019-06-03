using System;
using System.Collections.Generic;

namespace BizObjects
{
    public interface IFight
    {
        bool IsFightOver { get; }
        IEnumerable<Fighter> Fighters { get; }
        FightStatistics OffensiveStatistics { get; }
        FightStatistics DefensiveStatistics { get; }
        Character PrimaryMob { get; }
        string Title {get;}
        DateTime LastAttackTime { get; }

        void AddLine(ILine line);
        void AddLine(Attack line);
        void AddLine(Heal line);
    }
}
