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
    public class FightTests
    {
        private static readonly YouResolver YouAre = new YouResolver("Khadaji");
        private LineParserFactory _parser = new LineParserFactory();
        private readonly IParser _hitParser = new HitParser(YouAre);
        private readonly IParser _missParser = new MissParser(YouAre);
        private readonly IParser _healParser = new HealParser(YouAre);
        private readonly IParser _killParser = new KillParser(YouAre);


        public FightTests()
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
            var fight = new Fight(YouAre);

            fight.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:16:42 2019] Khadaji hit a dwarf disciple for 2 points of magic damage by Distant Strike I.")));
            fight.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:16:45 2019] A dwarf disciple is pierced by YOUR thorns for 60 points of non-melee damage.")));
            fight.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:16:45 2019] A dwarf disciple punches YOU for 3241 points of damage.")));
            fight.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:16:47 2019] A dwarf disciple tries to punch YOU, but YOU riposte!")));
            fight.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:16:49 2019] You kick a dwarf disciple for 3041 points of damage. (Strikethrough)")));
            fight.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:16:50 2019] You try to crush a dwarf disciple, but miss!")));
            fight.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:16:50 2019] Movanna healed you over time for 2335 hit points by Elixir of the Ardent.")));
            fight.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:16:51 2019] Khadaji hit a dwarf disciple for 892 points of poison damage by Strike of Venom IV. (Critical)")));
            fight.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:16:52 2019] A dwarf disciple punches YOU for 865 points of damage.")));
            fight.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:17:20 2019] Khadaji hit a dwarf disciple for 512 points of chromatic damage by Lynx Maw.")));
            fight.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:17:21 2019] Khronick healed you over time for 3036 hit points by Healing Counterbias Effect. (Critical)")));
            fight.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:17:38 2019] Bealica hit a dwarf disciple for 11481 points of cold damage by Glacial Cascade.")));
            fight.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:17:57 2019] A dwarf disciple has been slain by Bealica!")));

            Assert.AreEqual(5, fight.Fighters.Count);
            Assert.IsTrue(fight.Fighters.Any(x => x.Character.Name == "Khadaji"));
            Assert.IsTrue(fight.Fighters.Any(x => x.Character.Name == "Movanna"));
            Assert.IsTrue(fight.Fighters.Any(x => x.Character.Name == "Khronick"));
            Assert.IsTrue(fight.Fighters.Any(x => x.Character.Name == "Bealica"));
            Assert.IsTrue(fight.Fighters.Any(x => x.Character.Name == "a dwarf disciple"));

            Assert.AreEqual(4507, fight.Fighters.First(x => x.Character.Name == "Khadaji").OffensiveStatistics.Hit.Total);
            Assert.AreEqual(11481, fight.Fighters.First(x => x.Character.Name == "Bealica").OffensiveStatistics.Hit.Total);
            Assert.AreEqual(4106, fight.Fighters.First(x => x.Character.Name == "a dwarf disciple").OffensiveStatistics.Hit.Total);
            Assert.AreEqual(20094, fight.OffensiveStatistics.Hit.Total);

            Assert.AreEqual(4106, fight.Fighters.First(x => x.Character.Name == "Khadaji").DefensiveStatistics.Hit.Total);
            Assert.AreEqual(0, fight.Fighters.First(x => x.Character.Name == "Bealica").DefensiveStatistics.Hit.Total);
            Assert.AreEqual(15988, fight.Fighters.First(x => x.Character.Name == "a dwarf disciple").DefensiveStatistics.Hit.Total);
            Assert.AreEqual(20094, fight.DefensiveStatistics.Hit.Total);

            Assert.AreEqual(5371, fight.Fighters.First(x => x.Character.Name == "Khadaji").DefensiveStatistics.Heal.Total);
            Assert.AreEqual(2335, fight.Fighters.First(x => x.Character.Name == "Movanna").OffensiveStatistics.Heal.Total);
            Assert.AreEqual(3036, fight.Fighters.First(x => x.Character.Name == "Khronick").OffensiveStatistics.Heal.Total);
            Assert.AreEqual(5371, fight.DefensiveStatistics.Heal.Total);
            Assert.AreEqual(5371, fight.OffensiveStatistics.Heal.Total);

            Assert.AreEqual(1, fight.Fighters.First(x => x.Character.Name == "Bealica").OffensiveStatistics.Kill.Count);
            Assert.AreEqual(0, fight.Fighters.First(x => x.Character.Name == "Bealica").DefensiveStatistics.Kill.Count);
            Assert.AreEqual(1, fight.Fighters.First(x => x.Character.Name == "a dwarf disciple").DefensiveStatistics.Kill.Count);
            Assert.AreEqual(0, fight.Fighters.First(x => x.Character.Name == "a dwarf disciple").OffensiveStatistics.Kill.Count);
            Assert.AreEqual(1, fight.OffensiveStatistics.Kill.Count);
            Assert.AreEqual(1, fight.OffensiveStatistics.Kill.Count);

            Assert.AreEqual(new TimeSpan(0, 0, 38), fight.Fighters.First(x => x.Character.Name == "Khadaji").OffensiveStatistics.Duration.FighterDuration);
            Assert.AreEqual(new TimeSpan(0, 0, 19), fight.Fighters.First(x => x.Character.Name == "Bealica").OffensiveStatistics.Duration.FighterDuration);
            Assert.AreEqual(new TimeSpan(0, 1, 15), fight.OffensiveStatistics.Duration.FighterDuration);

            Assert.AreEqual(118.61, fight.Fighters.First(x => x.Character.Name == "Khadaji").OffensiveStatistics.PerTime.FighterDPS, 0.01);
            Assert.AreEqual(604.26, fight.Fighters.First(x => x.Character.Name == "Bealica").OffensiveStatistics.PerTime.FighterDPS, 0.01);
            Assert.AreEqual(267.92, fight.OffensiveStatistics.PerTime.FighterDPS, 0.01);

            Assert.AreEqual(new TimeSpan(0, 1, 15), fight.Fighters.First(x => x.Character.Name == "Khadaji").OffensiveStatistics.Duration.FightDuration);
            Assert.AreEqual(new TimeSpan(0, 1, 15), fight.Fighters.First(x => x.Character.Name == "Bealica").OffensiveStatistics.Duration.FightDuration);
            Assert.AreEqual(new TimeSpan(0, 1, 15), fight.OffensiveStatistics.Duration.FightDuration);

            Assert.AreEqual(60.09, fight.Fighters.First(x => x.Character.Name == "Khadaji").OffensiveStatistics.PerTime.FightDPS, 0.01);
            Assert.AreEqual(153.08, fight.Fighters.First(x => x.Character.Name == "Bealica").OffensiveStatistics.PerTime.FightDPS, 0.01);
        }

        [TestMethod]
        public void IdentifyMobFromYourAttack()
        {
            var pc = new Character(YouAre.Name);
            var fight = new Fight(YouAre);

            fight.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:16:49 2019] You kick a dwarf disciple for 3041 points of damage.")));

            Assert.AreEqual("a dwarf disciple", fight.PrimaryMob.Name);
        }

        [TestMethod]
        public void IdentifyMobAttackingYouButMisses()
        {
            var pc = new Character(YouAre.Name);
            var fight = new Fight(YouAre);

            fight.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:16:47 2019] A dwarf disciple tries to punch YOU, but YOU riposte!")));

            Assert.AreEqual("a dwarf disciple", fight.PrimaryMob.Name);
        }

        [TestMethod]
        public void IdentifyGenericMob()
        {
            var pc = new Character(YouAre.Name);
            var fight = new Fight(YouAre);

            fight.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:17:38 2019] Bealica hit a dwarf disciple for 11481 points of cold damage by Glacial Cascade.")));

            Assert.AreEqual("a dwarf disciple", fight.PrimaryMob.Name);
        }

        [TestMethod]
        public void IdentifyGenericMobHittingOther()
        {
            var pc = new Character(YouAre.Name);
            var fight = new Fight(YouAre);

            fight.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Thu Apr 04 22:29:09 2019] A telmira servant hits Bealica for 2331 points of damage.")));

            Assert.AreEqual("a telmira servant", fight.PrimaryMob.Name);
        }

        [TestMethod]
        public void IdentifyNamedMobUsingSpaces()
        {
            var pc = new Character(YouAre.Name);
            var fight = new Fight(YouAre);

            fight.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Sat Mar 30 10:19:36 2019] Ragbeard the Morose crushes Khronick for 1319 points of damage.")));

            Assert.AreEqual("Ragbeard the Morose", fight.PrimaryMob.Name);
        }

        [TestMethod]
        public void IdentifyNamedMobFromMultipleHits()
        {
            var pc = new Character(YouAre.Name);
            var fight = new Fight(YouAre);

            // We should be able to identify a mob within 2 attacks involving 3 different characters
            fight.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri May 16 20:21:00 2003] Sazzie punches Sontalak for 14 points of damage.")));
            Assert.AreEqual("Unknown", fight.PrimaryMob.Name);

            fight.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri May 16 20:21:13 2003] Sontalak claws Nair for 290 points of damage.")));
            Assert.AreEqual("Sontalak", fight.PrimaryMob.Name);
        }
    }
}
