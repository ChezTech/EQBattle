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
    public class FightTests : ParserTestBase
    {
        private readonly Action<Fight, CharacterTracker, string> AddFightTrackLine = (fight, tracker, logLine) =>
        {
            ILine line = _parser.ParseLine(new LogDatum(logLine));
            tracker.TrackLine((dynamic)line);
            fight.AddLine((dynamic)line);
        };

        [TestMethod]
        public void SmallFight()
        {
            var fight = SetupNewFight(out CharacterTracker charTracker);

            AddFightTrackLine(fight, charTracker, "[Fri Apr 05 16:16:42 2019] Khadaji hit a dwarf disciple for 2 points of magic damage by Distant Strike I.");
            AddFightTrackLine(fight, charTracker, "[Fri Apr 05 16:16:45 2019] A dwarf disciple is pierced by YOUR thorns for 60 points of non-melee damage.");
            AddFightTrackLine(fight, charTracker, "[Fri Apr 05 16:16:45 2019] A dwarf disciple punches YOU for 3241 points of damage.");
            AddFightTrackLine(fight, charTracker, "[Fri Apr 05 16:16:47 2019] A dwarf disciple tries to punch YOU, but YOU riposte!");
            AddFightTrackLine(fight, charTracker, "[Fri Apr 05 16:16:49 2019] You kick a dwarf disciple for 3041 points of damage. (Strikethrough)");
            AddFightTrackLine(fight, charTracker, "[Fri Apr 05 16:16:50 2019] You try to crush a dwarf disciple, but miss!");
            AddFightTrackLine(fight, charTracker, "[Fri Apr 05 16:16:50 2019] Movanna healed you over time for 2335 hit points by Elixir of the Ardent.");
            AddFightTrackLine(fight, charTracker, "[Fri Apr 05 16:16:51 2019] Khadaji hit a dwarf disciple for 892 points of poison damage by Strike of Venom IV. (Critical)");
            AddFightTrackLine(fight, charTracker, "[Fri Apr 05 16:16:52 2019] A dwarf disciple punches YOU for 865 points of damage.");
            AddFightTrackLine(fight, charTracker, "[Fri Apr 05 16:17:20 2019] Khadaji hit a dwarf disciple for 512 points of chromatic damage by Lynx Maw.");
            AddFightTrackLine(fight, charTracker, "[Fri Apr 05 16:17:21 2019] Khronick healed you over time for 3036 hit points by Healing Counterbias Effect. (Critical)");
            AddFightTrackLine(fight, charTracker, "[Fri Apr 05 16:17:38 2019] Bealica hit a dwarf disciple for 11481 points of cold damage by Glacial Cascade.");
            AddFightTrackLine(fight, charTracker, "[Fri Apr 05 16:17:57 2019] A dwarf disciple has been slain by Bealica!");

            Assert.AreEqual(5, fight.Fighters.Count());
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
            var fight = SetupNewFight(out CharacterTracker charTracker);

            AddFightTrackLine(fight, charTracker, "[Fri Apr 05 16:16:49 2019] You kick a dwarf disciple for 3041 points of damage.");

            Assert.AreEqual("a dwarf disciple", fight.PrimaryMob.Name);
        }

        [TestMethod]
        public void IdentifyMobAttackingYouButMisses()
        {
            var fight = SetupNewFight(out CharacterTracker charTracker);

            AddFightTrackLine(fight, charTracker, "[Fri Apr 05 16:16:47 2019] A dwarf disciple tries to punch YOU, but YOU riposte!");

            Assert.AreEqual("a dwarf disciple", fight.PrimaryMob.Name);
        }

        [TestMethod]
        public void IdentifyGenericMob()
        {
            var fight = SetupNewFight(out CharacterTracker charTracker);

            AddFightTrackLine(fight, charTracker, "[Fri Apr 05 16:17:38 2019] Bealica hit a dwarf disciple for 11481 points of cold damage by Glacial Cascade.");

            Assert.AreEqual("a dwarf disciple", fight.PrimaryMob.Name);
        }

        [TestMethod]
        public void IdentifyGenericMobHittingOther()
        {
            var fight = SetupNewFight(out CharacterTracker charTracker);

            AddFightTrackLine(fight, charTracker, "[Thu Apr 04 22:29:09 2019] A telmira servant hits Bealica for 2331 points of damage.");

            Assert.AreEqual("a telmira servant", fight.PrimaryMob.Name);
        }

        [TestMethod]
        public void IdentifyNamedMobUsingSpaces()
        {
            var fight = SetupNewFight(out CharacterTracker charTracker);

            AddFightTrackLine(fight, charTracker, "[Sat Mar 30 10:19:36 2019] Ragbeard the Morose crushes Khronick for 1319 points of damage.");

            Assert.AreEqual("Ragbeard the Morose", fight.PrimaryMob.Name);
        }

        [TestMethod]
        public void IdentifyNamedMobFromMultipleHits()
        {
            var fight = SetupNewFight(out CharacterTracker charTracker);

            // We should be able to identify a mob within 2 attacks involving 3 different characters
            AddFightTrackLine(fight, charTracker, "[Fri May 16 20:21:00 2003] Sazzie punches Sontalak for 14 points of damage.");
            Assert.AreEqual("Unknown", fight.PrimaryMob.Name);

            AddFightTrackLine(fight, charTracker, "[Fri May 16 20:21:13 2003] Sontalak claws Nair for 290 points of damage.");
            Assert.AreEqual("Sontalak", fight.PrimaryMob.Name);
        }

        [TestMethod]
        [Ignore]
        public void LeadingHealConfusesPrimaryMob()
        {
            var fight = SetupNewFight(out CharacterTracker charTracker);

            AddFightTrackLine(fight, charTracker, "[Tue May 28 06:01:17 2019] Khronick is bathed in a zealous light. Movanna healed Khronick for 9197 (23333) hit points by Zealous Light.");
            Assert.AreEqual("Unknown", fight.PrimaryMob.Name);

            AddFightTrackLine(fight, charTracker, "[Tue May 28 06:01:20 2019] Gomphus tries to hit Movanna, but Movanna blocks!");
            Assert.AreEqual("Gomphus", fight.PrimaryMob.Name);
        }

        [TestMethod]
        public void DontConfusePetsWithGenericMobs()
        {
            var fight = SetupNewFight(out CharacterTracker charTracker);

            AddFightTrackLine(fight, charTracker, "[Fri Apr 12 18:23:08 2019] Khadaji`s pet tries to hit a lavakin, but a lavakin dodges!");

            Assert.AreEqual("a lavakin", fight.PrimaryMob.Name);
        }

        [TestMethod]
        public void DontConfuseWarderPetsWithGenericMobs()
        {
            var fight = SetupNewFight(out CharacterTracker charTracker);

            AddFightTrackLine(fight, charTracker, "[Sat Mar 30 10:19:36 2019] Girnon`s warder bites a telmira servant for 17 points of damage.");

            Assert.AreEqual("a telmira servant", fight.PrimaryMob.Name);
        }

        [TestMethod]
        public void DontConfuseWarderPetsWithNamedMobsWithASpace()
        {
            var fight = SetupNewFight(out CharacterTracker charTracker);

            AddFightTrackLine(fight, charTracker, "[Sat Mar 30 10:19:36 2019] Girnon`s warder bites Sontalak for 17 points of damage.");
            Assert.AreEqual("Unknown", fight.PrimaryMob.Name); // We can't tell yet...

            AddFightTrackLine(fight, charTracker, "[Fri May 16 20:21:00 2003] Sazzie punches Sontalak for 14 points of damage.");
            Assert.AreEqual("Sontalak", fight.PrimaryMob.Name); // Now, we should be able to tell
        }

        [TestMethod]
        public void DontSwitchPrimaryMobOnAdd()
        {
            var fight = SetupNewFight(out CharacterTracker charTracker);

            AddFightTrackLine(fight, charTracker, "[Fri Apr 05 16:17:38 2019] Bealica hit a dwarf disciple for 11481 points of cold damage by Glacial Cascade.");
            Assert.AreEqual("a dwarf disciple", fight.PrimaryMob.Name);

            AddFightTrackLine(fight, charTracker, "[Thu Apr 04 22:29:09 2019] A telmira servant hits Bealica for 2331 points of damage.");
            Assert.AreEqual("a dwarf disciple", fight.PrimaryMob.Name);
        }

        [TestMethod]
        public void IsThisFightOverEasy()
        {
            var fight = SetupNewFight(out CharacterTracker charTracker);

            AddFightTrackLine(fight, charTracker, "[Fri Apr 05 16:17:38 2019] Bealica hit a dwarf disciple for 11481 points of cold damage by Glacial Cascade.");
            Assert.AreEqual("a dwarf disciple", fight.PrimaryMob.Name);

            AddFightTrackLine(fight, charTracker, "[Fri Apr 05 16:17:57 2019] A dwarf disciple has been slain by Bealica!");
            Assert.IsTrue(fight.IsFightOver);
        }

        [TestMethod]
        public void IsThisFightOverPetDied()
        {
            var fight = SetupNewFight(out CharacterTracker charTracker);

            AddFightTrackLine(fight, charTracker, "[Fri Apr 12 18:23:11 2019] A lavakin hits Khadaji`s pet for 824 points of damage. (Riposte)");
            Assert.AreEqual("a lavakin", fight.PrimaryMob.Name);

            AddFightTrackLine(fight, charTracker, "[Fri Apr 12 18:23:11 2019] Khadaji`s pet has been slain by a lavakin!");
            Assert.IsFalse(fight.IsFightOver);
        }

        [TestMethod]
        public void IsThisFightOverYouDied()
        {
            var fight = SetupNewFight(out CharacterTracker charTracker);

            AddFightTrackLine(fight, charTracker, "[Fri Apr 05 17:13:24 2019] An enraged disciple smashes YOU for 5206 points of damage.");
            Assert.AreEqual("an enraged disciple", fight.PrimaryMob.Name);

            AddFightTrackLine(fight, charTracker, "[Fri Apr 05 17:13:24 2019] You have been slain by an enraged disciple!");
            Assert.IsFalse(fight.IsFightOver);
        }

        [TestMethod]
        public void CheckSimilarFightNoLines()
        {
            var fight = SetupNewFight(out CharacterTracker charTracker);

            ILine line = _parser.ParseLine(new LogDatum("[Mon May 27 06:57:15 2019] You have taken 2080 damage from Paralyzing Bite."));

            Assert.IsFalse(fight.SimilarDamage(line as Hit));
        }

        [TestMethod]
        public void CheckSimilarFightNoMatch()
        {
            var fight = SetupNewFight(out CharacterTracker charTracker);

            AddFightTrackLine(fight, charTracker, "[Mon May 27 06:57:02 2019] You kick a sandspinner stalker for 967 points of damage. (Riposte)");
            ILine line = _parser.ParseLine(new LogDatum("[Mon May 27 06:57:03 2019] You have taken 2080 damage from Paralyzing Bite."));

            Assert.IsFalse(fight.SimilarDamage(line as Hit));
        }

        [TestMethod]
        public void CheckSimilarFight()
        {
            var fight = SetupNewFight(out CharacterTracker charTracker);

            AddFightTrackLine(fight, charTracker, "[Mon May 27 06:57:09 2019] You have taken 2080 damage from Paralyzing Bite by a sandspinner stalker.");
            ILine line = _parser.ParseLine(new LogDatum("[Mon May 27 06:57:15 2019] You have taken 2080 damage from Paralyzing Bite."));

            Assert.IsTrue(fight.SimilarDamage(line as Hit));
        }

        [TestMethod]
        public void CheckNotSimilarFight()
        {
            var fight = SetupNewFight(out CharacterTracker charTracker);

            AddFightTrackLine(fight, charTracker, "[Mon May 27 06:57:09 2019] You have taken 2080 damage from Paralyzing Bite by a sandspinner stalker.");
            ILine line = _parser.ParseLine(new LogDatum("[Mon May 27 06:57:15 2019] You have taken 2081 damage from Paralyzing Bite."));

            Assert.IsFalse(fight.SimilarDamage(line as Hit));
        }

        [TestMethod]
        public void CheckSimilarFightLoose()
        {
            var fight = SetupNewFight(out CharacterTracker charTracker);

            AddFightTrackLine(fight, charTracker, "[Mon May 27 09:56:45 2019] You have taken 1950 damage from Noxious Visions by Gomphus.");
            ILine line = _parser.ParseLine(new LogDatum("[Mon May 27 09:56:47 2019] Movanna has taken 3000 damage by Noxious Visions."));

            Assert.IsFalse(fight.SimilarDamage(line as Hit));
            Assert.IsTrue(fight.SimilarDamage(line as Hit, true));
        }

        [TestMethod]
        public void CheckNotSimilarFightLoose()
        {
            var fight = SetupNewFight(out CharacterTracker charTracker);

            AddFightTrackLine(fight, charTracker, "[Mon May 27 09:56:45 2019] You have taken 1950 damage from Noxious Visions by Gomphus.");
            ILine line = _parser.ParseLine(new LogDatum("[Mon May 27 09:56:47 2019] Movanna has taken 3000 damage by Putrid Visions."));

            Assert.IsFalse(fight.SimilarDamage(line as Hit));
            Assert.IsFalse(fight.SimilarDamage(line as Hit, true));
        }

        [TestMethod]
        public void TestFivePointPalmSameDamage()
        {
            var fight = SetupNewFight(out CharacterTracker charTracker);

            AddFightTrackLine(fight, charTracker, "[Mon May 27 07:20:03 2019] You kick a cliknar skirmish drone for 1314 points of damage. (Strikethrough)");
            AddFightTrackLine(fight, charTracker, "[Mon May 27 07:20:04 2019] You begin casting Five Point Palm VI.");
            AddFightTrackLine(fight, charTracker, "[Mon May 27 07:20:04 2019] You strike a cliknar skirmish drone for 3020 points of damage.");
            AddFightTrackLine(fight, charTracker, "[Mon May 27 07:20:04 2019] You hit a cliknar skirmish drone for 22528 points of physical damage by Five Point Palm VI.");
            AddFightTrackLine(fight, charTracker, "[Mon May 27 07:20:04 2019] You hit yourself for 1112 points of unresistable damage by Five Point Palm Focusing.");
            AddFightTrackLine(fight, charTracker, "[Mon May 27 07:20:04 2019]   You have taken 1112 points of damage.");
            AddFightTrackLine(fight, charTracker, "[Mon May 27 07:20:04 2019] You hit a cliknar skirmish drone for 512 points of chromatic damage by Lynx Maw.");

            VerifyFightStatistics("a cliknar skirmish drone", fight, 28486, 0, 0, 0);

            Assert.AreEqual(2, fight.Fighters.Count());
            VerifyFighterStatistics("Khadaji", fight, 28486, 0, 0, 0, 1112, 0, 0, 0);
            VerifyFighterStatistics("a cliknar skirmish drone", fight, 0, 0, 0, 0, 27374, 0, 0, 0);
        }

        [TestMethod]
        [Ignore] // We can't handle this use case for now
        public void TestFivePointPalmDifferentDamage()
        {
            var fight = SetupNewFight(out CharacterTracker charTracker);

            AddFightTrackLine(fight, charTracker, "[Sat Mar 30 08:15:01 2019] You kick a master hunter for 1458 points of damage.");
            AddFightTrackLine(fight, charTracker, "[Sat Mar 30 08:15:02 2019] You begin casting Five Point Palm VI.");
            AddFightTrackLine(fight, charTracker, "[Sat Mar 30 08:15:02 2019] Khadaji hit a master hunter for 51814 points of physical damage by Five Point Palm VI. (Critical)");
            AddFightTrackLine(fight, charTracker, "[Sat Mar 30 08:15:02 2019] You strike a master hunter for 4823 points of damage.");
            AddFightTrackLine(fight, charTracker, "[Sat Mar 30 08:15:02 2019] Khadaji hit Khadaji for 1354 points of unresistable damage by Five Point Palm Focusing.");

            // Usually this is the same value as the previous line. I suspect that this line is the real damage you take, while the above line is the potential damage you dealt on yourself.
            AddFightTrackLine(fight, charTracker, "[Sat Mar 30 08:15:02 2019]   You have taken 948 points of damage.");
            AddFightTrackLine(fight, charTracker, "[Sat Mar 30 08:15:02 2019] You crush a master hunter for 3614 points of damage. (Critical)");

            VerifyFightStatistics("a master hunter", fight, 62657, 0, 0, 0);

            Assert.AreEqual(2, fight.Fighters.Count());
            VerifyFighterStatistics("Khadaji", fight, 63063, 0, 0, 0, 948, 0, 0, 0);
            VerifyFighterStatistics("a master hunter", fight, 0, 0, 0, 0, 61709, 0, 0, 0);
        }

        [TestMethod]
        [Ignore] // This one will need to be solved
        public void TestCannibalization()
        {
            var fight = SetupNewFight(out CharacterTracker charTracker);

            AddFightTrackLine(fight, charTracker, "[Mon May 27 06:59:23 2019] You hit yourself for 8000 points of unresistable damage by Cannibalization V.");
            AddFightTrackLine(fight, charTracker, "[Mon May 27 06:59:23 2019] Your body aches as your mind clears.  You have taken 8000 points of damage.");

            VerifyFightStatistics("Unknown", fight, 8000, 0, 0, 0);

            Assert.AreEqual(1, fight.Fighters.Count());
            VerifyFighterStatistics("Khadaji", fight, 8000, 0, 0, 0, 8000, 0, 0, 0); // Really, this is Khronick, but I have "you" setup as "Khadaji" for this test
        }

        private Fight SetupNewFight(out CharacterTracker charTracker)
        {
            var charResolver = new CharacterResolver();
            charTracker = new CharacterTracker(YouAre, charResolver);
            return new Fight(YouAre, charResolver);
        }

        private void VerifyFighterStatistics(string name, Fight fight, int offHit, int offHeal, int offMisses, int offKills, int defHit, int defHeal, int defMisses, int defKills)
        {
            var fighter = fight.Fighters.FirstOrDefault(x => x.Character.Name == name);
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

        private void VerifyFightStatistics(string fightMob, Fight fight, int hit, int heal, int misses, int kills)
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
    }
}
