using BizObjects.Lines;
using BizObjects.Statistics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace BizObjects.Battle
{
    public interface IFight : INotifyPropertyChanged
    {
        bool IsFightOver { get; }
        ObservableCollection<Fighter> Fighters { get; }
        FightStatistics Statistics { get; }
        Character PrimaryMob { get; }
        string Title { get; }
        DateTime LastAttackTime { get; }
        int LineCount { get; }
        Fighter PrimaryMobFighter { get; }

        void AddLine(ILine line);
        void AddLine(Attack line);
        void AddLine(Heal line);
    }

    public interface ISkirmish : IFight
    {
        ObservableCollection<IFight> Fights { get; }
    }
}
