using System;
using System.Linq;
using BizObjects.Battle;
using BizObjects.Lines;
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
    public class BattleTests : FightTestBase
    {
        private readonly Action<Battle, string> AddBattleLine = (battle, logLine) => battle.AddLine((dynamic)_parser.ParseLine(new LogDatum(logLine)));

        [TestMethod]
        public void AddNonAttackLineToNewBattleSoSkirmishIsAdded()
        {
            var battle = SetupNewBattle();
            Assert.AreEqual(0, battle.Skirmishes.Count);

            AddBattleLine(battle, "[Sat Sep 07 18:52:44 2019] You are not currently assigned to an adventure.");

            Assert.AreEqual(1, battle.Skirmishes.Count);
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

        [TestMethod]
        public void May24ScenarioHealAfterFightShouldApplyToFightNotStartANewFight()
        {
            var battle = SetupNewBattle();

            AddBattleLine(battle, "[Fri May 24 18:25:42 2019] Jebartik hit a cliknar scout drone for 1026 points of cold damage by Water Elemental Strike VIII.");
            AddBattleLine(battle, "[Fri May 24 18:25:43 2019] A cliknar scout drone has taken 3289 damage from Breath of the Shiverback Rk. II by Zenarya.");
            AddBattleLine(battle, "[Fri May 24 18:25:47 2019] Eruzen hit a cliknar scout drone for 4435 points of corruption damage by Force of Corruption X. (Critical)");
            AddBattleLine(battle, "[Fri May 24 18:25:48 2019] Eruzen hit a cliknar scout drone for 72287 points of fire damage by Spear of Magma Rk. II. (Critical Twincast)");
            AddBattleLine(battle, "[Fri May 24 18:25:48 2019] A cliknar scout drone has been slain by Eruzen!");

            AddBattleLine(battle, "[Fri May 24 18:25:49 2019] Jebartik hit a cliknar scout drone for 985 points of chromatic damage by Tiger Maw II.");
            AddBattleLine(battle, "[Fri May 24 18:25:49 2019] Vatalae hit a cliknar scout drone for 985 points of chromatic damage by Tiger Maw II.");
            AddBattleLine(battle, "[Fri May 24 18:25:52 2019] A cliknar scout drone has taken 10151 damage from Breath of the Shiverback Rk. II by Zenarya. (Critical)");
            AddBattleLine(battle, "[Fri May 24 18:25:52 2019] A cliknar scout drone has been slain by Zenarya!");

            // This should get put onto the last fight
            AddBattleLine(battle, "[Fri May 24 18:25:53 2019] Vatalae is infused by a divine restitution. Vatalae healed herself for 5843 (24815) hit points by Promised Restitution Trigger II.");

            AddBattleLine(battle, "[Fri May 24 18:29:42 2019] You hit a cliknar scout drone for 2 points of magic damage by Distant Strike I.");
            AddBattleLine(battle, "[Fri May 24 18:29:44 2019] A cliknar scout drone bites YOU for 3400 points of damage. (Riposte Strikethrough)");
            AddBattleLine(battle, "[Fri May 24 18:29:44 2019] You kick a cliknar scout drone for 2247 points of damage. (Critical)");

            Assert.AreEqual(2, battle.Skirmishes.Count);

            var skirmish1 = battle.Skirmishes.First() as Skirmish;
            VerifySkirmishStats(skirmish1, 93158, 5843, 0, 2);

            Assert.AreEqual(2, skirmish1.Fights.Count);
            // 81037
            // 12121


            var skirmish2 = battle.Skirmishes.Last() as Skirmish;
            VerifySkirmishStats(skirmish2, 5649, 0, 0, 0);

            Assert.AreEqual(1, skirmish2.Fights.Count);
        }

        [TestMethod]
        public void GapBetweenAttacksIsANewSkirmishEvenIfNotDead()
        {
            var battle = SetupNewBattle();

            AddBattleLine(battle, "[Mon May 27 09:27:00 2019] You strike a sporali foodling for 975 points of damage.");
            AddBattleLine(battle, "[Mon May 27 09:27:00 2019] A sporali foodling hits YOU for 953 points of damage.");

            AddBattleLine(battle, "[Mon May 27 09:27:30 2019] You kick a sporali foodling for 1866 points of damage. (Critical)");
            AddBattleLine(battle, "[Mon May 27 09:27:30 2019] A sporali foodling hits YOU for 3002 points of damage.");

            Assert.AreEqual(2, battle.Skirmishes.Count);
        }

        [TestMethod]
        [Ignore]
        public void PullBeforeTheFightShouldBeTogetherEvenWithGap()
        {
            var battle = SetupNewBattle();

            AddBattleLine(battle, "[Fri Apr 26 09:25:35 2019] You hit a cliknar adept for 2 points of magic damage by Distant Strike I.");

            // Here the time gap is big enough to force this to be a new Skirmish, let's sticky them together
            AddBattleLine(battle, "[Fri Apr 26 09:25:43 2019] A cliknar adept hits YOU for 1780 points of damage.");
            AddBattleLine(battle, "[Fri Apr 26 09:25:45 2019] You kick a cliknar adept for 2894 points of damage.");

            Assert.AreEqual(1, battle.Skirmishes.Count);
            var skirmish1 = battle.Skirmishes.First() as Skirmish;
            Assert.AreEqual(1, skirmish1.Fights.Count);

            // Stats should not include the pull damage nor the pull time (don't want DPS to be diluted)
        }

        [TestMethod]
        [Ignore] // TODO
        public void PullWithSmallGapShouldHaveStatsSeparated()
        {
            var battle = SetupNewBattle();

            AddBattleLine(battle, "[Mon May 27 07:25:30 2019] You hit a cliknar skirmish drone for 2 points of magic damage by Distant Strike I.");
            AddBattleLine(battle, "[Mon May 27 07:25:32 2019] A cliknar skirmish drone tries to bite YOU, but YOU dodge!");
            AddBattleLine(battle, "[Mon May 27 07:25:32 2019] A cliknar skirmish drone bites YOU for 4338 points of damage. (Strikethrough)");
            AddBattleLine(battle, "[Mon May 27 07:25:36 2019] A cliknar skirmish drone bites YOU for 2145 points of damage.");
            AddBattleLine(battle, "[Mon May 27 07:25:37 2019] Auto attack is on.");
            AddBattleLine(battle, "[Mon May 27 07:25:37 2019] A cliknar skirmish drone bashes YOU for 684 points of damage.");
            AddBattleLine(battle, "[Mon May 27 07:25:38 2019] Khronick healed you over time for 3036 hit points by Healing Counterbias Effect. (Critical)");
            AddBattleLine(battle, "[Mon May 27 07:25:38 2019] A cliknar skirmish drone bites YOU for 865 points of damage.");
            AddBattleLine(battle, "[Mon May 27 07:25:38 2019] A cliknar skirmish drone tries to bite YOU, but YOU block!");
            AddBattleLine(battle, "[Mon May 27 07:25:38 2019] A cliknar skirmish drone bites YOU for 3608 points of damage.");
            AddBattleLine(battle, "[Mon May 27 07:25:40 2019] You crush a cliknar skirmish drone for 2278 points of damage.");
            AddBattleLine(battle, "[Mon May 27 07:25:40 2019] You crush a cliknar skirmish drone for 2278 points of damage.");
            AddBattleLine(battle, "[Mon May 27 07:25:40 2019] You crush a cliknar skirmish drone for 2188 points of damage.");
            AddBattleLine(battle, "[Mon May 27 07:25:40 2019] You crush a cliknar skirmish drone for 6671 points of damage. (Critical)");

            var skirmish = battle.Skirmishes.First() as Skirmish;
            var fighter = skirmish.Fighters.FirstOrDefault(x => x.Character.Name == "Khadaji");
            Assert.AreEqual(new TimeSpan(0, 0, 6), fighter.DefensiveStatistics.Duration.FighterDuration);
            Assert.AreEqual(new TimeSpan(0, 0, 1), fighter.OffensiveStatistics.Duration.FighterDuration);
        }

        [TestMethod]
        [Ignore]
        public void PullWithinGapOfFirstSkirmishShouldMoveToNextSkirmish()
        {
            var battle = SetupNewBattle();

            // First Skirmish/Fight
            AddBattleLine(battle, "[Mon May 27 07:30:03 2019] A cliknar battle drone bites YOU for 992 points of damage.");
            AddBattleLine(battle, "[Mon May 27 07:30:03 2019] You kick a cliknar battle drone for 163537 points of damage. (Riposte Finishing Blow)");
            AddBattleLine(battle, "[Mon May 27 07:30:03 2019] You have slain a cliknar battle drone!");

            // Pull, gap between prev skirmish but within next skirmish
            AddBattleLine(battle, "[Mon May 27 07:30:08 2019] You hit a cliknar mage for 5 points of magic damage by Distant Strike I. (Critical)");

            // Next skirmish/fight
            AddBattleLine(battle, "[Mon May 27 07:30:11 2019] You crush a cliknar mage for 2691 points of damage. (Riposte)");
            AddBattleLine(battle, "[Mon May 27 07:30:11 2019] A cliknar mage hits YOU for 3744 points of damage.");

            Assert.AreEqual(2, battle.Skirmishes.Count);
            var skirmish1 = battle.Skirmishes.First() as Skirmish;
            Assert.AreEqual(1, skirmish1.Fights.Count);

            var skirmish2 = battle.Skirmishes.Last() as Skirmish;
            Assert.AreEqual(1, skirmish2.Fights.Count);
        }

        [TestMethod]
        [Ignore]
        public void PullBetweenGapsOfEitherSkirmishShouldMoveToNextSkirmish()
        {
            var battle = SetupNewBattle();

            // First Skirmish/Fight
            AddBattleLine(battle, "[Mon May 27 07:30:03 2019] A cliknar battle drone bites YOU for 992 points of damage.");
            AddBattleLine(battle, "[Mon May 27 07:30:03 2019] You kick a cliknar battle drone for 163537 points of damage. (Riposte Finishing Blow)");
            AddBattleLine(battle, "[Mon May 27 07:30:03 2019] You have slain a cliknar battle drone!");

            // Pull, gap between prev skirmish but within next skirmish
            AddBattleLine(battle, "[Mon May 27 07:30:13 2019] You hit a cliknar mage for 5 points of magic damage by Distant Strike I. (Critical)");

            // Next skirmish/fight
            AddBattleLine(battle, "[Mon May 27 07:30:20 2019] You crush a cliknar mage for 2691 points of damage. (Riposte)");
            AddBattleLine(battle, "[Mon May 27 07:30:20 2019] A cliknar mage hits YOU for 3744 points of damage.");

            Assert.AreEqual(2, battle.Skirmishes.Count);
            var skirmish1 = battle.Skirmishes.First() as Skirmish;
            Assert.AreEqual(1, skirmish1.Fights.Count);

            var skirmish2 = battle.Skirmishes.Last() as Skirmish;
            Assert.AreEqual(1, skirmish2.Fights.Count);
        }

        [TestMethod]
        public void PullWithinGapOfNextSkirmishShouldStayWithNextSkirmish()
        {
            var battle = SetupNewBattle();

            // First Skirmish/Fight
            AddBattleLine(battle, "[Mon May 27 07:30:03 2019] A cliknar battle drone bites YOU for 992 points of damage.");
            AddBattleLine(battle, "[Mon May 27 07:30:03 2019] You kick a cliknar battle drone for 163537 points of damage. (Riposte Finishing Blow)");
            AddBattleLine(battle, "[Mon May 27 07:30:03 2019] You have slain a cliknar battle drone!");

            // Pull, gap between prev skirmish but within next skirmish
            AddBattleLine(battle, "[Mon May 27 07:30:13 2019] You hit a cliknar mage for 5 points of magic damage by Distant Strike I. (Critical)");

            // Next skirmish/fight
            AddBattleLine(battle, "[Mon May 27 07:30:17 2019] You crush a cliknar mage for 2691 points of damage. (Riposte)");
            AddBattleLine(battle, "[Mon May 27 07:30:17 2019] A cliknar mage hits YOU for 3744 points of damage.");

            Assert.AreEqual(2, battle.Skirmishes.Count);
            var skirmish1 = battle.Skirmishes.First() as Skirmish;
            Assert.AreEqual(1, skirmish1.Fights.Count);
            VerifySkirmishStats(skirmish1, 164529, 0, 0, 1);

            var skirmish2 = battle.Skirmishes.Last() as Skirmish;
            Assert.AreEqual(1, skirmish2.Fights.Count);
            VerifySkirmishStats(skirmish2, 6440, 0, 0, 0);
        }

        [TestMethod]
        [Ignore]
        public void RiposteAfterDeathShouldStayWithThatFight()
        {
            var battle = SetupNewBattle();

            AddBattleLine(battle, "[Mon May 27 07:30:03 2019] A cliknar battle drone bites YOU for 992 points of damage.");
            AddBattleLine(battle, "[Mon May 27 07:30:03 2019] You kick a cliknar battle drone for 163537 points of damage. (Riposte Finishing Blow)");
            AddBattleLine(battle, "[Mon May 27 07:30:03 2019] You have slain a cliknar battle drone!");
            AddBattleLine(battle, "[Mon May 27 07:30:03 2019] A cliknar battle drone tries to bite YOU, but misses! (Riposte Strikethrough)");

            Assert.AreEqual(1, battle.Skirmishes.Count);
            var skirmish = battle.Skirmishes.First() as Skirmish;
            Assert.AreEqual(1, skirmish.Fights.Count);
        }

        [TestMethod]
        public void MagicDamageByUnknownShouldGoToTheCurrentFight()
        {
            var battle = SetupNewBattle();

            AddBattleLine(battle, "[Fri Apr 26 09:26:27 2019] You hit a cliknar adept for 1180 points of chromatic damage by Lynx Maw. (Critical)");
            AddBattleLine(battle, "[Fri Apr 26 09:26:28 2019] You are covered in crystals of rime.  You have taken 10 points of damage.");
            AddBattleLine(battle, "[Fri Apr 26 09:26:28 2019] You punch a cliknar adept for 3176 points of damage. (Critical)");

            Assert.AreEqual(1, battle.Skirmishes.Count);
            var skirmish1 = battle.Skirmishes.First() as Skirmish;
            Assert.AreEqual(1, skirmish1.Fights.Count);
            VerifySkirmishStats(skirmish1, 4366, 0, 0, 0);
            VerifyFightStatistics("a cliknar adept", skirmish1, 4366, 0, 0, 0);
        }

        [TestMethod]
        public void EnsureLinesToUnknownZone()
        {
            var battle = SetupNewBattle();

            AddBattleLine(battle, "[Fri Apr 26 09:26:27 2019] You hit a cliknar adept for 1180 points of chromatic damage by Lynx Maw. (Critical)");
            AddBattleLine(battle, "[Fri Apr 26 09:26:28 2019] You are covered in crystals of rime.  You have taken 10 points of damage.");

            Assert.AreEqual(Zone.Unknown, battle.CurrentZone);
            Assert.AreEqual(1, battle.ZoneLineMap.Keys.Count);
            Assert.IsTrue(battle.ZoneLineMap.ContainsKey(Zone.Unknown));
            Assert.AreEqual(2, battle.ZoneLineMap[Zone.Unknown].Count);
        }

        [TestMethod]
        public void EnsureLinesToAppropriateZone()
        {
            var battle = SetupNewBattle();

            AddBattleLine(battle, "[Fri Apr 26 09:26:27 2019] You hit a cliknar adept for 1180 points of chromatic damage by Lynx Maw. (Critical)");
            AddBattleLine(battle, "[Fri Apr 26 09:26:28 2019] You are covered in crystals of rime.  You have taken 10 points of damage.");
            AddBattleLine(battle, "[Sun Sep 15 08:43:52 2019] You have entered Argath, Bastion of Illdaera.");
            AddBattleLine(battle, "[Sun Sep 15 09:02:28 2019] You hit living blades for 2 points of magic damage by Distant Strike I.");
            AddBattleLine(battle, "[Sun Sep 15 09:02:31 2019] Living blades is pierced by YOUR thorns for 855 points of non-melee damage.");

            Assert.AreEqual("Argath, Bastion of Illdaera", battle.CurrentZone.Name);
            Assert.AreEqual(2, battle.ZoneLineMap.Keys.Count);
            Assert.IsTrue(battle.ZoneLineMap.ContainsKey(Zone.Unknown));
            Assert.AreEqual(2, battle.ZoneLineMap[Zone.Unknown].Count);

            Assert.IsTrue(battle.ZoneLineMap.Keys.Any(z => z.Name == "Argath, Bastion of Illdaera"));
            var key = battle.ZoneLineMap.Keys.First(z => z.Name == "Argath, Bastion of Illdaera");
            Assert.AreEqual(3, battle.ZoneLineMap[key].Count);
        }

        private Battle SetupNewBattle()
        {
            return new Battle(YouAre);
        }
    }
}
