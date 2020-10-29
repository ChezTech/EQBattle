using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BizObjects.Battle;
using BizObjects.Converters;
using BizObjects.Lines;
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
    public class SkirmishTests : FightTestBase
    {
        private readonly Action<Skirmish, CharacterTracker, string> AddSkirmishTrackLine = (skirmish, tracker, logLine) =>
        {
            ILine line = _parser.ParseLine(new LogDatum(logLine));
            tracker.TrackLine((dynamic)line);
            skirmish.AddLine((dynamic)line);
        };

        [TestMethod]
        public void SmallFight()
        {
            var skirmish = SetupNewSkirmish(out CharacterTracker charTracker);

            AddSkirmishTrackLine(skirmish, charTracker, "[Tue May 28 06:01:03 2019] Gomphus tries to hit YOU, but YOU block!");
            AddSkirmishTrackLine(skirmish, charTracker, "[Tue May 28 06:01:17 2019] Khronick is bathed in a zealous light. Movanna healed Khronick for 9197 (23333) hit points by Zealous Light.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Tue May 28 06:01:20 2019] Harvester Collyx tries to hit Movanna, but Movanna blocks!");
            AddSkirmishTrackLine(skirmish, charTracker, "[Tue May 28 06:01:23 2019] Gomphus hits Movanna for 3870 points of damage.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Tue May 28 06:01:23 2019] Harvester Collyx hits Movanna for 2566 points of damage.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Tue May 28 06:01:43 2019] You kick Gomphus for 1207 points of damage.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Tue May 28 06:02:16 2019] Khronick feels the touch of recuperation. Movanna healed Khronick for 1827 (23605) hit points by Word of Recuperation.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Tue May 28 06:02:16 2019] Movanna feels the touch of recuperation. Movanna healed herself for 23605 hit points by Word of Recuperation.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Tue May 28 06:02:16 2019] You feel the touch of recuperation. Movanna healed you for 20053 (23605) hit points by Word of Recuperation.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Tue May 28 06:02:16 2019] Movanna has taken 3000 damage by Noxious Visions.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Tue May 28 06:02:56 2019] Movanna crushes Gomphus for 322 points of damage. (Riposte)");
            AddSkirmishTrackLine(skirmish, charTracker, "[Tue May 28 06:03:01 2019] Khronick healed you over time for 1518 hit points by Healing Counterbias Effect.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Tue May 28 06:03:19 2019] Harvester Collyx has taken 8206 damage from Blood of Jaled'Dar by Khronick. (Critical)");
            AddSkirmishTrackLine(skirmish, charTracker, "[Tue May 28 06:03:30 2019] Khronick has taken 1950 damage from Noxious Visions by Gomphus.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Tue May 28 06:03:53 2019] You kick Harvester Collyx for 13333 points of damage. (Riposte Critical)");
            AddSkirmishTrackLine(skirmish, charTracker, "[Tue May 28 06:03:53 2019] You have slain Harvester Collyx!");
            AddSkirmishTrackLine(skirmish, charTracker, "[Tue May 28 06:04:04 2019] Gomphus hits Movanna for 2076 points of damage.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Tue May 28 06:04:04 2019] Movanna has been slain by Gomphus!");
            AddSkirmishTrackLine(skirmish, charTracker, "[Tue May 28 06:04:36 2019] Gomphus hits Khronick for 6177 points of damage.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Tue May 28 06:04:36 2019] Khronick has been slain by Gomphus!");

            // Skirmish stats
            VerifySkirmishStats(skirmish, 42707, 56200, 2, 3);

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

        [TestMethod]
        public void FightWithAddOfSameTypeThenOneDies()
        {
            var skirmish = SetupNewSkirmish(out CharacterTracker charTracker);

            // Setup a fight and get an add (no way to tell it's an add)
            // Then have the first die and a new fight should be made
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:28:43 2019] You hit a cliknar sporali farmer for 2 points of magic damage by Distant Strike I.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:28:49 2019] A cliknar sporali farmer hits YOU for 1048 points of damage.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:28:51 2019] A cliknar sporali farmer hits Khronick for 3791 points of damage. (Strikethrough)"); // Let's pretend this is the add ... going after the healer
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:28:56 2019] You kick a cliknar sporali farmer for 2044 points of damage. (Riposte Critical)");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:29:58 2019] You kick a cliknar sporali farmer for 75472 points of damage. (Riposte Finishing Blow)");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:29:58 2019] You have slain a cliknar sporali farmer!"); // First mob dies
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:31:14 2019] A cliknar sporali farmer hits YOU for 4342 points of damage.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:31:19 2019] You kick a cliknar sporali farmer for 3224 points of damage. (Riposte)");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:32:32 2019] You crush a cliknar sporali farmer for 75203 points of damage. (Finishing Blow)");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:32:32 2019] You have slain a cliknar sporali farmer!"); // Second mob dies

            // Skirmish stats
            VerifySkirmishStats(skirmish, 165126, 0, 0, 2);

            // Skirmish Fighters
            Assert.AreEqual(3, skirmish.Fighters.Count());
            VerifyFighterStatistics("Khadaji", skirmish, 155945, 0, 0, 2, 5390, 0, 0, 0);
            VerifyFighterStatistics("Khronick", skirmish, 0, 0, 0, 0, 3791, 0, 0, 0);
            VerifyFighterStatistics("a cliknar sporali farmer", skirmish, 9181, 0, 0, 0, 155945, 0, 0, 2);

            // Skirmish Fights
            Assert.AreEqual(2, skirmish.Fights.Count);
            VerifyFightStatistics("a cliknar sporali farmer", skirmish, 82357, 0, 0, 1);

            // The helper method just grabs the first match of the mobName which would be the fight above, so get the second/last fight our own way
            var lastFight = skirmish.Fights.LastOrDefault(x => x.PrimaryMob.Name == "a cliknar sporali farmer");
            VerifyFightStatistics("a cliknar sporali farmer", lastFight, 82769, 0, 0, 1);
        }

        [TestMethod]
        public void FightWithAddSameSingleName()
        {
            var skirmish = SetupNewSkirmish(out CharacterTracker charTracker);

            // Setup a fight and get an add (no way to tell it's an add)
            // Then have the first die and a new fight should be made
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:28:43 2019] You hit Bob for 2 points of magic damage by Distant Strike I.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:28:49 2019] Bob hits YOU for 10 points of damage.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:28:56 2019] You kick Bob for 6 points of damage. (Riposte Critical)");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:29:58 2019] You have slain Bob!");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:31:14 2019] Bob hits YOU for 3 points of damage.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:31:19 2019] You kick Bob for 7 points of damage. (Riposte)");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:32:32 2019] You have slain Bob!"); // Second mob dies

            // Skirmish stats
            VerifySkirmishStats(skirmish, 28, 0, 0, 2);

            // Skirmish Fighters
            Assert.AreEqual(2, skirmish.Fighters.Count());
            VerifyFighterStatistics("Khadaji", skirmish, 15, 0, 0, 2, 13, 0, 0, 0);
            VerifyFighterStatistics("Bob", skirmish, 13, 0, 0, 0, 15, 0, 0, 2);

            // Skirmish Fights
            Assert.AreEqual(2, skirmish.Fights.Count);
            VerifyFightStatistics("Bob", skirmish, 18, 0, 0, 1);

            // The helper method just grabs the first match of the mobName which would be the fight above, so get the second/last fight our own way
            var lastFight = skirmish.Fights.LastOrDefault(x => x.PrimaryMob.Name == "Bob");
            VerifyFightStatistics("Bob", lastFight, 10, 0, 0, 1);
        }

        [TestMethod]
        public void SoloFightAgainstTwoSingleNamedMobs()
        {
            var skirmish = SetupNewSkirmish(out CharacterTracker charTracker);

            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:28:43 2019] You hit Mob1 for 2 points of magic damage by Distant Strike I.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:28:49 2019] Mob1 hits YOU for 10 points of damage.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:28:56 2019] You kick Mob1 for 6 points of damage. (Riposte Critical)");

            // This depends on the CharTracker tracking that Mob2 is attacking "You" and classifying it as NPC. The tracker will be called from Battle.AddLine()
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:31:14 2019] Mob2 hits YOU for 3 points of damage.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:31:19 2019] You kick Mob2 for 7 points of damage. (Riposte)");

            // Skirmish stats
            VerifySkirmishStats(skirmish, 28, 0, 0, 0);

            // Skirmish Fighters
            Assert.AreEqual(3, skirmish.Fighters.Count());
            VerifyFighterStatistics("Khadaji", skirmish, 15, 0, 0, 0, 13, 0, 0, 0);
            VerifyFighterStatistics("Mob1", skirmish, 10, 0, 0, 0, 8, 0, 0, 0);
            VerifyFighterStatistics("Mob2", skirmish, 3, 0, 0, 0, 7, 0, 0, 0);

            // Skirmish Fights
            Assert.AreEqual(2, skirmish.Fights.Count);
            VerifyFightStatistics("Mob1", skirmish, 18, 0, 0, 0);
            VerifyFightStatistics("Mob2", skirmish, 10, 0, 0, 0);
        }

        [TestMethod]
        public void SkirmishWhereFirstMobDiesAndSecondMobWeDontKnowIsAMobThatDamageGoesToThatNewFight()
        {
            var skirmish = SetupNewSkirmish(out CharacterTracker charTracker);

            // Fight and kill the first mob
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:28:43 2019] A generic mob hits YOU for 7 points of damage.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:28:43 2019] You crush a generic mob for 10 points of damage.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:28:43 2019] You have slain a generic mob!");

            // Second is a named mob (a mob that we don't know is a mob yet)
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:28:45 2019] Mob1 hits Player1 for 10 points of damage.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:28:46 2019] Player2 slashes Mob1 for 9 points of damage."); // This line is used to establish Mob1 as PrimaryMob

            // Skirmish Fights
            Assert.AreEqual(2, skirmish.Fights.Count);
            VerifyFightStatistics("a generic mob", skirmish, 17, 0, 0, 1);
            VerifyFightStatistics("Mob1", skirmish, 19, 0, 0, 0);
        }

        [TestMethod]
        public void IsSkirmishOverJustWhenItsGettingStarted()
        {
            var skirmish = SetupNewSkirmish(out CharacterTracker charTracker);
            Assert.IsFalse(skirmish.IsFightOver);
        }

        [TestMethod]
        public void SkirmishIsntOverAfterOneHit()
        {
            var skirmish = SetupNewSkirmish(out CharacterTracker charTracker);
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:28:43 2019] You crush a generic mob for 10 points of damage.");

            Assert.IsFalse(skirmish.IsFightOver);
        }

        [TestMethod]
        public void SkirmishIsntOverAfterOneHeal()
        {
            var skirmish = SetupNewSkirmish(out CharacterTracker charTracker);
            AddSkirmishTrackLine(skirmish, charTracker, "[Tue May 28 06:02:16 2019] You feel the touch of recuperation. Movanna healed you for 20053 (23605) hit points by Word of Recuperation.");

            Assert.IsFalse(skirmish.IsFightOver);
        }

        [TestMethod]
        public void SkirmishIsOverAfterOneKillShot()
        {
            var skirmish = SetupNewSkirmish(out CharacterTracker charTracker);
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:28:43 2019] You have slain a generic mob!");

            Assert.IsTrue(skirmish.IsFightOver);
        }

        [TestMethod]
        public void FightWithDotAfterDeath()
        {
            // Things to test...
            // - the DOT is put onto the fight that is now over
            // - the DOT is associated with the MOB that died ... (Posthumous Damage)

            var skirmish = SetupNewSkirmish(out CharacterTracker charTracker);

            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 06:57:07 2019] You crush a sandspinner stalker for 708 points of damage.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 06:57:09 2019] You have taken 2080 damage from Paralyzing Bite by a sandspinner stalker.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 06:57:09 2019] You kick a sandspinner stalker for 75472 points of damage. (Finishing Blow)");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 06:57:09 2019] You have slain a sandspinner stalker!");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 06:57:15 2019] You have taken 2080 damage from Paralyzing Bite.");

            Assert.AreEqual(1, skirmish.Fights.Count);
            VerifyFightStatistics("a sandspinner stalker", skirmish, 80340, 0, 0, 1);

            Assert.AreEqual(2, skirmish.Fighters.Count());
            VerifyFighterStatistics("Khadaji", skirmish, 76180, 0, 0, 1, 4160, 0, 0, 0);
            VerifyFighterStatistics("a sandspinner stalker", skirmish, 4160, 0, 0, 0, 76180, 0, 0, 1);
        }

        [TestMethod]
        public void SimilarDotDamage()
        {
            var skirmish = SetupNewSkirmish(out CharacterTracker charTracker);

            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 06:57:07 2019] You crush a sandspinner stalker for 708 points of damage.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 06:57:09 2019] You have taken 2080 damage from Paralyzing Bite by a sandspinner stalker.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 06:57:15 2019] You have taken 2080 damage from Paralyzing Bite.");

            Assert.AreEqual(1, skirmish.Fights.Count);
            VerifyFightStatistics("a sandspinner stalker", skirmish, 4868, 0, 0, 0);

            Assert.AreEqual(2, skirmish.Fighters.Count());
            VerifyFighterStatistics("Khadaji", skirmish, 708, 0, 0, 0, 4160, 0, 0, 0);
            VerifyFighterStatistics("a sandspinner stalker", skirmish, 4160, 0, 0, 0, 708, 0, 0, 0);
        }

        [TestMethod]
        public void FightMobWithDoTAfterDeathButDuringSecondMobFight()
        {
            var skirmish = SetupNewSkirmish(out CharacterTracker charTracker);

            // Mob #1, with DoT
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 06:57:07 2019] You crush a sandspinner stalker for 708 points of damage.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 06:57:09 2019] You have taken 2080 damage from Paralyzing Bite by a sandspinner stalker.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 06:57:09 2019] You kick a sandspinner stalker for 75472 points of damage. (Finishing Blow)");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 06:57:09 2019] You have slain a sandspinner stalker!");

            // Mob #2 fight start
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 06:57:12 2019] You kick a cliknar battle drone for 1074 points of damage. (Critical)");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 06:57:12 2019] A cliknar battle drone bites YOU for 2415 points of damage.");

            // This should be assigned to Mob #1
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 06:57:15 2019] You have taken 2080 damage from Paralyzing Bite.");

            // Now, back to Mob #2
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 06:57:15 2019] You strike a cliknar battle drone for 580 points of damage.");

            Assert.AreEqual(2, skirmish.Fights.Count);
            VerifyFightStatistics("a sandspinner stalker", skirmish, 80340, 0, 0, 1);
            VerifyFightStatistics("a cliknar battle drone", skirmish, 4069, 0, 0, 0);

            Assert.AreEqual(3, skirmish.Fighters.Count());
            VerifyFighterStatistics("Khadaji", skirmish, 77834, 0, 0, 1, 6575, 0, 0, 0);
            VerifyFighterStatistics("a sandspinner stalker", skirmish, 4160, 0, 0, 0, 76180, 0, 0, 1);
            VerifyFighterStatistics("a cliknar battle drone", skirmish, 2415, 0, 0, 0, 1654, 0, 0, 0);
        }

        [TestMethod]
        public void FightWithDoTAfterDeathNotAnonymousDamage()
        {
            var skirmish = SetupNewSkirmish(out CharacterTracker charTracker);

            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:58:34 2019] You strike Gomphus for 577 points of damage.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:58:34 2019] You have taken 1950 damage from Noxious Visions by Gomphus.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:58:37 2019] You strike Gomphus for 572 points of damage.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:58:37 2019] You have slain Gomphus!");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:58:40 2019] You have taken 1950 damage from Noxious Visions by Gomphus's corpse.");

            Assert.AreEqual(1, skirmish.Fights.Count);
            VerifyFightStatistics("Gomphus", skirmish, 5049, 0, 0, 1);

            Assert.AreEqual(2, skirmish.Fighters.Count());
            VerifyFighterStatistics("Khadaji", skirmish, 1149, 0, 0, 1, 3900, 0, 0, 0);
            VerifyFighterStatistics("Gomphus", skirmish, 3900, 0, 0, 0, 1149, 0, 0, 1);
        }

        [TestMethod]
        public void FightWithDoTAfterButNotBeforeDeathNotAnonymousDamage()
        {
            var skirmish = SetupNewSkirmish(out CharacterTracker charTracker);

            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:58:34 2019] You strike Gomphus for 577 points of damage.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:58:37 2019] You strike Gomphus for 572 points of damage.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:58:37 2019] You have slain Gomphus!");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 09:58:40 2019] You have taken 1950 damage from Noxious Visions by Gomphus's corpse.");

            Assert.AreEqual(1, skirmish.Fights.Count);
            VerifyFightStatistics("Gomphus", skirmish, 3099, 0, 0, 1);

            Assert.AreEqual(2, skirmish.Fighters.Count());
            VerifyFighterStatistics("Khadaji", skirmish, 1149, 0, 0, 1, 1950, 0, 0, 0);
            VerifyFighterStatistics("Gomphus", skirmish, 1950, 0, 0, 0, 1149, 0, 0, 1);
        }


        [TestMethod]
        public void NotAnonymousDotDamageNoCorpse()
        {
            var skirmish = SetupNewSkirmish(out CharacterTracker charTracker);

            AddSkirmishTrackLine(skirmish, charTracker, "[Mon Oct 26 20:21:49 2020] A Syldon drill sergeant has taken 49611 damage from Phase Spider Blood by Khronick. (Critical)");

            Assert.AreEqual(1, skirmish.Fights.Count);
            VerifyFightStatistics("a Syldon drill sergeant", skirmish, 49611, 0, 0, 0);

            Assert.AreEqual(2, skirmish.Fighters.Count());
            VerifyFighterStatistics("Khronick", skirmish, 49611, 0, 0, 0, 0, 0, 0, 0);
            VerifyFighterStatistics("a Syldon drill sergeant", skirmish, 0, 0, 0, 0, 49611, 0, 0, 0);
        }

        [TestMethod]
        public void NotAnonymousDotDamageWithCorpse()
        {
            var skirmish = SetupNewSkirmish(out CharacterTracker charTracker);

            AddSkirmishTrackLine(skirmish, charTracker, "[Sun Apr 12 13:10:03 2020] You have taken 14 damage from Strong Poison by a moss viper's corpse.");

            Assert.AreEqual(1, skirmish.Fights.Count);
            VerifyFightStatistics("a moss viper", skirmish, 14, 0, 0, 0);

            Assert.AreEqual(2, skirmish.Fighters.Count());
            VerifyFighterStatistics("Khadaji", skirmish, 0, 0, 0, 0, 14, 0, 0, 0);
            VerifyFighterStatistics("a moss viper", skirmish, 14, 0, 0, 0, 0, 0, 0, 0);
        }

        [TestMethod]
        public void NotAnonymousDotDamageWithCorpseByOther()
        {
            var skirmish = SetupNewSkirmish(out CharacterTracker charTracker);

            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Aug 15 11:23:51 2020] Brocli has taken 10 damage from Chaos Claws by a noc's corpse.");

            Assert.AreEqual(1, skirmish.Fights.Count);
            VerifyFightStatistics("a noc", skirmish, 10, 0, 0, 0);

            Assert.AreEqual(2, skirmish.Fighters.Count());
            VerifyFighterStatistics("Brocli", skirmish, 0, 0, 0, 0, 10, 0, 0, 0);
            VerifyFighterStatistics("a noc", skirmish, 10, 0, 0, 0, 0, 0, 0, 0);
        }

        [TestMethod]
        public void NotAnonymousDotDamageWithCorpseDoubleApostrophe()
        {
            var skirmish = SetupNewSkirmish(out CharacterTracker charTracker);

            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Sep 12 11:05:27 2020] Neophine has taken 1351 damage from Livio's Affliction by a sarnak enthusiast's corpse.");

            Assert.AreEqual(1, skirmish.Fights.Count);
            VerifyFightStatistics("a sarnak enthusiast", skirmish, 1351, 0, 0, 0);

            Assert.AreEqual(2, skirmish.Fighters.Count());
            VerifyFighterStatistics("Neophine", skirmish, 0, 0, 0, 0, 1351, 0, 0, 0);
            VerifyFighterStatistics("a sarnak enthusiast", skirmish, 1351, 0, 0, 0, 0, 0, 0, 0);
        }

        [TestMethod]
        public void NotAnonymousDotDamageNoCorpseDoubleApostrophe()
        {
            var skirmish = SetupNewSkirmish(out CharacterTracker charTracker);

            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Sep 12 11:04:44 2020] Neophine has taken 1351 damage from Livio's Affliction by a sarnak enthusiast.");

            Assert.AreEqual(1, skirmish.Fights.Count);
            VerifyFightStatistics("a sarnak enthusiast", skirmish, 1351, 0, 0, 0);

            Assert.AreEqual(2, skirmish.Fighters.Count());
            VerifyFighterStatistics("Neophine", skirmish, 0, 0, 0, 0, 1351, 0, 0, 0);
            VerifyFighterStatistics("a sarnak enthusiast", skirmish, 1351, 0, 0, 0, 0, 0, 0, 0);
        }

        [TestMethod]
        public void NotAnonymousDotDamageByApostropheMob()
        {
            var skirmish = SetupNewSkirmish(out CharacterTracker charTracker);

            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Sep 12 16:25:04 2020] You have taken 57121 damage from Dread Admiral's Curse by Arisen Gloriant Kra`du's corpse.");

            Assert.AreEqual(1, skirmish.Fights.Count);
            VerifyFightStatistics("Arisen Gloriant Kra`du", skirmish, 57121, 0, 0, 0);

            Assert.AreEqual(2, skirmish.Fighters.Count());
            VerifyFighterStatistics("Khadaji", skirmish, 0, 0, 0, 0, 57121, 0, 0, 0);
            VerifyFighterStatistics("Arisen Gloriant Kra`du", skirmish, 57121, 0, 0, 0, 0, 0, 0, 0);
        }

        [TestMethod]
        public void NotAnonymousDotDamageOnOtherByApostropheMob()
        {
            var skirmish = SetupNewSkirmish(out CharacterTracker charTracker);

            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Sep 12 16:24:46 2020] Khadajitwo has taken 56242 damage from Dread Admiral's Curse by Arisen Gloriant Kra`du.");

            Assert.AreEqual(1, skirmish.Fights.Count);
            VerifyFightStatistics("Arisen Gloriant Kra`du", skirmish, 56242, 0, 0, 0);

            Assert.AreEqual(2, skirmish.Fighters.Count());
            VerifyFighterStatistics("Khadajitwo", skirmish, 0, 0, 0, 0, 56242, 0, 0, 0);
            VerifyFighterStatistics("Arisen Gloriant Kra`du", skirmish, 56242, 0, 0, 0, 0, 0, 0, 0);
        }

        [TestMethod]
        public void AnonymousDotDamageAfterCure()
        {
            var skirmish = SetupNewSkirmish(out CharacterTracker charTracker);

            // This is an interesting one. The mob casts and lands a Dot, then dies before it takes effect
            // So, won't be able to look up previous DoT lines (though this is "corpse" damage, so it's clear who the mob is
            // Then, the anonymous DoT line can look up the previous line

            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Sep 19 14:14:19 2020] Deathfang begins casting Rabid Anklebite.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Sep 19 14:14:19 2020] You are infected by a rabid bite.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Sep 19 14:14:24 2020] Twoco slashes Deathfang for 82069 points of damage. (Lucky Critical Flurry)");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Sep 19 14:14:24 2020] Deathfang has been slain by Twoco!");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Sep 19 14:14:25 2020] You have taken 768 damage from Rabid Anklebite by Deathfang's corpse.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Sep 19 14:14:27 2020] Jathenai begins casting Counteract Disease.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Sep 19 14:14:31 2020] You have taken 768 damage from Rabid Anklebite by Deathfang's corpse.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Sep 19 14:14:32 2020] Jathenai begins casting Counteract Disease.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Sep 19 14:14:37 2020] You have taken 768 damage from Rabid Anklebite by Deathfang's corpse.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Sep 19 14:14:37 2020] Jathenai begins casting Counteract Disease.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Sep 19 14:14:42 2020] Jathenai begins casting Counteract Disease.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Sep 19 14:14:43 2020] You feel better.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Sep 19 14:14:43 2020] You have taken 768 damage from Rabid Anklebite.");

            VerifySkirmishStats(skirmish, 85141, 0, 0, 1);

            var fight = VerifyFightStatistics("Deathfang", skirmish, 85141, 0, 0, 1);
            VerifyFighterStatistics("Twoco", fight, 82069, 0, 0, 1, 0, 0, 0, 0);
            VerifyFighterStatistics("Khadaji", fight, 0, 0, 0, 0, 3072, 0, 0, 0);
        }


        [TestMethod]
        public void AnonymousDotDamageOnMercFirstLine()
        {
            var skirmish = SetupNewSkirmish(out CharacterTracker charTracker);

            // Note: For a merc, it's the same message before the mob dies as after
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Oct 10 12:52:58 2020] Vryklak has taken 28350 damage by Aura of the Kar`Zok.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Oct 10 12:52:59 2020] You have taken 28350 damage from Aura of the Kar`Zok by a Kar`Zok scourge.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Oct 10 12:53:02 2020] Ilyiche has taken 28350 damage from Aura of the Kar`Zok by a Kar`Zok scourge.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Oct 10 12:53:04 2020] Bloodyassassin has taken 28800 damage from Aura of the Kar`Zok by a Kar`Zok scourge's corpse.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Oct 10 12:53:04 2020] Vryklak has taken 28350 damage by Aura of the Kar`Zok.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Oct 10 12:53:04 2020] Vryklak died.");

            VerifySkirmishStats(skirmish, 142200, 0, 0, 1);

            var fight = VerifyFightStatistics("a Kar`Zok scourge", skirmish, 142200, 0, 0, 1);
            Assert.AreEqual(6, fight.Fighters.Count());
            //VerifyFighterStatistics("a Kar`Zok scourge", fight, 142200, 0, 0, 1, 0, 0, 0, 0); // This is the proper number if we can handle the Unknown attacker in the first line. Also kill count if we can associate that as well.
            VerifyFighterStatistics("a Kar`Zok scourge", fight, 113850, 0, 0, 0, 0, 0, 0, 0);
            VerifyFighterStatistics("Vryklak", fight, 0, 0, 0, 0, 56700, 0, 0, 1);
            VerifyFighterStatistics("Khadaji", fight, 0, 0, 0, 0, 28350, 0, 0, 0);
            VerifyFighterStatistics("Ilyiche", fight, 0, 0, 0, 0, 28350, 0, 0, 0);
            VerifyFighterStatistics("Bloodyassassin", fight, 0, 0, 0, 0, 28800, 0, 0, 0);

            // Can we assign this Unknown damage from the first line to the mob once we get other lines?
            // Keep track of unknown DoT lines and see if we can adjust them post-facto as we get new lines coming in (that match the DamageBy type)
            VerifyFighterStatistics("Unknown", fight, 28350, 0, 0, 1, 0, 0, 0, 0);
        }

        [TestMethod]
        public void AnonymousDotDamageOnMercSecondLine()
        {
            var skirmish = SetupNewSkirmish(out CharacterTracker charTracker);

            // Note: For a merc, it's the same message before the mob dies as after
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Oct 10 12:52:59 2020] You have taken 28350 damage from Aura of the Kar`Zok by a Kar`Zok scourge.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Oct 10 12:52:59 2020] Vryklak has taken 28350 damage by Aura of the Kar`Zok.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Oct 10 12:53:02 2020] Ilyiche has taken 28350 damage from Aura of the Kar`Zok by a Kar`Zok scourge.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Oct 10 12:53:04 2020] Bloodyassassin has taken 28800 damage from Aura of the Kar`Zok by a Kar`Zok scourge's corpse.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Oct 10 12:53:04 2020] Vryklak has taken 28350 damage by Aura of the Kar`Zok.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Oct 10 12:53:04 2020] Vryklak died.");

            VerifySkirmishStats(skirmish, 142200, 0, 0, 1);

            var fight = VerifyFightStatistics("a Kar`Zok scourge", skirmish, 142200, 0, 0, 1);
            Assert.AreEqual(6, fight.Fighters.Count());
            VerifyFighterStatistics("a Kar`Zok scourge", fight, 142200, 0, 0, 0, 0, 0, 0, 0);
            VerifyFighterStatistics("Vryklak", fight, 0, 0, 0, 0, 56700, 0, 0, 1);
            VerifyFighterStatistics("Khadaji", fight, 0, 0, 0, 0, 28350, 0, 0, 0);
            VerifyFighterStatistics("Ilyiche", fight, 0, 0, 0, 0, 28350, 0, 0, 0);
            VerifyFighterStatistics("Bloodyassassin", fight, 0, 0, 0, 0, 28800, 0, 0, 0);

            // Can we know how the merc died?
            // Can we assume that the last damage line they received was responsible (and the mob) for killing them?
            VerifyFighterStatistics("Unknown", fight, 0, 0, 0, 1, 0, 0, 0, 0);
        }


        [TestMethod]
        public void PosthumousDotDamageOnGroup()
        {
            var skirmish = SetupNewSkirmish(out CharacterTracker charTracker);

            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Sep 12 16:24:46 2020] You have taken 57121 damage from Dread Admiral's Curse by Arisen Gloriant Kra`du.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Sep 12 16:24:46 2020] Khadajitwo has taken 56242 damage from Dread Admiral's Curse by Arisen Gloriant Kra`du.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Sep 12 16:24:46 2020] Refrainxv has taken 56242 damage from Dread Admiral's Curse by Arisen Gloriant Kra`du.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Sep 12 16:24:46 2020] Neophine has taken 35987 damage from Dread Admiral's Curse by Arisen Gloriant Kra`du.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Sep 12 16:25:00 2020] Refrainxv`s pet hits Arisen Gloriant Kra`du for 840 points of damage. (Strikethrough)");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Sep 12 16:25:00 2020] Refrainxv`s pet hits Arisen Gloriant Kra`du for 1140 points of damage.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Sep 12 16:25:00 2020] You kick Arisen Gloriant Kra`du for 28793 points of damage.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Sep 12 16:25:00 2020] You gain party experience!");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Sep 12 16:25:00 2020] Arisen Gloriant Kra`du's corpse says, 'Fie to thee intruder! The Kunzar will have their revenge!'");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Sep 12 16:25:00 2020] You have slain Arisen Gloriant Kra`du!");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Sep 12 16:25:10 2020] Goingape has taken 56242 damage from Dread Admiral's Curse by Arisen Gloriant Kra`du's corpse.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Sep 12 16:25:10 2020] You have taken 57121 damage from Dread Admiral's Curse by Arisen Gloriant Kra`du's corpse.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Sep 12 16:25:10 2020] Khadajitwo has taken 56242 damage from Dread Admiral's Curse by Arisen Gloriant Kra`du's corpse.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Sep 12 16:25:10 2020] Refrainxv has taken 56242 damage from Dread Admiral's Curse by Arisen Gloriant Kra`du's corpse.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Sep 12 16:25:10 2020] Neophine has taken 35987 damage from Dread Admiral's Curse by Arisen Gloriant Kra`du's corpse.");

            VerifySkirmishStats(skirmish, 498199, 0, 0, 1);

            var fight = VerifyFightStatistics("Arisen Gloriant Kra`du", skirmish, 498199, 0, 0, 1);
            Assert.AreEqual(7, fight.Fighters.Count());
            VerifyFighterStatistics("Arisen Gloriant Kra`du", fight, 467426, 0, 0, 0, 30773, 0, 0, 1);
            VerifyFighterStatistics("Khadaji", fight, 28793, 0, 0, 1, 114242, 0, 0, 0);
            VerifyFighterStatistics("Khadajitwo", fight, 0, 0, 0, 0, 112484, 0, 0, 0);
            VerifyFighterStatistics("Refrainxv", fight, 0, 0, 0, 0, 112484, 0, 0, 0);
            VerifyFighterStatistics("Refrainxv", fight, 1980, 0, 0, 0, 0, 0, 0, 0, true);
            VerifyFighterStatistics("Neophine", fight, 0, 0, 0, 0, 71974, 0, 0, 0);
            VerifyFighterStatistics("Goingape", fight, 0, 0, 0, 0, 56242, 0, 0, 0);
        }


        [TestMethod]
        public void PosthumousDotDamageOnGroupAfterSecondMob()
        {
            var skirmish = SetupNewSkirmish(out CharacterTracker charTracker);

            // Scenario where Dot hits one character before death
            // mob dies
            // anonymous dot hits a merc after death

            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Oct 10 12:53:02 2020] Ilyiche has taken 28350 damage from Aura of the Kar`Zok by a Kar`Zok scourge.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Oct 10 12:53:02 2020] Khadajitwo crushes a Kar`Zok scourge for 28651 points of damage. (Critical Flurry)");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Oct 10 12:53:02 2020] A Kar`Zok scourge has been slain by Khadajitwo!");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Oct 10 12:53:02 2020] You kick a Wulthan grand inquisitor for 19656 points of damage.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Oct 10 12:53:03 2020] You hit a Wulthan grand inquisitor for 14467 points of damage. (Lucky Critical)");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Oct 10 12:53:03 2020] A Wulthan grand inquisitor is burned by Vryklak's flames for 3873 points of non-melee damage.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Oct 10 12:53:03 2020] A Wulthan grand inquisitor slashes Vryklak for 13092 points of damage. (Riposte Strikethrough)");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Oct 10 12:53:04 2020] Bloodyassassin has taken 28800 damage from Aura of the Kar`Zok by a Kar`Zok scourge's corpse.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Oct 10 12:53:04 2020] Vryklak has taken 28350 damage by Aura of the Kar`Zok.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Oct 10 12:53:04 2020] Vryklak died.");

            VerifySkirmishStats(skirmish, 165239, 0, 0, 2);

            Assert.AreEqual(2, skirmish.Fights.Count);
            Assert.AreEqual(8, skirmish.Fighters.Count());

            var fight = VerifyFightStatistics("a Kar`Zok scourge", skirmish, 114151, 0, 0, 1);
            Assert.AreEqual(5, fight.Fighters.Count());
            VerifyFighterStatistics("a Kar`Zok scourge", fight, 85500, 0, 0, 0, 28651, 0, 0, 1);
            VerifyFighterStatistics("Ilyiche", fight, 0, 0, 0, 0, 28350, 0, 0, 0);
            VerifyFighterStatistics("Khadajitwo", fight, 28651, 0, 0, 1, 0, 0, 0, 0);
            VerifyFighterStatistics("Bloodyassassin", fight, 0, 0, 0, 0, 28800, 0, 0, 0);
            VerifyFighterStatistics("Vryklak", fight, 0, 0, 0, 0, 28350, 0, 0, 0);

            // Death goes to new fight. Be nice to associate a death (by anonymous mob) to the previous damage line (a dot line) and have it stick to the "correct" fight
            //VerifyFighterStatistics("Vryklak", fight, 0, 0, 0, 0, 28350, 0, 0, 1);
            //VerifyFighterStatistics("Unknown", fight, 0, 0, 0, 1, 0, 0, 0, 0);

            fight = VerifyFightStatistics("a Wulthan grand inquisitor", skirmish, 51088, 0, 0, 1);
            Assert.AreEqual(4, fight.Fighters.Count());
            VerifyFighterStatistics("a Wulthan grand inquisitor", fight, 13092, 0, 0, 0, 37996, 0, 0, 0);
            VerifyFighterStatistics("Khadaji", fight, 34123, 0, 0, 0, 0, 0, 0, 0);
            VerifyFighterStatistics("Vryklak", fight, 3873, 0, 0, 0, 13092, 0, 0, 1);
            VerifyFighterStatistics("Unknown", fight, 0, 0, 0, 1, 0, 0, 0, 0);
        }

        [TestMethod]
        public void PosthumousDotDamageOnGroupxxxx2()
        {
            var skirmish = SetupNewSkirmish(out CharacterTracker charTracker);

            // Scenario where:
            // mob dies
            // dot hits a PC after death with corpse as attacker


            //Assert.Fail();

        }

        [TestMethod]
        public void PosthumousDotDamageOnGroupxxxx3()
        {
            var skirmish = SetupNewSkirmish(out CharacterTracker charTracker);

            // Scenario where:
            // mob dies
            // anonymous dot hits a merc after death
            // assign that damage to the new fight? Or, if no new fight to the old fight.
            // anon DoT damage should go to current fight, or if none, to the last fight, it shouldn't cause a new fight to happen. Or, if these is no fight at all, it should start one


            //Assert.Fail();

        }

        [TestMethod]
        public void AnonymousDOTDamageWhileMobStillAlive()
        {
            var skirmish = SetupNewSkirmish(out CharacterTracker charTracker);

            AddSkirmishTrackLine(skirmish, charTracker, "[Fri Apr 26 09:30:44 2019] You punch a sandspinner juvenile for 1102 points of damage.");

            // https://everquest.allakhazam.com/db/spell.html?spell=17827
            // Spell cast message
            AddSkirmishTrackLine(skirmish, charTracker, "[Fri Apr 26 09:30:44 2019] You have been bitten and immediately feel the poison's effect.");

            AddSkirmishTrackLine(skirmish, charTracker, "[Fri Apr 26 09:30:47 2019] You punch a sandspinner juvenile for 1276 points of damage. (Critical)");
            AddSkirmishTrackLine(skirmish, charTracker, "[Fri Apr 26 09:30:47 2019] A sandspinner juvenile is burned by YOUR flames for 896 points of non-melee damage.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Fri Apr 26 09:30:47 2019] A sandspinner juvenile bites YOU for 3973 points of damage.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Fri Apr 26 09:30:49 2019] You have taken 2080 damage from Paralyzing Bite by a sandspinner juvenile.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Fri Apr 26 09:30:49 2019] Movanna healed you over time for 3666 hit points by Zealous Elixir.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Fri Apr 26 09:30:54 2019] You hit a sandspinner juvenile for 197 points of magic damage by Vampiric Strike II.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Fri Apr 26 09:30:54 2019] A sandspinner juvenile is withered by a vampiric strike.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Fri Apr 26 09:30:54 2019] You wither under a vampiric strike. You healed Khadaji for 227 hit points by Vampiric Strike II.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Fri Apr 26 09:30:55 2019] You have taken 2080 damage from Paralyzing Bite by a sandspinner juvenile.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Fri Apr 26 09:30:55 2019] Movanna healed you over time for 3666 hit points by Zealous Elixir.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Fri Apr 26 09:31:01 2019] You have taken 2080 damage from Paralyzing Bite by a sandspinner juvenile.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Fri Apr 26 09:31:02 2019] You kick a sandspinner juvenile for 2196 points of damage. (Riposte Critical)");
            AddSkirmishTrackLine(skirmish, charTracker, "[Fri Apr 26 09:31:02 2019] A sandspinner juvenile is burned by YOUR flames for 896 points of non-melee damage.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Fri Apr 26 09:31:02 2019] A sandspinner juvenile bites YOU for 3241 points of damage.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Fri Apr 26 09:31:07 2019] You hit a sandspinner juvenile for 309 points of disease damage by Strike of Disease II.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Fri Apr 26 09:31:07 2019] A sandspinner juvenile is struck by disease.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Fri Apr 26 09:31:07 2019] You hit a sandspinner juvenile for 1149 points of magic damage by Demon Crusher.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Fri Apr 26 09:31:07 2019] A sandspinner juvenile is struck by a blast of chi.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Fri Apr 26 09:31:07 2019] You have taken 2080 damage from Paralyzing Bite by a sandspinner juvenile.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Fri Apr 26 09:31:07 2019] The promise of divine restitution is fulfilled. You healed Khadaji for 15517 (23663) hit points by Promised Restitution Trigger I.");

            // Spell worn off message
            AddSkirmishTrackLine(skirmish, charTracker, "[Fri Apr 26 09:31:13 2019] You begin to feel better.");
            // Once it wears off, there's still a blip of DoT damage, but it's now anonymous
            AddSkirmishTrackLine(skirmish, charTracker, "[Fri Apr 26 09:31:13 2019] You have taken 2080 damage from Paralyzing Bite.");

            AddSkirmishTrackLine(skirmish, charTracker, "[Fri Apr 26 09:31:14 2019] You punch a sandspinner juvenile for 3114 points of damage. (Critical)");
            AddSkirmishTrackLine(skirmish, charTracker, "[Fri Apr 26 09:31:16 2019] You punch a sandspinner juvenile for 75231 points of damage. (Finishing Blow)");
            AddSkirmishTrackLine(skirmish, charTracker, "[Fri Apr 26 09:31:16 2019] You have slain a sandspinner juvenile!");

            VerifySkirmishStats(skirmish, 103980, 23076, 0, 1);

            Assert.AreEqual(1, skirmish.Fights.Count);
            Assert.AreEqual(3, skirmish.Fighters.Count());

            var fight = VerifyFightStatistics("a sandspinner juvenile", skirmish, 103980, 23076, 0, 1);
            Assert.AreEqual(3, fight.Fighters.Count());
            VerifyFighterStatistics("a sandspinner juvenile", fight, 17614, 0, 0, 0, 86366, 0, 0, 1);
            VerifyFighterStatistics("Khadaji", fight, 86366, 15744, 0, 1, 17614, 23076, 0, 0);
            VerifyFighterStatistics("Movanna", fight, 0, 7332, 0, 0, 0, 0, 0, 0);

        }

        [TestMethod]
        public void EnsureFightsInASkirmishUseTheirOwnDurationForDps()
        {
            var skirmish = SetupNewSkirmish(out CharacterTracker charTracker);

            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 07:18:30 2019] A sandspinner juvenile bites YOU for 2329 points of damage.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Mar 30 07:18:31 2019] Bealica hit a sandspinner juvenile for 1 points of fire damage by Tears of Flame.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Mar 30 07:18:34 2019] Bealica hit a sandspinner juvenile for 2 points of fire damage by Tears of Flame.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 07:18:35 2019] You crush a sandspinner juvenile for 4051 points of damage. (Riposte Critical)");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 07:18:36 2019] You have slain a sandspinner juvenile!");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 07:18:40 2019] A cliknar skirmish drone bites YOU for 961 points of damage.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Mar 30 07:18:42 2019] Bealica hit a cliknar skirmish drone for 3 points of fire damage by Tears of Flame.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Sat Mar 30 07:18:44 2019] Bealica hit a cliknar skirmish drone for 4 points of fire damage by Tears of Flame.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 07:18:45 2019] You kick a cliknar skirmish drone for 7555 points of damage.");
            AddSkirmishTrackLine(skirmish, charTracker, "[Mon May 27 07:18:45 2019] You have slain a cliknar skirmish drone!");

            // Skirmish stats
            VerifySkirmishStats(skirmish, 14906, 0, 0, 2);
            Assert.AreEqual(new TimeSpan(0, 0, 15), skirmish.Statistics.Duration.FightDuration);
            VerifyFighterDuration(skirmish, "Khadaji", new TimeSpan(0, 0, 15), new TimeSpan(0, 0, 10));
            VerifyFighterDuration(skirmish, "Bealica", new TimeSpan(0, 0, 15), new TimeSpan(0, 0, 13));

            // Fight1
            Assert.AreEqual(2, skirmish.Fights.Count);
            var fight = VerifyFightStatistics("a sandspinner juvenile", skirmish, 6383, 0, 0, 1);
            Assert.AreEqual(new TimeSpan(0, 0, 6), fight.Statistics.Duration.FightDuration);
            VerifyFighterDuration(fight, "Khadaji", new TimeSpan(0, 0, 6), new TimeSpan(0, 0, 1));
            VerifyFighterDuration(fight, "Bealica", new TimeSpan(0, 0, 6), new TimeSpan(0, 0, 3));

            // Fight2
            fight = VerifyFightStatistics("a cliknar skirmish drone", skirmish, 8523, 0, 0, 1);
            Assert.AreEqual(new TimeSpan(0, 0, 5), fight.Statistics.Duration.FightDuration);
            VerifyFighterDuration(fight, "Khadaji", new TimeSpan(0, 0, 5), new TimeSpan(0, 0, 0));
            VerifyFighterDuration(fight, "Bealica", new TimeSpan(0, 0, 5), new TimeSpan(0, 0, 2));
        }

        private Skirmish SetupNewSkirmish(out CharacterTracker charTracker)
        {
            var charResolver = new CharacterResolver();
            charTracker = new CharacterTracker(YouAre, charResolver);
            return new Skirmish(YouAre, charResolver);
        }
    }
}
