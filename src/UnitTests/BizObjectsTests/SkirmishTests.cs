using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BizObjects;
using LineParser;
using LineParser.Parsers;
using LogObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BizObjectsTests
{
    // Notes:
    // https://forums.daybreakgames.com/eq/index.php?threads/dps-parsers.232663/
    // https://github.com/rumstil
    // http://www.eqpixie.com/
    // http://www.eqwatcher.com/
    // https://eq.gimasoft.com/gina/Default.aspx

    [TestClass]
    public class SkirmishTests
    {
        private static readonly YouResolver YouAre = new YouResolver("Khadaji");
        private LineParserFactory _parser = new LineParserFactory();
        private readonly IParser _hitParser = new HitParser(YouAre);
        private readonly IParser _missParser = new MissParser(YouAre);
        private readonly IParser _healParser = new HealParser(YouAre);
        private readonly IParser _killParser = new KillParser(YouAre);


        public SkirmishTests()
        {
            _parser.AddParser(_hitParser, null);
            _parser.AddParser(_missParser, null);
            _parser.AddParser(_healParser, null);
            _parser.AddParser(_killParser, null);
        }

        [TestMethod]
        public void SmallFight()
        {
            var pc = new Character(YouAre.Name);
            var skirmish = new Skirmish(YouAre);

            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:01:03 2019] Gomphus tries to hit YOU, but YOU block!")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:01:17 2019] Khronick is bathed in a zealous light. Movanna healed Khronick for 9197 (23333) hit points by Zealous Light.")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:01:20 2019] Harvester Collyx tries to hit Movanna, but Movanna blocks!")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:01:23 2019] Gomphus hits Movanna for 3870 points of damage.")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:01:23 2019] Harvester Collyx hits Movanna for 2566 points of damage.")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:01:43 2019] You kick Gomphus for 1207 points of damage.")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:02:16 2019] Khronick feels the touch of recuperation. Movanna healed Khronick for 1827 (23605) hit points by Word of Recuperation.")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:02:16 2019] Movanna feels the touch of recuperation. Movanna healed herself for 23605 hit points by Word of Recuperation.")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:02:16 2019] You feel the touch of recuperation. Movanna healed you for 20053 (23605) hit points by Word of Recuperation.")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:02:16 2019] Movanna has taken 3000 damage by Noxious Visions.")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:02:56 2019] Movanna crushes Gomphus for 322 points of damage. (Riposte)")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:03:01 2019] Khronick healed you over time for 1518 hit points by Healing Counterbias Effect.")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:03:19 2019] Harvester Collyx has taken 8206 damage from Blood of Jaled'Dar by Khronick. (Critical)")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:03:30 2019] Khronick has taken 1950 damage from Noxious Visions by Gomphus.")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:03:53 2019] You kick Harvester Collyx for 13333 points of damage. (Riposte Critical)")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:03:53 2019] You have slain Harvester Collyx!")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:04:04 2019] Gomphus hits Movanna for 2076 points of damage.")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:04:04 2019] Movanna has been slain by Gomphus!")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:04:36 2019] Gomphus hits Khronick for 6177 points of damage.")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:04:36 2019] Khronick has been slain by Gomphus!")));

            Assert.AreEqual(6, skirmish.Fighters.Count());
            Assert.IsTrue(skirmish.Fighters.Any(x => x.Character.Name == "Khadaji"));
            Assert.IsTrue(skirmish.Fighters.Any(x => x.Character.Name == "Movanna"));
            Assert.IsTrue(skirmish.Fighters.Any(x => x.Character.Name == "Khronick"));
            Assert.IsTrue(skirmish.Fighters.Any(x => x.Character.Name == "Gomphus"));
            Assert.IsTrue(skirmish.Fighters.Any(x => x.Character.Name == "Harvester Collyx"));
            Assert.IsTrue(skirmish.Fighters.Any(x => x.Character.Name == "Unknown"));

            // Skirmish stats
            Assert.AreEqual(42707, skirmish.OffensiveStatistics.Hit.Total);
            Assert.AreEqual(42707, skirmish.DefensiveStatistics.Hit.Total);
            Assert.AreEqual(56200, skirmish.DefensiveStatistics.Heal.Total);
            Assert.AreEqual(56200, skirmish.OffensiveStatistics.Heal.Total);
            Assert.AreEqual(3, skirmish.OffensiveStatistics.Kill.Count);
            Assert.AreEqual(3, skirmish.OffensiveStatistics.Kill.Count);






            // Assert.AreEqual(4507, skirmish.Fighters.First(x => x.Character.Name == "Khadaji").OffensiveStatistics.Hit.Total);
            // Assert.AreEqual(11481, skirmish.Fighters.First(x => x.Character.Name == "Bealica").OffensiveStatistics.Hit.Total);
            // Assert.AreEqual(4106, skirmish.Fighters.First(x => x.Character.Name == "a dwarf disciple").OffensiveStatistics.Hit.Total);
            // Assert.AreEqual(20094, skirmish.OffensiveStatistics.Hit.Total);

            // Assert.AreEqual(4106, skirmish.Fighters.First(x => x.Character.Name == "Khadaji").DefensiveStatistics.Hit.Total);
            // Assert.AreEqual(0, skirmish.Fighters.First(x => x.Character.Name == "Bealica").DefensiveStatistics.Hit.Total);
            // Assert.AreEqual(15988, skirmish.Fighters.First(x => x.Character.Name == "a dwarf disciple").DefensiveStatistics.Hit.Total);
            // Assert.AreEqual(20094, skirmish.DefensiveStatistics.Hit.Total);

            // Assert.AreEqual(5371, skirmish.Fighters.First(x => x.Character.Name == "Khadaji").DefensiveStatistics.Heal.Total);
            // Assert.AreEqual(2335, skirmish.Fighters.First(x => x.Character.Name == "Movanna").OffensiveStatistics.Heal.Total);
            // Assert.AreEqual(3036, skirmish.Fighters.First(x => x.Character.Name == "Khronick").OffensiveStatistics.Heal.Total);
            // Assert.AreEqual(5371, skirmish.DefensiveStatistics.Heal.Total);
            // Assert.AreEqual(5371, skirmish.OffensiveStatistics.Heal.Total);

            // Assert.AreEqual(1, skirmish.Fighters.First(x => x.Character.Name == "Bealica").OffensiveStatistics.Kill.Count);
            // Assert.AreEqual(0, skirmish.Fighters.First(x => x.Character.Name == "Bealica").DefensiveStatistics.Kill.Count);
            // Assert.AreEqual(1, skirmish.Fighters.First(x => x.Character.Name == "a dwarf disciple").DefensiveStatistics.Kill.Count);
            // Assert.AreEqual(0, skirmish.Fighters.First(x => x.Character.Name == "a dwarf disciple").OffensiveStatistics.Kill.Count);
            // Assert.AreEqual(1, skirmish.OffensiveStatistics.Kill.Count);
            // Assert.AreEqual(1, skirmish.OffensiveStatistics.Kill.Count);

            // Assert.AreEqual(new TimeSpan(0, 0, 38), skirmish.Fighters.First(x => x.Character.Name == "Khadaji").OffensiveStatistics.Duration.FighterDuration);
            // Assert.AreEqual(new TimeSpan(0, 0, 19), skirmish.Fighters.First(x => x.Character.Name == "Bealica").OffensiveStatistics.Duration.FighterDuration);
            // Assert.AreEqual(new TimeSpan(0, 1, 15), skirmish.OffensiveStatistics.Duration.FighterDuration);

            // Assert.AreEqual(118.61, skirmish.Fighters.First(x => x.Character.Name == "Khadaji").OffensiveStatistics.PerTime.FighterDPS, 0.01);
            // Assert.AreEqual(604.26, skirmish.Fighters.First(x => x.Character.Name == "Bealica").OffensiveStatistics.PerTime.FighterDPS, 0.01);
            // Assert.AreEqual(267.92, skirmish.OffensiveStatistics.PerTime.FighterDPS, 0.01);

            // Assert.AreEqual(new TimeSpan(0, 1, 15), skirmish.Fighters.First(x => x.Character.Name == "Khadaji").OffensiveStatistics.Duration.FightDuration);
            // Assert.AreEqual(new TimeSpan(0, 1, 15), skirmish.Fighters.First(x => x.Character.Name == "Bealica").OffensiveStatistics.Duration.FightDuration);
            // Assert.AreEqual(new TimeSpan(0, 1, 15), skirmish.OffensiveStatistics.Duration.FightDuration);

            // Assert.AreEqual(60.09, skirmish.Fighters.First(x => x.Character.Name == "Khadaji").OffensiveStatistics.PerTime.FightDPS, 0.01);
            // Assert.AreEqual(153.08, skirmish.Fighters.First(x => x.Character.Name == "Bealica").OffensiveStatistics.PerTime.FightDPS, 0.01);
        }
    }
}
