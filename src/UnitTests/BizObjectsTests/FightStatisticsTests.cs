using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BizObjects.Converters;
using BizObjects.Lines;
using BizObjects.Statistics;
using LineParser;
using LineParser.Parsers;
using LogObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BizObjectsTests
{
    [TestClass]
    public class FightStatisticsTests : ParserTestBase
    {
        private readonly Action<FightStatistics, string> AddFightStats = (fightStats, logLine) =>
        {
            ILine line = _parser.ParseLine(new LogDatum(logLine));
            fightStats.AddLine((dynamic)line);
        };
        private readonly Func<string, ILine> ParseLine = logLine => _parser.ParseLine(new LogDatum(logLine));

        [TestMethod]
        public void SmallFightBasicStats()
        {
            var fightStats = new FightStatistics();

            // Offensive LogLines only
            AddFightStats(fightStats, "[Fri Apr 05 16:16:42 2019] Khadaji hit a dwarf disciple for 2 points of magic damage by Distant Strike I.");
            AddFightStats(fightStats, "[Fri Apr 05 16:16:45 2019] A dwarf disciple is pierced by YOUR thorns for 60 points of non-melee damage.");
            AddFightStats(fightStats, "[Fri Apr 05 16:16:49 2019] You kick a dwarf disciple for 3041 points of damage. (Strikethrough)");
            AddFightStats(fightStats, "[Fri Apr 05 16:16:50 2019] You try to crush a dwarf disciple, but miss!");
            AddFightStats(fightStats, "[Fri Apr 05 16:16:51 2019] Khadaji hit a dwarf disciple for 892 points of poison damage by Strike of Venom IV. (Critical)");
            AddFightStats(fightStats, "[Fri Apr 05 16:17:20 2019] Khadaji hit a dwarf disciple for 512 points of chromatic damage by Lynx Maw.");

            Assert.AreEqual(4507, fightStats.Hit.Total);
            Assert.AreEqual(3041, fightStats.Hit.Max);
            Assert.AreEqual(2, fightStats.Hit.Min);
            Assert.AreEqual(5, fightStats.Hit.Count);
            Assert.AreEqual(901.4, fightStats.Hit.Average, 0.01);
            Assert.AreEqual(0.833, fightStats.HitPercentage, 0.001);

            Assert.AreEqual(1, fightStats.Miss.Count);
            Assert.AreEqual(0, fightStats.Kill.Count);
        }

        [TestMethod]
        public void ContinuedFight()
        {
            var fightStats = new FightStatistics();

            // First part of the fight
            AddFightStats(fightStats, "[Fri Apr 05 16:16:42 2019] Khadaji hit a dwarf disciple for 2 points of magic damage by Distant Strike I.");
            AddFightStats(fightStats, "[Fri Apr 05 16:16:45 2019] A dwarf disciple is pierced by YOUR thorns for 60 points of non-melee damage.");
            AddFightStats(fightStats, "[Fri Apr 05 16:16:49 2019] You kick a dwarf disciple for 3041 points of damage. (Strikethrough)");

            Assert.AreEqual(3103, fightStats.Hit.Total);
            Assert.AreEqual(3, fightStats.Hit.Count);
            Assert.AreEqual(0, fightStats.Miss.Count);

            // Add some more lines
            AddFightStats(fightStats, "[Fri Apr 05 16:16:50 2019] You try to crush a dwarf disciple, but miss!");
            AddFightStats(fightStats, "[Fri Apr 05 16:16:51 2019] Khadaji hit a dwarf disciple for 892 points of poison damage by Strike of Venom IV. (Critical)");
            AddFightStats(fightStats, "[Fri Apr 05 16:17:20 2019] Khadaji hit a dwarf disciple for 512 points of chromatic damage by Lynx Maw.");

            Assert.AreEqual(4507, fightStats.Hit.Total);
            Assert.AreEqual(3041, fightStats.Hit.Max);
            Assert.AreEqual(2, fightStats.Hit.Min);
            Assert.AreEqual(5, fightStats.Hit.Count);
            Assert.AreEqual(901.4, fightStats.Hit.Average, 0.01);
            Assert.AreEqual(0.833, fightStats.HitPercentage, 0.001);

            Assert.AreEqual(1, fightStats.Miss.Count);
            Assert.AreEqual(0, fightStats.Kill.Count);
        }

        [TestMethod]
        public void DefensiveStats()
        {
            var fightStats = new FightStatistics();

            // Defensive LogLines only
            AddFightStats(fightStats, "[Fri Apr 05 16:16:45 2019] A dwarf disciple punches YOU for 3241 points of damage.");
            AddFightStats(fightStats, "[Fri Apr 05 16:16:47 2019] A dwarf disciple tries to punch YOU, but YOU riposte!");
            AddFightStats(fightStats, "[Fri Apr 05 16:16:50 2019] Movanna healed you over time for 2335 hit points by Elixir of the Ardent.");
            AddFightStats(fightStats, "[Fri Apr 05 16:16:52 2019] A dwarf disciple punches YOU for 865 points of damage.");
            AddFightStats(fightStats, "[Fri Apr 05 16:17:21 2019] Khronick healed you over time for 3036 hit points by Healing Counterbias Effect. (Critical)");

            Assert.AreEqual(4106, fightStats.Hit.Total);
            Assert.AreEqual(3241, fightStats.Hit.Max);
            Assert.AreEqual(865, fightStats.Hit.Min);
            Assert.AreEqual(2, fightStats.Hit.Count);
            Assert.AreEqual(0.666, fightStats.HitPercentage, 0.001);

            Assert.AreEqual(5371, fightStats.Heal.Total);
            Assert.AreEqual(2, fightStats.Heal.Count);

            Assert.AreEqual(1, fightStats.Miss.Count);
            Assert.AreEqual(0, fightStats.Kill.Count);
        }

        [TestMethod]
        public void HitTypeTests()
        {
            var fightStats = new FightStatistics();

            // First part of the fight
            AddFightStats(fightStats, "[Fri Apr 05 16:16:42 2019] Khadaji hit a dwarf disciple for 2 points of magic damage by Distant Strike I.");
            AddFightStats(fightStats, "[Fri Apr 05 16:16:45 2019] A dwarf disciple is pierced by YOUR thorns for 60 points of non-melee damage.");
            AddFightStats(fightStats, "[Fri Apr 05 16:16:49 2019] You kick a dwarf disciple for 3041 points of damage. (Strikethrough)");
            AddFightStats(fightStats, "[Fri Apr 05 16:16:51 2019] Khadaji hit a dwarf disciple for 892 points of poison damage by Strike of Venom IV. (Critical)");

            var hitTypeStats = fightStats.HitStatsByType;
            Assert.AreEqual(3, hitTypeStats.Count());

            Assert.IsTrue(hitTypeStats.Keys.Contains(AttackType.Hit));
            Assert.AreEqual(894, hitTypeStats[AttackType.Hit].Total);

            Assert.IsTrue(hitTypeStats.Keys.Contains(AttackType.Pierce));
            Assert.AreEqual(60, hitTypeStats[AttackType.Pierce].Total);

            Assert.IsTrue(hitTypeStats.Keys.Contains(AttackType.Kick));
            Assert.AreEqual(3041, hitTypeStats[AttackType.Kick].Total);
        }

        [TestMethod]
        public void DpsTestForFighter()
        {
            var fightStats = new FightStatistics();

            AddFightStats(fightStats, "[Fri Apr 05 16:16:42 2019] Khadaji hit a dwarf disciple for 2 points of magic damage by Distant Strike I.");
            AddFightStats(fightStats, "[Fri Apr 05 16:16:45 2019] A dwarf disciple is pierced by YOUR thorns for 60 points of non-melee damage.");
            AddFightStats(fightStats, "[Fri Apr 05 16:16:49 2019] You kick a dwarf disciple for 3041 points of damage. (Strikethrough)");
            AddFightStats(fightStats, "[Fri Apr 05 16:16:51 2019] Khadaji hit a dwarf disciple for 892 points of poison damage by Strike of Venom IV. (Critical)");

            Assert.AreEqual(3995, fightStats.Hit.Total);
            Assert.AreEqual(new TimeSpan(0, 0, 9), fightStats.Duration.FighterDuration);
            Assert.AreEqual(443.89, fightStats.PerTime.FighterDPS, 0.01);
        }

        [TestMethod]
        public void LongTestForSlidingWindows()
        {
            var fightStats = new FightStatistics();

            VerifyLastSix(fightStats, 0, 0);

            AddFightStats(fightStats, "[Tue May 28 06:18:10 2019] You kick Gomphus for 1288 points of damage. (Riposte)");
            AddFightStats(fightStats, "[Tue May 28 06:18:10 2019] You crush Gomphus for 6528 points of damage. (Riposte Critical)");
            AddFightStats(fightStats, "[Tue May 28 06:18:13 2019] You crush Gomphus for 1472 points of damage.");
            VerifyLastSix(fightStats, 9288, 3096); // DPS here should be based on how long the fight is so far, since that's less than six seconds

            AddFightStats(fightStats, "[Tue May 28 06:18:14 2019] Gomphus is pierced by YOUR thorns for 60 points of non-melee damage.");
            AddFightStats(fightStats, "[Tue May 28 06:18:14 2019] You kick Gomphus for 2598 points of damage.");
            AddFightStats(fightStats, "[Tue May 28 06:18:14 2019] Gomphus is pierced by YOUR thorns for 60 points of non-melee damage.");
            AddFightStats(fightStats, "[Tue May 28 06:18:15 2019] You strike Gomphus for 9723 points of damage. (Critical)");
            AddFightStats(fightStats, "[Tue May 28 06:18:15 2019] You hit Gomphus for 22528 points of physical damage by Five Point Palm VI.");
            AddFightStats(fightStats, "[Tue May 28 06:18:18 2019] You crush Gomphus for 6795 points of damage. (Critical)");
            VerifyLastSix(fightStats, 43236, 7206);

            AddFightStats(fightStats, "[Tue May 28 06:18:20 2019] Gomphus is pierced by YOUR thorns for 60 points of non-melee damage.");
            AddFightStats(fightStats, "[Tue May 28 06:18:20 2019] You kick Gomphus for 7078 points of damage. (Critical)");
            AddFightStats(fightStats, "[Tue May 28 06:18:22 2019] You kick Gomphus for 9605 points of damage. (Riposte Critical)");
            AddFightStats(fightStats, "[Tue May 28 06:18:23 2019] You strike Gomphus for 598 points of damage. (Strikethrough)");
            VerifyLastSix(fightStats, 24136, 4022.66);

            AddFightStats(fightStats, "[Tue May 28 06:18:26 2019] You strike Gomphus for 514 points of damage.");
            AddFightStats(fightStats, "[Tue May 28 06:18:27 2019] You hit Gomphus for 388 points of poison damage by Strike of Venom IV.");
            AddFightStats(fightStats, "[Tue May 28 06:18:27 2019] You kick Gomphus for 7938 points of damage. (Riposte)");
            AddFightStats(fightStats, "[Tue May 28 06:18:28 2019] You strike Gomphus for 1208 points of damage.");
            AddFightStats(fightStats, "[Tue May 28 06:18:28 2019] You kick Gomphus for 2905 points of damage. (Strikethrough Critical)");
            VerifyLastSix(fightStats, 13551, 2258.5); // Note: this is not inclusive of ":22 9605 damage" hit, the six seconds are [:23 - :28]

            AddFightStats(fightStats, "[Tue May 28 06:18:29 2019] Gomphus is pierced by YOUR thorns for 60 points of non-melee damage.");
            AddFightStats(fightStats, "[Tue May 28 06:18:29 2019] You kick Gomphus for 7938 points of damage.");
            AddFightStats(fightStats, "[Tue May 28 06:18:30 2019] You crush Gomphus for 663 points of damage.");
            AddFightStats(fightStats, "[Tue May 28 06:18:30 2019] You kick Gomphus for 4021 points of damage.");

            // Basic tests
            Assert.AreEqual(94028, fightStats.Hit.Total);
            Assert.AreEqual(new TimeSpan(0, 0, 20), fightStats.Duration.FighterDuration);
            Assert.AreEqual(4701.4, fightStats.PerTime.FighterDPS, 0.01);
            VerifyLastSix(fightStats, 25635, 4272.5);
        }

        private void VerifyLastSix(FightStatistics fightStats, int total, double dps)
        {
            Assert.AreEqual(total, fightStats.Hit.LastSixTotal);
            Assert.AreEqual(dps, fightStats.PerTime.FighterDPSLastSixSeconds, 0.01);
        }

        [TestMethod]
        public void EmptyStats()
        {
            var fightStats = new FightStatistics();

            Assert.AreEqual(0, fightStats.HitPercentage);

            Assert.AreEqual(TimeSpan.Zero, fightStats.Duration.FightDuration);
            Assert.AreEqual(TimeSpan.Zero, fightStats.Duration.FighterDuration);

            Assert.AreEqual(0.0, fightStats.PerTime.FightDPS);
            Assert.AreEqual(0.0, fightStats.PerTime.FighterDPS);
            Assert.AreEqual(0.0, fightStats.PerTime.FighterDPSLastSixSeconds);

            Assert.AreEqual(0, fightStats.Hit.Average);
            Assert.AreEqual(0, fightStats.Hit.Count);
            Assert.AreEqual(0, fightStats.Hit.LastSixTotal);
            Assert.AreEqual(0, fightStats.Hit.Max);
            Assert.AreEqual(0, fightStats.Hit.Min);
            Assert.AreEqual(0, fightStats.Hit.Total);
        }
    }
}
