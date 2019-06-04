﻿using System;
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
    public class BattleTests
    {
        private static readonly YouResolver YouAre = new YouResolver("Khadaji");
        private static readonly CharacterResolver CharResolver = new CharacterResolver();
        private static LineParserFactory _parser = new LineParserFactory();
        private readonly IParser _hitParser = new HitParser(YouAre);
        private readonly IParser _missParser = new MissParser(YouAre);
        private readonly IParser _healParser = new HealParser(YouAre);
        private readonly IParser _killParser = new KillParser(YouAre);

        private readonly Action<Battle, string> AddBattleLine = (battle, logLine) => battle.AddLine((dynamic)_parser.ParseLine(new LogDatum(logLine)));

        public BattleTests()
        {
            _parser.AddParser(_hitParser, null);
            _parser.AddParser(_missParser, null);
            _parser.AddParser(_healParser, null);
            _parser.AddParser(_killParser, null);

            CharResolver.AddNonPlayer("Harvester Collyx");
        }

        [TestMethod]
        public void FightThreeOfTheSameMobsPrettyMuchSameTimeWrapInOneSkirmish()
        {
            var battle = SetupNewBattle();

            AddBattleLine(battle, "[Fri May 24 18:25:36 2019] Jebartik hit a cliknar scout drone for 3000 points of magic damage by Companion's Strike XIII.");
            AddBattleLine(battle, "[Fri May 24 18:25:36 2019] Eruzen hit a cliknar scout drone for 70859 points of fire damage by Spear of Magma Rk. II. (Critical Twincast)");
            AddBattleLine(battle, "[Fri May 24 18:25:36 2019] A cliknar scout drone has taken 2546 damage from Breath of the Shiverback Rk. II by Zenarya.");
            AddBattleLine(battle, "[Fri May 24 18:25:36 2019] A cliknar scout drone has been slain by Zenarya!");

            // 1 second gap
            AddBattleLine(battle, "[Fri May 24 18:25:37 2019] Vatalae hit a cliknar scout drone for 985 points of chromatic damage by Tiger Maw II.");
            AddBattleLine(battle, "[Fri May 24 18:25:39 2019] Eruzen hit a cliknar scout drone for 35386 points of magic damage by Shock of Argathian Steel. (Critical)");
            AddBattleLine(battle, "[Fri May 24 18:25:40 2019] Eruzen hit a cliknar scout drone for 72287 points of fire damage by Spear of Magma Rk. II. (Critical Twincast)");
            AddBattleLine(battle, "[Fri May 24 18:25:40 2019] A cliknar scout drone has been slain by Eruzen!");

            // 2 second gap
            AddBattleLine(battle, "[Fri May 24 18:25:42 2019] Vatalae hit a cliknar scout drone for 985 points of chromatic damage by Tiger Maw II.");
            AddBattleLine(battle, "[Fri May 24 18:25:42 2019] A cliknar scout drone has taken 10151 damage from Breath of the Shiverback Rk. II by Zenarya. (Critical)");
            AddBattleLine(battle, "[Fri May 24 18:25:42 2019] A cliknar scout drone has been slain by Zenarya!");

            Assert.AreEqual(1, battle.Skirmishes.Count);
        }

        [TestMethod]
        public void FightThreeOfTheSameMobsWithBreaksGiveThreeSkirmishes()
        {
            var battle = SetupNewBattle();

            AddBattleLine(battle, "[Fri May 24 18:25:00 2019] Jebartik hit a cliknar scout drone for 3000 points of magic damage by Companion's Strike XIII.");
            AddBattleLine(battle, "[Fri May 24 18:25:00 2019] Eruzen hit a cliknar scout drone for 70859 points of fire damage by Spear of Magma Rk. II. (Critical Twincast)");
            AddBattleLine(battle, "[Fri May 24 18:25:00 2019] A cliknar scout drone has taken 2546 damage from Breath of the Shiverback Rk. II by Zenarya.");
            AddBattleLine(battle, "[Fri May 24 18:25:00 2019] A cliknar scout drone has been slain by Zenarya!");

            // 10 second gap
            AddBattleLine(battle, "[Fri May 24 18:25:10 2019] Vatalae hit a cliknar scout drone for 985 points of chromatic damage by Tiger Maw II.");
            AddBattleLine(battle, "[Fri May 24 18:25:10 2019] Eruzen hit a cliknar scout drone for 35386 points of magic damage by Shock of Argathian Steel. (Critical)");
            AddBattleLine(battle, "[Fri May 24 18:25:11 2019] Eruzen hit a cliknar scout drone for 72287 points of fire damage by Spear of Magma Rk. II. (Critical Twincast)");
            AddBattleLine(battle, "[Fri May 24 18:25:11 2019] A cliknar scout drone has been slain by Eruzen!");

            // 6 second gap
            AddBattleLine(battle, "[Fri May 24 18:25:17 2019] Vatalae hit a cliknar scout drone for 985 points of chromatic damage by Tiger Maw II.");
            AddBattleLine(battle, "[Fri May 24 18:25:17 2019] A cliknar scout drone has taken 10151 damage from Breath of the Shiverback Rk. II by Zenarya. (Critical)");
            AddBattleLine(battle, "[Fri May 24 18:25:18 2019] A cliknar scout drone has been slain by Zenarya!");

            Assert.AreEqual(3, battle.Skirmishes.Count);
        }

        private void VerifySkirmishStats(Skirmish skirmish, int hit, int heal, int misses, int kills)
        {
            var stats = skirmish.OffensiveStatistics;
            Assert.AreEqual(hit, stats.Hit.Total, $"Offensive hit");
            Assert.AreEqual(heal, stats.Heal.Total, $"Offensive heal");
            Assert.AreEqual(misses, stats.Miss.Count, $"Offensive misses");
            Assert.AreEqual(kills, stats.Kill.Count, $"Offensive kills");

            stats = skirmish.DefensiveStatistics;
            Assert.AreEqual(hit, stats.Hit.Total, $"Defensive hit");
            Assert.AreEqual(heal, stats.Heal.Total, $"Defensive heal");
            Assert.AreEqual(misses, stats.Miss.Count, $"Defensive misses");
            Assert.AreEqual(kills, stats.Kill.Count, $"Defensive kills");
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
            VerifyFightStatistics(fight, fightMob, skirmish, hit, heal, misses, kills);
        }

        private void VerifyFightStatistics(IFight fight, string fightMob, Skirmish skirmish, int hit, int heal, int misses, int kills)
        {
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

        private Battle SetupNewBattle()
        {
            return new Battle(YouAre);
        }
    }
}