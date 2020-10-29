using BizObjects.Lines;
using BizObjects.Statistics;
using Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BizObjects.Battle
{
    public abstract class FightBase : PropertyChangeBase, IFight
    {
        public ObservableCollection<ILine> Lines { get; } = new ObservableCollection<ILine>();

        protected ConcurrentDictionary<Character, Fighter> _fighters = new ConcurrentDictionary<Character, Fighter>();
        private Character primaryMob = Character.Unknown;

        public ObservableCollection<Fighter> Fighters { get; } = new ObservableCollection<Fighter>();
        public FightStatistics Statistics { get; } = new FightStatistics();

        public abstract bool IsFightOver { get; }
        
        public Character PrimaryMob
        {
            get => primaryMob;
            protected set
            {
                if (SetProperty(ref primaryMob, value))
                    OnPropertyChanged(nameof(PrimaryMobFighter));
            }
        }

        public abstract string Title { get; }
        public abstract DateTime LastAttackTime { get; }
        public abstract int LineCount { get; }

        public Fighter PrimaryMobFighter { get => Fighters.Where(x => x.Character == PrimaryMob).DefaultIfEmpty(new Fighter(PrimaryMob, this)).First(); }

        public virtual void AddLine(ILine line)
        {
            Lines.Add(line);
        }

        public abstract void AddLine(Attack line);
        public abstract void AddLine(Heal line);

        protected void AddFighterLine(Character fighterChar, Action<Fighter> addLine)
        {
            var fighter = _fighters.GetOrAdd(fighterChar, k =>
            {
                var newFighter = new Fighter(fighterChar, this);
                Fighters.Add(newFighter);
                return newFighter;
            });
            addLine(fighter);
        }
    }
}
