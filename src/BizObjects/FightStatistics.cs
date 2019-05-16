using System.Collections.Generic;
using System.Linq;

namespace BizObjects
{
    public class FightStatistics
    {
        public IList<ILine> Lines { get; } = new List<ILine>();

        public int TotalDamage { get => Lines.Where(x => x is Hit).Select(x => x as Hit).Sum(x => x.Damage); }
        public int MaxHitDamage { get => Lines.Where(x => x is Hit).Select(x => x as Hit).Max(x => x.Damage); }
        public int MinHitDamage { get => Lines.Where(x => x is Hit).Select(x => x as Hit).Min(x => x.Damage); }
        public int HitCount { get; }
        public int MissCount { get; }
        public int KillCount { get => Lines.Where(x => x is Kill).Select(x => x as Kill).Count(); }
        public double HitPercentage { get; }
        public int HealAmount { get => Lines.Where(x => x is Heal).Select(x => x as Heal).Sum(x => x.Amount); }
        public IDictionary<string, int> DamagePerType { get; }
        public IDictionary<string, int> HitsPerType { get; }
        public IDictionary<string, int> MissesPerType { get; }

        public void AddLine(ILine line)
        {
            Lines.Add(line);
        }
    }
}
