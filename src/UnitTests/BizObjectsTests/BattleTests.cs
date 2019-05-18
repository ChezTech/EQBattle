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

    [TestClass]
    public class BattleTests
    {
        private static readonly YouResolver YouAre = new YouResolver("Khadaji");
        private LineParserFactory _parser = new LineParserFactory();
        private readonly IParser _hitParser = new HitParser(YouAre);
        private readonly IParser _missParser = new MissParser(YouAre);
        private readonly IParser _healParser = new HealParser(YouAre);
        private readonly IParser _killParser = new KillParser(YouAre);


        public BattleTests()
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
            var battle = new Battle();

            battle.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:16:42 2019] Khadaji hit a dwarf disciple for 2 points of magic damage by Distant Strike I.")));
            battle.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:16:45 2019] A dwarf disciple is pierced by YOUR thorns for 60 points of non-melee damage.")));
            battle.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:16:45 2019] A dwarf disciple punches YOU for 3241 points of damage.")));
            battle.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:16:47 2019] A dwarf disciple tries to punch YOU, but YOU riposte!")));
            battle.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:16:49 2019] You kick a dwarf disciple for 3041 points of damage. (Strikethrough)")));
            battle.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:16:50 2019] You try to crush a dwarf disciple, but miss!")));
            battle.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:16:50 2019] Movanna healed you over time for 2335 hit points by Elixir of the Ardent.")));
            battle.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:16:51 2019] Khadaji hit a dwarf disciple for 892 points of poison damage by Strike of Venom IV. (Critical)")));
            battle.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:16:52 2019] A dwarf disciple punches YOU for 865 points of damage.")));
            battle.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:17:20 2019] Khadaji hit a dwarf disciple for 512 points of chromatic damage by Lynx Maw.")));
            battle.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:17:21 2019] Khronick healed you over time for 3036 hit points by Healing Counterbias Effect. (Critical)")));
            battle.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:17:38 2019] Bealica hit a dwarf disciple for 11481 points of cold damage by Glacial Cascade.")));
            battle.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:17:57 2019] A dwarf disciple has been slain by Bealica!")));

            Assert.AreEqual(5, battle.Fighters.Count);
            Assert.IsTrue(battle.Fighters.Any(x => x.Character.Name == "Khadaji"));
            Assert.IsTrue(battle.Fighters.Any(x => x.Character.Name == "Movanna"));
            Assert.IsTrue(battle.Fighters.Any(x => x.Character.Name == "Khronick"));
            Assert.IsTrue(battle.Fighters.Any(x => x.Character.Name == "Bealica"));
            Assert.IsTrue(battle.Fighters.Any(x => x.Character.Name == "a dwarf disciple"));

            Assert.AreEqual(4507, battle.Fighters.First(x => x.Character.Name == "Khadaji").OffensiveStatistics.Hit.Total);
            Assert.AreEqual(11481, battle.Fighters.First(x => x.Character.Name == "Bealica").OffensiveStatistics.Hit.Total);
            Assert.AreEqual(4106, battle.Fighters.First(x => x.Character.Name == "a dwarf disciple").OffensiveStatistics.Hit.Total);
            Assert.AreEqual(20094, battle.OffensiveStatistics.Hit.Total);

            Assert.AreEqual(4106, battle.Fighters.First(x => x.Character.Name == "Khadaji").DefensiveStatistics.Hit.Total);
            Assert.AreEqual(0, battle.Fighters.First(x => x.Character.Name == "Bealica").DefensiveStatistics.Hit.Total);
            Assert.AreEqual(15988, battle.Fighters.First(x => x.Character.Name == "a dwarf disciple").DefensiveStatistics.Hit.Total);
            Assert.AreEqual(20094, battle.DefensiveStatistics.Hit.Total);

            Assert.AreEqual(5371, battle.Fighters.First(x => x.Character.Name == "Khadaji").DefensiveStatistics.Heal.Total);
            Assert.AreEqual(2335, battle.Fighters.First(x => x.Character.Name == "Movanna").OffensiveStatistics.Heal.Total);
            Assert.AreEqual(3036, battle.Fighters.First(x => x.Character.Name == "Khronick").OffensiveStatistics.Heal.Total);
            Assert.AreEqual(5371, battle.DefensiveStatistics.Heal.Total);
            Assert.AreEqual(5371, battle.OffensiveStatistics.Heal.Total);

            Assert.AreEqual(1, battle.Fighters.First(x => x.Character.Name == "Bealica").OffensiveStatistics.Kill.Count);
            Assert.AreEqual(0, battle.Fighters.First(x => x.Character.Name == "Bealica").DefensiveStatistics.Kill.Count);
            Assert.AreEqual(1, battle.Fighters.First(x => x.Character.Name == "a dwarf disciple").DefensiveStatistics.Kill.Count);
            Assert.AreEqual(0, battle.Fighters.First(x => x.Character.Name == "a dwarf disciple").OffensiveStatistics.Kill.Count);
            Assert.AreEqual(1, battle.OffensiveStatistics.Kill.Count);
            Assert.AreEqual(1, battle.OffensiveStatistics.Kill.Count);

            Assert.AreEqual(new TimeSpan(0, 0, 38), battle.Fighters.First(x => x.Character.Name == "Khadaji").OffensiveStatistics.Duration.EntireDuration);
            Assert.AreEqual(new TimeSpan(0, 0, 19), battle.Fighters.First(x => x.Character.Name == "Bealica").OffensiveStatistics.Duration.EntireDuration);
            Assert.AreEqual(new TimeSpan(0, 1, 15), battle.OffensiveStatistics.Duration.EntireDuration);

            Assert.AreEqual(118.61, battle.Fighters.First(x => x.Character.Name == "Khadaji").OffensiveStatistics.PerTime.DPS, 0.01);
            Assert.AreEqual(604.26, battle.Fighters.First(x => x.Character.Name == "Bealica").OffensiveStatistics.PerTime.DPS, 0.01);
            Assert.AreEqual(267.92, battle.OffensiveStatistics.PerTime.DPS, 0.01);
        }
    }
}
