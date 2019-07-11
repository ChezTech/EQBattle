using BizObjects.Lines;
using BizObjects.Statistics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace BizObjects.Battle
{
    public abstract class FightBase : IFight
    {
        protected ConcurrentDictionary<Character, Fighter> _fighters = new ConcurrentDictionary<Character, Fighter>();
        public IEnumerable<Fighter> Fighters => _fighters.Values;
        public FightStatistics Statistics { get; } = new FightStatistics();

        public abstract bool IsFightOver { get; }
        public Character PrimaryMob { get; protected set; } = Character.Unknown;
        public abstract string Title { get; }
        public abstract DateTime LastAttackTime { get; }
        public abstract int LineCount { get; }

        public void AddLine(ILine line) { }
        public abstract void AddLine(Attack line);
        public abstract void AddLine(Heal line);

        public virtual bool SimilarDamage(Hit line, bool looseMatch = false)
        {
            throw new NotImplementedException();
        }
        protected void AddFighterLine(Character fighterChar, Action<Fighter> addLine)
        {
            var fighter = _fighters.GetOrAdd(fighterChar, k => new Fighter(fighterChar, this));
            addLine(fighter);
        }
    }
}
