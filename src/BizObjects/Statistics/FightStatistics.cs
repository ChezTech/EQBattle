using BizObjects.Battle;
using BizObjects.Converters;
using BizObjects.Lines;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BizObjects.Statistics
{
    public class FightStatistics
    {
        private readonly IFight _fight;

        public FightStatistics(IFight fight = null)
        {
            _fight = fight;

            Hit = new HitPointStatistics<Hit>(Lines, x => x.Damage);
            Heal = new HitPointStatistics<Heal>(Lines, x => x.Amount);
            Miss = new CountStatistics<Miss>(Lines);
            Kill = new CountStatistics<Kill>(Lines);

            Duration = new DurationStatistics<ILine>(_fight, Lines);
            PerTime = new PerTimeStatistics<Hit, ILine>(Hit, Duration);
        }

        public IList<ILine> Lines { get; } = new List<ILine>();

        public HitPointStatistics<Hit> Hit { get; }
        public HitPointStatistics<Heal> Heal { get; }
        public CountStatistics<Miss> Miss { get; }
        public CountStatistics<Kill> Kill { get; }


        public DurationStatistics<ILine> Duration { get; }
        public PerTimeStatistics<Hit, ILine> PerTime { get; }


        public double HitPercentage
        {
            get
            {
                var totalCount = Hit.Count + Miss.Count;
                if (totalCount == 0)
                    return 0;
                return (double)Hit.Count / totalCount;
            }
        }

        public IDictionary<AttackType, HitPointStatistics<Hit>> HitStatsByType => Hit.Lines.Select(x => x.Type).Distinct().ToDictionary(x => x, x => new HitPointStatistics<Hit>(Hit.Lines.Where(y => y.Type == x), x => x.Damage));
        public IDictionary<AttackType, CountStatistics<Miss>> MissStatsByType => Miss.Lines.Select(x => x.Type).Distinct().ToDictionary(x => x, x => new CountStatistics<Miss>(Miss.Lines.Where(y => y.Type == x)));
        public IDictionary<string, HitPointStatistics<Heal>> HealsBySpell => Heal.Lines.Select(x => x.SpellName).Distinct().ToDictionary(x => x, x => new HitPointStatistics<Heal>(Heal.Lines.Where(y => y.SpellName == x), x => x.Amount));

        public void AddLine(ILine line)
        {
            Lines.Add(line);
        }
    }

    public class DurationStatistics<T> where T : ILine
    {
        private readonly IFight _fight;

        public DurationStatistics(IFight fight, IEnumerable<T> lines)
        {
            _fight = fight;
            Lines = lines;
        }

        public IEnumerable<T> Lines { get; }

        /// <Summary>
        /// Duration of the whole fight, from first pull to when the mobs are all dead
        /// </Summary>
        public TimeSpan FightDuration
        {
            get
            {
                return _fight == null
                  ? FighterDuration
                  : _fight.Statistics.Duration.FightDuration;
            }
        }

        /// <Summary>
        /// Duration from when a particular fighter started their main engagement in the fight
        /// </Summary>
        public TimeSpan FighterDuration
        {
            get
            {
                if (!Lines.Any())
                    return TimeSpan.Zero;
                return Lines.Last().Time - Lines.First().Time;
            }
        }
    }

    public class PerTimeStatistics<T, U>
        where T : class, ILine
        where U : class, ILine
    {
        private readonly TimeSpan SixSeconds = new TimeSpan(0, 0, 6);
        private readonly HitPointStatistics<T> _hitStats;
        private readonly DurationStatistics<U> _timeStats;

        public PerTimeStatistics(HitPointStatistics<T> hitStats, DurationStatistics<U> timeStats)
        {
            _hitStats = hitStats;
            _timeStats = timeStats;
        }

        public double FighterDPS
        {
            get => getDPS(_hitStats.Total, _timeStats.FighterDuration);
        }

        /// <Summary>
        /// Last six seconds DPS (last six since the latest line)
        /// </Summary>

        public double FighterDPSLastSixSeconds
        {
            get => getDPS(_hitStats.LastSixTotal, new[] { SixSeconds, _timeStats.FighterDuration }.Min());
        }

        public double FightDPS
        {
            get => getDPS(_hitStats.Total, _timeStats.FightDuration);
        }

        private double getDPS(int total, TimeSpan duration)
        {
            // One hit would give infinity DPS ... naw, let's cap it at your total damage
            if (duration == TimeSpan.Zero)
                return total;

            return total / duration.TotalSeconds;
        }

        // DPS, trailing 6s, trailing 12s, per fighterEngagement, perFight
    }
}
