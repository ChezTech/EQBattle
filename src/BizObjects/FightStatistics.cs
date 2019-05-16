using System.Collections.Generic;
using System.Linq;

namespace BizObjects
{
    public class FightStatistics
    {
        public FightStatistics()
        {
            Damage = new HitPointStatistics<Hit>(Lines, x => x.Damage);
            Heals = new HitPointStatistics<Heal>(Lines, x => x.Amount);
        }

        public IList<ILine> Lines { get; } = new List<ILine>();


        public HitPointStatistics<Hit> Damage { get; }
        public HitPointStatistics<Heal> Heals { get; }


        public int MissCount { get => Lines.Where(x => x is Miss).Count(); }
        public int KillCount { get => Lines.Where(x => x is Kill).Select(x => x as Kill).Count(); }
        public double HitPercentage { get => (double)Damage.Count / (Damage.Count + MissCount); }
        public IDictionary<string, int> DamagePerType { get; }
        public IDictionary<string, int> HitsPerType { get; }
        public IDictionary<string, int> MissesPerType { get; }

        public void AddLine(ILine line)
        {
            Lines.Add(line);
        }
    }
}
