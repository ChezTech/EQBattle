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

            // Skirmish stats
            Assert.AreEqual(42707, skirmish.OffensiveStatistics.Hit.Total);
            Assert.AreEqual(42707, skirmish.DefensiveStatistics.Hit.Total);
            Assert.AreEqual(56200, skirmish.DefensiveStatistics.Heal.Total);
            Assert.AreEqual(56200, skirmish.OffensiveStatistics.Heal.Total);
            Assert.AreEqual(3, skirmish.OffensiveStatistics.Kill.Count);
            Assert.AreEqual(3, skirmish.OffensiveStatistics.Kill.Count);

            // Skirmish Fighters
            Assert.AreEqual(6, skirmish.Fighters.Count());
            VerifyFighterStatistics("Khadaji", skirmish, 14540, 0, 0, 1, 0, 21571, 1, 0);
            VerifyFighterStatistics("Movanna", skirmish, 322, 54682, 0, 0, 11512, 23605, 1, 1);
            VerifyFighterStatistics("Khronick", skirmish, 8206, 1518, 0, 0, 8127, 11024, 0, 1);
            VerifyFighterStatistics("Gomphus", skirmish, 14073, 0, 1, 2, 1529, 0, 0, 0);
            VerifyFighterStatistics("Harvester Collyx", skirmish, 2566, 0, 1, 0, 21539, 0, 0, 1);
            VerifyFighterStatistics("Unknown", skirmish, 3000, 0, 0, 0, 0, 0, 0, 0);

            // Skirmish Fights
            Assert.AreEqual(2, skirmish.Fights.Count);
            VerifyFightStatistics("Gomphus", skirmish, 18602, 56200, 1, 2);
            VerifyFightStatistics("Harvester Collyx", skirmish, 24105, 0, 1, 1);
        }

        private void VerifyFighterStatistics(string name, Skirmish skirmish, int offHit, int offHeal, int offMisses, int offKills, int defHit, int defHeal, int defMisses, int defKills)
        {
            var fighter = skirmish.Fighters.FirstOrDefault(x => x.Character.Name == name);
            Assert.IsNotNull(fighter, $"Fighter doesn't exist - {name}");

            var stats = fighter.OffensiveStatistics;
            Assert.AreEqual(offHit, stats.Hit.Total, $"Offensive hit - {fighter.Character.Name}");
            Assert.AreEqual(offHeal, stats.Heal.Total, $"Offensive heal - {fighter.Character.Name}");
            Assert.AreEqual(offMisses, stats.Miss.Count, $"Offensive misses - {fighter.Character.Name}");
            Assert.AreEqual(offKills, stats.Kill.Count, $"Offensive kills - {fighter.Character.Name}");

            stats = fighter.DefensiveStatistics;
            Assert.AreEqual(defHit, stats.Hit.Total, $"Defensive hit - {fighter.Character.Name}");
            Assert.AreEqual(defHeal, stats.Heal.Total, $"Defensive heal - {fighter.Character.Name}");
            Assert.AreEqual(defMisses, stats.Miss.Count, $"Defensive misses - {fighter.Character.Name}");
            Assert.AreEqual(defKills, stats.Kill.Count, $"Defensive kills - {fighter.Character.Name}");
        }

        private void VerifyFightStatistics(string fightMob, Skirmish skirmish, int hit, int heal, int misses, int kills)
        {
            var fight = skirmish.Fights.FirstOrDefault(x => x.PrimaryMob.Name == fightMob);
            Assert.IsNotNull(fight, $"Fight doesn't exist - {fightMob}");

            var stats = fight.OffensiveStatistics;
            Assert.AreEqual(hit, stats.Hit.Total, $"Offensive hit - {fight.PrimaryMob.Name}");
            Assert.AreEqual(heal, stats.Heal.Total, $"Offensive heal - {fight.PrimaryMob.Name}");
            Assert.AreEqual(misses, stats.Miss.Count, $"Offensive misses - {fight.PrimaryMob.Name}");
            Assert.AreEqual(kills, stats.Kill.Count, $"Offensive kills - {fight.PrimaryMob.Name}");

            stats = fight.DefensiveStatistics;
            Assert.AreEqual(hit, stats.Hit.Total, $"Defensive hit - {fight.PrimaryMob.Name}");
            Assert.AreEqual(heal, stats.Heal.Total, $"Defensive heal - {fight.PrimaryMob.Name}");
            Assert.AreEqual(misses, stats.Miss.Count, $"Defensive misses - {fight.PrimaryMob.Name}");
            Assert.AreEqual(kills, stats.Kill.Count, $"Defensive kills - {fight.PrimaryMob.Name}");
        }
    }
}
