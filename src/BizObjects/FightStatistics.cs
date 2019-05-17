using System.Collections.Generic;
using System.Linq;

namespace BizObjects
{
    public class FightStatistics
    {
        public FightStatistics()
        {
            Damage = new HitPointStatistics<Hit>(Lines, x => x.Damage);
            Heal = new HitPointStatistics<Heal>(Lines, x => x.Amount);
            Miss = new CountStatistics<Miss>(Lines);
            Kill = new CountStatistics<Kill>(Lines);
        }

        public IList<ILine> Lines { get; } = new List<ILine>();


        public HitPointStatistics<Hit> Damage { get; }
        public HitPointStatistics<Heal> Heal { get; }
        public CountStatistics<Miss> Miss { get; }
        public CountStatistics<Kill> Kill { get; }


        public double HitPercentage { get => (double)Damage.Count / (Damage.Count + Miss.Count); }

        public IEnumerable<IGrouping<AttackType, Hit>> HitsPerType { get => Damage.Lines.GroupBy(x => x.Type); }
        public IEnumerable<AttackType> HitTypes { get => HitsPerType.Select(x => x.Key); }
        public HitPointStatistics<Hit> GetHitStatisticsForAttackType(AttackType attackType)
        {
            return new HitPointStatistics<Hit>(
                HitsPerType
                    .Where(x => x.Key == attackType)
                    .SelectMany(x => x)
                , z => z.Damage);
        }
        public IEnumerable<IGrouping<AttackType, Miss>> MissesPerType { get => Miss.Lines.GroupBy(x => x.Type); }
        public IEnumerable<IGrouping<string, Heal>> HealsPerSpell { get => Heal.Lines.GroupBy(x => x.SpellName); }

        public void AddLine(ILine line)
        {
            Lines.Add(line);
        }
    }
}
