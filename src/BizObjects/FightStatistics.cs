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
            Miss = new CountStatistics<Miss>(Lines);
            Kill = new CountStatistics<Kill>(Lines);
        }

        public IList<ILine> Lines { get; } = new List<ILine>();


        public HitPointStatistics<Hit> Damage { get; }
        public HitPointStatistics<Heal> Heals { get; }
        public CountStatistics<Miss> Miss { get; }
        public CountStatistics<Kill> Kill { get; }


        public double HitPercentage { get => (double)Damage.Count / (Damage.Count + Miss.Count); }
        public IDictionary<string, int> DamagePerType { get; }
        public IDictionary<string, int> HitsPerType { get; }
        public IDictionary<string, int> MissesPerType { get; }

        public void AddLine(ILine line)
        {
            Lines.Add(line);
        }
    }
}
