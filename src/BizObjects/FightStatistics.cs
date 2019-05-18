using System;
using System.Collections.Generic;
using System.Linq;

namespace BizObjects
{
    public class FightStatistics
    {
        public FightStatistics()
        {
            Hit = new HitPointStatistics<Hit>(Lines, x => x.Damage);
            Heal = new HitPointStatistics<Heal>(Lines, x => x.Amount);
            Miss = new CountStatistics<Miss>(Lines);
            Kill = new CountStatistics<Kill>(Lines);

            DPS = new DurationStatistics<ILine>(Lines);
        }

        public IList<ILine> Lines { get; } = new List<ILine>();

        public HitPointStatistics<Hit> Hit { get; }
        public HitPointStatistics<Heal> Heal { get; }
        public CountStatistics<Miss> Miss { get; }
        public CountStatistics<Kill> Kill { get; }

        public DurationStatistics<ILine> DPS { get; }


        public double HitPercentage { get => (double)Hit.Count / (Hit.Count + Miss.Count); }

        public IEnumerable<IGrouping<AttackType, Hit>> HitsPerType { get => Hit.Lines.GroupBy(x => x.Type); }
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

    public class DurationStatistics<T> where T : ILine
    {
        public DurationStatistics(IEnumerable<T> lines)
        {
            Lines = lines;
        }

        public IEnumerable<T> Lines { get; }

        /// <Summary>
        /// Duration of the whole fight, from first pull to when the mobs are all dead
        /// </Summary>
        public TimeSpan EntireDuration
        {
            get
            {
                if (!Lines.Any())
                    return TimeSpan.Zero;
                return Lines.Last().Time - Lines.First().Time;
            }
        }

        /// <Summary>
        /// Duration from the main engagement till death
        /// </Summary>
        public TimeSpan PrimaryDuration { get; }

        /// <Summary>
        /// Duration from when a particular fighter started their main engagement in the fight
        /// </Summary>
        public TimeSpan FighterDuration { get; }
    }
}
