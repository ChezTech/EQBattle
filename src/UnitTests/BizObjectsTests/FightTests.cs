using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BizObjects.Battle;
using BizObjects.Converters;
using BizObjects.Lines;
using BizObjects.Statistics;
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
    public class FightTests : FightTestBase
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
            Assert.AreEqual(20094, fight.Statistics.Hit.Total);

            Assert.AreEqual(4106, fight.Fighters.First(x => x.Character.Name == "Khadaji").DefensiveStatistics.Hit.Total);
            Assert.AreEqual(0, fight.Fighters.First(x => x.Character.Name == "Bealica").DefensiveStatistics.Hit.Total);
            Assert.AreEqual(15988, fight.Fighters.First(x => x.Character.Name == "a dwarf disciple").DefensiveStatistics.Hit.Total);

            Assert.AreEqual(5371, fight.Fighters.First(x => x.Character.Name == "Khadaji").DefensiveStatistics.Heal.Total);
            Assert.AreEqual(2335, fight.Fighters.First(x => x.Character.Name == "Movanna").OffensiveStatistics.Heal.Total);
            Assert.AreEqual(3036, fight.Fighters.First(x => x.Character.Name == "Khronick").OffensiveStatistics.Heal.Total);
            Assert.AreEqual(5371, fight.Statistics.Heal.Total);

            Assert.AreEqual(1, fight.Fighters.First(x => x.Character.Name == "Bealica").OffensiveStatistics.Kill.Count);
            Assert.AreEqual(0, fight.Fighters.First(x => x.Character.Name == "Bealica").DefensiveStatistics.Kill.Count);
            Assert.AreEqual(1, fight.Fighters.First(x => x.Character.Name == "a dwarf disciple").DefensiveStatistics.Kill.Count);
            Assert.AreEqual(0, fight.Fighters.First(x => x.Character.Name == "a dwarf disciple").OffensiveStatistics.Kill.Count);
            Assert.AreEqual(1, fight.Statistics.Kill.Count);
            Assert.AreEqual(1, fight.Statistics.Kill.Count);

            Assert.AreEqual(new TimeSpan(0, 0, 38), fight.Fighters.First(x => x.Character.Name == "Khadaji").OffensiveStatistics.Duration.FighterDuration);
            Assert.AreEqual(new TimeSpan(0, 0, 19), fight.Fighters.First(x => x.Character.Name == "Bealica").OffensiveStatistics.Duration.FighterDuration);
            Assert.AreEqual(new TimeSpan(0, 1, 15), fight.Statistics.Duration.FighterDuration);

            Assert.AreEqual(118.61, fight.Fighters.First(x => x.Character.Name == "Khadaji").OffensiveStatistics.PerTime.FighterDPS, 0.01);
            Assert.AreEqual(604.26, fight.Fighters.First(x => x.Character.Name == "Bealica").OffensiveStatistics.PerTime.FighterDPS, 0.01);
            Assert.AreEqual(267.92, fight.Statistics.PerTime.FighterDPS, 0.01);

            Assert.AreEqual(new TimeSpan(0, 1, 15), fight.Fighters.First(x => x.Character.Name == "Khadaji").OffensiveStatistics.Duration.FightDuration);
            Assert.AreEqual(new TimeSpan(0, 1, 15), fight.Fighters.First(x => x.Character.Name == "Bealica").OffensiveStatistics.Duration.FightDuration);
            Assert.AreEqual(new TimeSpan(0, 1, 15), fight.Statistics.Duration.FightDuration);

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
        public void GenericMobWithoutIndefiniteArticleShouldNotBeCapitalizedMobFirst()
        {
            var fight = SetupNewFight(out CharacterTracker charTracker);

            AddFightTrackLine(fight, charTracker, "[Sun Sep 15 09:37:55 2019] Molten steel hits YOU for 6017 points of damage.");
            AddFightTrackLine(fight, charTracker, "[Sun Sep 15 09:37:55 2019] You strike molten steel for 2306 points of damage.");

            Assert.AreEqual(2, fight.Fighters.Count());
            Assert.IsTrue(fight.Fighters.Any(x => x.Character.Name == "Khadaji"));
            Assert.IsTrue(fight.Fighters.Any(x => x.Character.Name == "molten steel"));

            Assert.AreEqual("molten steel", fight.PrimaryMob.Name);
        }

        [TestMethod]
        public void GenericMobWithoutIndefiniteArticleShouldNotBeCapitalizedMobLast()
        {
            var fight = SetupNewFight(out CharacterTracker charTracker);

            AddFightTrackLine(fight, charTracker, "[Sun Sep 15 09:37:55 2019] You strike molten steel for 2306 points of damage.");
            AddFightTrackLine(fight, charTracker, "[Sun Sep 15 09:37:55 2019] Molten steel hits YOU for 6017 points of damage.");

            Assert.AreEqual(2, fight.Fighters.Count());
            Assert.IsTrue(fight.Fighters.Any(x => x.Character.Name == "Khadaji"));
            Assert.IsTrue(fight.Fighters.Any(x => x.Character.Name == "molten steel"));

            Assert.AreEqual("molten steel", fight.PrimaryMob.Name);
        }

        [TestMethod]
        public void GenericMobWithoutIndefiniteArticleShouldNotBeCapitalizedNotYou()
        {
            var fight = SetupNewFight(out CharacterTracker charTracker);

            AddFightTrackLine(fight, charTracker, "[Sun Sep 15 09:40:27 2019] Molten steel hits Kelanna for 4214 points of damage. (Riposte)");
            AddFightTrackLine(fight, charTracker, "[Sun Sep 15 09:40:27 2019] Kelanna pierces molten steel for 980 points of damage.");

            Assert.AreEqual(2, fight.Fighters.Count());
            Assert.IsTrue(fight.Fighters.Any(x => x.Character.Name == "Kelanna"));
            Assert.IsTrue(fight.Fighters.Any(x => x.Character.Name == "molten steel"));

            Assert.AreEqual("molten steel", fight.PrimaryMob.Name);
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

        [TestMethod]
        public void DpsTestPerFighter()
        {
            var fight = SetupNewFight(out CharacterTracker charTracker);

            AddFightTrackLine(fight, charTracker, "[Sat Mar 30 07:33:00 2019] A scary mob smashes YOU for 927 points of damage.");
            AddFightTrackLine(fight, charTracker, "[Sat Mar 30 07:33:01 2019] You kick a scary mob for 741 points of damage.");
            AddFightTrackLine(fight, charTracker, "[Sat Mar 30 07:33:03 2019] A scary mob smashes YOU for 4259 points of damage.");
            AddFightTrackLine(fight, charTracker, "[Sat Mar 30 07:33:04 2019] You strike a scary mob for 647 points of damage.");
            AddFightTrackLine(fight, charTracker, "[Sat Mar 30 07:33:04 2019] Khadaji hit a scary mob for 388 points of poison damage by Strike of Venom IV.");
            AddFightTrackLine(fight, charTracker, "[Sat Mar 30 07:33:06 2019] Bealica hit a scary mob for 6426 points of magic damage by Pure Wildmagic.");
            AddFightTrackLine(fight, charTracker, "[Sat Mar 30 07:33:07 2019] You crush a scary mob for 1520 points of damage.");
            AddFightTrackLine(fight, charTracker, "[Sat Mar 30 07:33:08 2019] A scary mob smashes YOU for 3257 points of damage.");
            AddFightTrackLine(fight, charTracker, "[Sat Mar 30 07:33:09 2019] Bealica hit a scary mob for 8085 points of fire damage by Inizen's Fire.");
            AddFightTrackLine(fight, charTracker, "[Sat Mar 30 07:33:10 2019] You crush a scary mob for 1546 points of damage.");

            VerifyDpsStats(fight.Statistics, 27796, new TimeSpan(0, 0, 10), 2779.6, 2779.6);
            VerifyDpsStats(fight, "a scary mob", 8443, new TimeSpan(0, 0, 8), 1055.38, 844.3, x => x.OffensiveStatistics);
            VerifyDpsStats(fight, "Khadaji", 4842, new TimeSpan(0, 0, 9), 538.0, 484.2, x => x.OffensiveStatistics);
            VerifyDpsStats(fight, "Bealica", 14511, new TimeSpan(0, 0, 3), 4837, 1451.1, x => x.OffensiveStatistics);
        }

        [TestMethod]
        [Ignore]
        public void CompleteHealLineCountsAsOne()
        {
            var fight = SetupNewFight(out CharacterTracker charTracker);

            AddFightTrackLine(fight, charTracker, "[Mon May 12 19:42:26 2003] You have been healed for 1951 points of damage.");
            AddFightTrackLine(fight, charTracker, "[Mon May 12 19:42:26 2003] You are completely healed.");

            VerifyFightStatistics("Unknown", fight, 0, 1951, 0, 0);

            Assert.AreEqual(2, fight.Fighters.Count());
            var fighter = VerifyFighterStatistics("Khadaji", fight, 0, 0, 0, 0, 0, 1951, 0, 0);

            // It's really one heal event
            Assert.AreEqual(1, fighter.DefensiveStatistics.Heal.Count);
        }

        [TestMethod]
        public void YouDiedDueToDot()
        {
            var fight = SetupNewFight(out CharacterTracker charTracker);

            //[Sun Sep 27 09:46:53 2020] Embalming Goo begins casting Fink's Diseased Touch.
            //[Sun Sep 27 09:46:53 2020] You are touched by poison.
            //[Sun Sep 27 09:46:55 2020] Embalming Goo is pierced by YOUR thorns for 7023 points of non-melee damage.
            //[Sun Sep 27 09:46:55 2020] Embalming Goo hits YOU for 45317 points of damage. (Strikethrough)
            //[Sun Sep 27 09:46:55 2020] Embalming Goo is pierced by YOUR thorns for 7023 points of non-melee damage.
            //[Sun Sep 27 09:46:55 2020] Embalming Goo hits YOU for 20482 points of damage.
            //[Sun Sep 27 09:46:55 2020] Embalming Goo tries to hit YOU, but misses!
            //[Sun Sep 27 09:46:55 2020] Embalming Goo is pierced by YOUR thorns for 7023 points of non-melee damage.
            //[Sun Sep 27 09:46:55 2020] Embalming Goo hits YOU for 47387 points of damage.
            //[Sun Sep 27 09:46:55 2020] Embalming Goo is pierced by YOUR thorns for 7023 points of non-melee damage.
            //[Sun Sep 27 09:46:55 2020] Embalming Goo bashes YOU for 5995 points of damage. (Strikethrough)
            //[Sun Sep 27 09:46:56 2020] You magically mend your wounds and heal considerable damage.
            //[Sun Sep 27 09:46:57 2020] Embalming Goo tries to hit YOU, but misses!
            //[Sun Sep 27 09:46:57 2020] Embalming Goo is pierced by YOUR thorns for 7023 points of non-melee damage.
            //[Sun Sep 27 09:46:57 2020] Embalming Goo hits YOU for 32900 points of damage. (Strikethrough)
            //[Sun Sep 27 09:46:57 2020] Embalming Goo is pierced by YOUR thorns for 7023 points of non-melee damage.
            //[Sun Sep 27 09:46:57 2020] Embalming Goo hits YOU for 45317 points of damage. (Strikethrough)
            //[Sun Sep 27 09:46:58 2020] You have taken 56889 damage from Fink's Diseased Touch by Embalming Goo.
            //[Sun Sep 27 09:47:04 2020] You have taken 56889 damage from Fink's Diseased Touch by Embalming Goo.
            //[Sun Sep 27 09:47:05 2020] You have been knocked unconscious!
            //[Sun Sep 27 09:47:05 2020] You may not use that command while dead.
            //[Sun Sep 27 09:47:10 2020] You have taken 56889 damage from Fink's Diseased Touch by Embalming Goo.
            //[Sun Sep 27 09:47:10 2020] You died.

            // Can we assign my death to last damage line, hence last mob?


        }

        [TestMethod]
        public void OtherDiedDueToDot()
        {
            var fight = SetupNewFight(out CharacterTracker charTracker);

            //[Sun Sep 20 11:05:14 2020] Tormented Adalora is tormented by Skullmasta's frost for 313 points of non-melee damage.
            //[Sun Sep 20 11:05:14 2020] Tormented Adalora punches Skullmasta for 23943 points of damage.
            //[Sun Sep 20 11:05:14 2020] Tormented Adalora is tormented by Skullmasta's frost for 313 points of non-melee damage.
            //[Sun Sep 20 11:05:14 2020] Tormented Adalora punches Skullmasta for 18149 points of damage.
            //[Sun Sep 20 11:05:14 2020] Tormented Adalora is tormented by Skullmasta's frost for 313 points of non-melee damage.
            //[Sun Sep 20 11:05:14 2020] Tormented Adalora punches Skullmasta for 45192 points of damage.
            //[Sun Sep 20 11:05:14 2020] Tormented Adalora is tormented by Skullmasta's frost for 313 points of non-melee damage.
            //[Sun Sep 20 11:05:14 2020] Tormented Adalora punches Skullmasta for 8490 points of damage.
            //[Sun Sep 20 11:05:18 2020] Skullmasta has taken 55364 damage from Dread Admiral's Curse by Tormented Adalora.
            //[Sun Sep 20 11:05:24 2020] Skullmasta has taken 55364 damage from Dread Admiral's Curse by Tormented Adalora.
            //[Sun Sep 20 11:05:24 2020] Skullmasta died.

        }

        [TestMethod]
        public void MobDiedDueToDot()
        {
            var fight = SetupNewFight(out CharacterTracker charTracker);

            //[Sat Sep 05 21:14:21 2020] Kelanna hit a filthy ape for 8138 points of poison damage by Bite of the Shissar Poison Strike.
            //[Sat Sep 05 21:14:21 2020] A filthy ape has taken 2252 damage from Jugular Gash Rk.II by Kelanna.
            //[Sat Sep 05 21:14:21 2020] A filthy ape died.

        }

        [TestMethod]
        public void YouDiedDueToDamageShield()
        {
            var fight = SetupNewFight(out CharacterTracker charTracker);

            //[Thu Oct 08 21:47:11 2020] You activate Vigorous Shuriken Rk.II.
            //[Thu Oct 08 21:47:11 2020] You hit an ember trooper for 1440 points of physical damage by Vigorous Shuriken Rk.II.
            //[Thu Oct 08 21:47:11 2020] An ember trooper is struck from a distance by a razor-sharp shuriken.
            //[Thu Oct 08 21:47:11 2020] An ember trooper begins casting Cooling Crust.
            //[Thu Oct 08 21:47:14 2020] An ember trooper tries to pierce YOU, but YOU riposte!
            //[Thu Oct 08 21:47:14 2020] YOU are pierced by an ember trooper's thorns for 11971 points of non-melee damage!
            //[Thu Oct 08 21:47:14 2020] You try to crush an ember trooper, but an ember trooper's magical skin absorbs the blow! (Riposte Strikethrough)
            //[Thu Oct 08 21:47:14 2020] An ember trooper's magical skin absorbs the damage of YOUR thorns.
            //[Thu Oct 08 21:47:14 2020] An ember trooper pierces YOU for 6334 points of damage. (Riposte Strikethrough)
            //[Thu Oct 08 21:47:14 2020] An ember trooper tries to pierce YOU, but YOU riposte!
            //[Thu Oct 08 21:47:14 2020] YOU are pierced by an ember trooper's thorns for 11971 points of non-melee damage!
            //[Thu Oct 08 21:47:14 2020] You try to crush an ember trooper, but an ember trooper's magical skin absorbs the blow! (Riposte Strikethrough)
            //[Thu Oct 08 21:47:14 2020] YOU are pierced by an ember trooper's thorns for 11971 points of non-melee damage!
            //[Thu Oct 08 21:47:14 2020] You try to crush an ember trooper, but an ember trooper's magical skin absorbs the blow! (Riposte Strikethrough)
            //[Thu Oct 08 21:47:14 2020] An ember trooper tries to hit YOU, but misses!
            //[Thu Oct 08 21:47:14 2020] An ember trooper tries to hit YOU, but misses!
            //[Thu Oct 08 21:47:15 2020] Auto attack is on.
            //[Thu Oct 08 21:47:15 2020] An ember trooper tries to bash YOU, but misses!
            //[Thu Oct 08 21:47:15 2020] YOU are pierced by an ember trooper's thorns for 11971 points of non-melee damage!
            //[Thu Oct 08 21:47:15 2020] You try to kick an ember trooper, but an ember trooper's magical skin absorbs the blow!
            //[Thu Oct 08 21:47:15 2020] YOU are pierced by an ember trooper's thorns for 11971 points of non-melee damage!
            //[Thu Oct 08 21:47:15 2020] You try to kick an ember trooper, but an ember trooper's magical skin absorbs the blow!
            //[Thu Oct 08 21:47:15 2020] The spirit of Master Wu fills you!  You gain 3 additional attack(s).
            //[Thu Oct 08 21:47:15 2020] YOU are pierced by an ember trooper's thorns for 11971 points of non-melee damage!
            //[Thu Oct 08 21:47:15 2020] You try to kick an ember trooper, but an ember trooper's magical skin absorbs the blow!
            //[Thu Oct 08 21:47:15 2020] YOU are pierced by an ember trooper's thorns for 11971 points of non-melee damage!
            //[Thu Oct 08 21:47:15 2020] You try to kick an ember trooper, but an ember trooper's magical skin absorbs the blow!
            //[Thu Oct 08 21:47:15 2020] YOU are pierced by an ember trooper's thorns for 11971 points of non-melee damage!
            //[Thu Oct 08 21:47:15 2020] You try to kick an ember trooper, but an ember trooper's magical skin absorbs the blow!
            //[Thu Oct 08 21:47:15 2020] YOU are pierced by an ember trooper's thorns for 11971 points of non-melee damage!
            //[Thu Oct 08 21:47:15 2020] You try to strike an ember trooper, but an ember trooper's magical skin absorbs the blow!
            //[Thu Oct 08 21:47:15 2020] YOU are pierced by an ember trooper's thorns for 11971 points of non-melee damage!
            //[Thu Oct 08 21:47:15 2020] You try to strike an ember trooper, but an ember trooper's magical skin absorbs the blow! (Strikethrough)
            //[Thu Oct 08 21:47:15 2020] The spirit of Master Wu fills you!  You gain 2 additional attack(s).
            //[Thu Oct 08 21:47:15 2020] YOU are pierced by an ember trooper's thorns for 11971 points of non-melee damage!
            //[Thu Oct 08 21:47:15 2020] You try to strike an ember trooper, but an ember trooper's magical skin absorbs the blow! (Strikethrough)
            //[Thu Oct 08 21:47:15 2020] YOU are pierced by an ember trooper's thorns for 11971 points of non-melee damage!
            //[Thu Oct 08 21:47:15 2020] You try to strike an ember trooper, but an ember trooper's magical skin absorbs the blow!
            //[Thu Oct 08 21:47:15 2020] YOU are pierced by an ember trooper's thorns for 11971 points of non-melee damage!
            //[Thu Oct 08 21:47:15 2020] You try to crush an ember trooper, but an ember trooper's magical skin absorbs the blow! (Strikethrough)
            //[Thu Oct 08 21:47:15 2020] YOU are pierced by an ember trooper's thorns for 11971 points of non-melee damage!
            //[Thu Oct 08 21:47:15 2020] You try to crush an ember trooper, but an ember trooper's magical skin absorbs the blow!
            //[Thu Oct 08 21:47:15 2020] You can use the ability Hoshkar's Fang Rk. II again in 0 minute(s) 3 seconds.
            //[Thu Oct 08 21:47:15 2020] You can use the ability Vigorous Shuriken Rk. II again in 0 minute(s) 2 seconds.
            //[Thu Oct 08 21:47:15 2020] You can use the ability Zan Fi's Whistle again in 3 minute(s) 51 seconds.
            //[Thu Oct 08 21:47:15 2020] You begin casting Two-Finger Wasp Touch VII.
            //[Thu Oct 08 21:47:15 2020] an ember trooper is crippled by a two-finger wasp touch.
            //[Thu Oct 08 21:47:15 2020] You activate Firestorm of Fists Rk.II.
            //[Thu Oct 08 21:47:15 2020] YOU are pierced by an ember trooper's thorns for 11971 points of non-melee damage!
            //[Thu Oct 08 21:47:15 2020] You try to strike an ember trooper, but an ember trooper's magical skin absorbs the blow!
            //[Thu Oct 08 21:47:15 2020] YOU are pierced by an ember trooper's thorns for 11971 points of non-melee damage!
            //[Thu Oct 08 21:47:15 2020] An ember trooper says, 'An Ember must be strong and perceptive to survive. I am clearly both.'
            //[Thu Oct 08 21:47:15 2020] You have been knocked unconscious!
            //[Thu Oct 08 21:47:15 2020] You have been slain by an ember trooper!
            //[Thu Oct 08 21:47:15 2020] You try to strike an ember trooper, but an ember trooper's magical skin absorbs the blow!
            //[Thu Oct 08 21:47:15 2020] An ember trooper flinches, 'What was that? Ah, I see you.'
            //[Thu Oct 08 21:47:15 2020] You try to strike an ember trooper, but an ember trooper's magical skin absorbs the blow!
            //[Thu Oct 08 21:47:15 2020] an ember trooper is struck by fists from every direction.

        }

    private Fight SetupNewFight(out CharacterTracker charTracker)
        {
            var charResolver = new CharacterResolver();
            charTracker = new CharacterTracker(YouAre, charResolver);
            return new Fight(YouAre, charResolver);
        }
    }
}
