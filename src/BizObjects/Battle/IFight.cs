using BizObjects.Lines;
using BizObjects.Statistics;
using System;
using System.Collections.Generic;

namespace BizObjects.Battle
{
    public interface IFight
    {
        bool IsFightOver { get; }
        IEnumerable<Fighter> Fighters { get; }
        FightStatistics Statistics { get; }
        Character PrimaryMob { get; }
        string Title { get; }
        DateTime LastAttackTime { get; }
        int LineCount { get; }
        Fighter PrimaryMobFighter { get; }

        void AddLine(ILine line);
        void AddLine(Attack line);
        void AddLine(Heal line);
        bool SimilarDamage(Hit line, bool looseMatch = false);
    }
}
