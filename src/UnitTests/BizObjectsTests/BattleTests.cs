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
    public class BattleTests : ParserTestBase
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

        [TestMethod]
        public void SlainDuringFightRezdRightAfter()
        {
            var battle = SetupNewBattle();

            AddBattleLine(battle, "[Sun Mar 22 13:48:34 2020] You crush a dirty oashim for 2642 points of damage. (Strikethrough Critical)");
            AddBattleLine(battle, "[Sun Mar 22 13:48:34 2020] A dirty oashim bites YOU for 1935 points of damage.");
            AddBattleLine(battle, "[Sun Mar 22 13:48:34 2020] A dirty oashim quickly moves in to feast upon your body.");
            AddBattleLine(battle, "[Sun Mar 22 13:48:34 2020] You have been knocked unconscious!");
            AddBattleLine(battle, "[Sun Mar 22 13:48:34 2020] You have been slain by a dirty oashim!");

            AddBattleLine(battle, "[Sun Mar 22 13:48:35 2020] A dirty oashim tries to bite Khronick, but Khronick blocks!");
            AddBattleLine(battle, "[Sun Mar 22 13:48:35 2020] A dirty oashim bites Khronick for 9129 points of damage.");
            AddBattleLine(battle, "[Sun Mar 22 13:48:36 2020] Kelanna pierces a dirty oashim for 1068 points of damage.");
            AddBattleLine(battle, "[Sun Mar 22 13:48:36 2020] A dirty oashim's corpse squeals in panic as the life leaves its body.");
            AddBattleLine(battle, "[Sun Mar 22 13:48:36 2020] A dirty oashim has been slain by Kelanna!");

            AddBattleLine(battle, "[Sun Mar 22 13:48:39 2020] Kiuchelle begins casting Reviviscence.");
            AddBattleLine(battle, "[Sun Mar 22 13:48:43 2020] You have been offered a resurrection.");
            AddBattleLine(battle, "[Sun Mar 22 13:48:46 2020] You regain some experience from resurrection.");
            AddBattleLine(battle, "[Sun Mar 22 13:48:46 2020] Returning to Resurrect.Please wait...");
            AddBattleLine(battle, "[Sun Mar 22 13:48:48 2020] Kiuchelle healed you for 4798 (13975) hit points by Rejuvenating Splash Rk.II.");

            Assert.AreEqual(1, battle.Skirmishes.Count);

            var skirmish1 = battle.Skirmishes.First() as Skirmish;
            VerifySkirmishStats(skirmish1, 14774, 4798, 1, 2);

            Assert.AreEqual(1, skirmish1.Fights.Count);
            VerifyFightStatistics("a dirty oashim", skirmish1, 14774, 4798, 1, 2);
        }

        [TestMethod]
        public void PartyWipeReturnToPoKChatShouldGoToNewFight()
        {
            var battle = SetupNewBattle();

            AddBattleLine(battle, "[Sun Mar 22 14:00:10 2020] An adult oashim bites YOU for 9713 points of damage. (Strikethrough)");
            AddBattleLine(battle, "[Sun Mar 22 14:00:10 2020] An adult oashim tries to bite YOU, but misses!");
            AddBattleLine(battle, "[Sun Mar 22 14:00:10 2020] You have been knocked unconscious!");
            AddBattleLine(battle, "[Sun Mar 22 14:00:10 2020] Kelanna pierces a dirty oashim for 1068 points of damage.");
            AddBattleLine(battle, "[Sun Mar 22 14:00:10 2020] A dirty oashim bites YOU for 7552 points of damage. (Strikethrough)");
            AddBattleLine(battle, "[Sun Mar 22 14:00:10 2020] A dirty oashim quickly moves in to feast upon your body.");
            AddBattleLine(battle, "[Sun Mar 22 14:00:10 2020] You have been slain by a dirty oashim!");

            AddBattleLine(battle, "[Sun Mar 22 14:00:11 2020] An adult oashim bites Kelanna for 1759 points of damage.");
            AddBattleLine(battle, "[Sun Mar 22 14:00:12 2020] A dirty oashim tries to bite Kelanna, but Kelanna dodges!");
            AddBattleLine(battle, "[Sun Mar 22 14:00:12 2020] A dirty oashim bites Khronick for 3298 points of damage.");
            AddBattleLine(battle, "[Sun Mar 22 14:00:12 2020] A dirty oashim tries to bite Khronick, but Khronick dodges!");
            AddBattleLine(battle, "[Sun Mar 22 14:00:12 2020] Kelanna backstabs a dirty oashim for 3345 points of damage.");
            AddBattleLine(battle, "[Sun Mar 22 14:00:13 2020] An adult oashim bites Khronick for 5985 points of damage.");
            AddBattleLine(battle, "[Sun Mar 22 14:00:13 2020] An adult oashim quickly moves in to feast upon your body.");
            AddBattleLine(battle, "[Sun Mar 22 14:00:13 2020] Khronick has been slain by an adult oashim!");

            AddBattleLine(battle, "[Sun Mar 22 14:00:14 2020] An adult oashim bites Kelanna for 1759 points of damage.");
            AddBattleLine(battle, "[Sun Mar 22 14:00:14 2020] An adult oashim quickly moves in to feast upon your body.");
            AddBattleLine(battle, "[Sun Mar 22 14:00:14 2020] Kelanna has been slain by an adult oashim!");

            AddBattleLine(battle, "[Sun Mar 22 14:01:00 2020] Returning to Bind Location. Please wait...");
            AddBattleLine(battle, "[Sun Mar 22 14:01:01 2020] LOADING, PLEASE WAIT...");
            AddBattleLine(battle, "[Sun Mar 22 14:01:03 2020] MESSAGE OF THE DAY: ");
            AddBattleLine(battle, "[Sun Mar 22 14:01:20 2020] You have entered Guild Lobby.");
            AddBattleLine(battle, "[Sun Mar 22 14:01:24 2020] Kiuchelle begins casting Hand of Credence Rk. II.");
            AddBattleLine(battle, "[Sun Mar 22 14:01:24 2020] Channels: 1=General(395), 2=Monk(19)");
            AddBattleLine(battle, "[Sun Mar 22 14:01:24 2020] Channels: 1=General(395), 2=Monk(19), 3=Planes(170)");
            AddBattleLine(battle, "[Sun Mar 22 14:01:30 2020] You are filled with a powerful credence.");
            AddBattleLine(battle, "[Sun Mar 22 14:01:30 2020] Kiuchelle healed you for 6198 hit points by Hand of Credence Rk. II.");

            AddBattleLine(battle, "[Sun Mar 22 14:03:00 2020] You begin casting Expedient Recovery.");
            AddBattleLine(battle, "[Sun Mar 22 14:03:03 2020] You call for your bodies to return to you.");
            AddBattleLine(battle, "[Sun Mar 22 14:03:04 2020] You regain some experience from resurrection.");

            AddBattleLine(battle, "[Sun Mar 22 14:04:00 2020] Celerese's eyes gleam with gallantry.");
            AddBattleLine(battle, "[Sun Mar 22 14:04:00 2020] Ruhurte healed Celerese for 100 (3700) hit points by Hand of Gallantry.");
            AddBattleLine(battle, "[Sun Mar 22 14:04:00 2020] Ruhurte's eyes gleam with gallantry.");
            AddBattleLine(battle, "[Sun Mar 22 14:04:00 2020] Ruhurte healed himself for 3194 (3700) hit points by Hand of Gallantry.");
            AddBattleLine(battle, "[Sun Mar 22 14:04:00 2020] Utishelle begins casting Word of Greater Replenishment Rk. II.");
            AddBattleLine(battle, "[Sun Mar 22 14:04:00 2020] Utishelle feels a powerful healing touch.");
            AddBattleLine(battle, "[Sun Mar 22 14:04:00 2020] Utishelle healed herself for 54999 (95812) hit points by Word of Greater Replenishment Rk. II.");
            AddBattleLine(battle, "[Sun Mar 22 14:04:00 2020] Bulbeye feels a powerful healing touch.");
            AddBattleLine(battle, "[Sun Mar 22 14:04:00 2020] Utishelle healed Bulbeye for 13038 (95812) hit points by Word of Greater Replenishment Rk. II.");


            Assert.AreEqual(2, battle.Skirmishes.Count);

            var skirmish1 = battle.Skirmishes.First() as Skirmish;
            VerifySkirmishStats(skirmish1, 14774, 4798, 1, 2);

            Assert.AreEqual(2, skirmish1.Fights.Count);
            VerifyFightStatistics("a dirty oashim", skirmish1, 5, 0, 1, 2);
            VerifyFightStatistics("an adult oashim", skirmish1, 5, 0, 1, 2);

            // Going to a new zone (bind point) .... should end the skirmish
            // Open Q: how will rez'ing back to the zone pick back up the skirmish? Should it make a new, new skirmish
            // How will dying in the same zone as your bind point affect this?

            var skirmish2 = battle.Skirmishes.Last() as Skirmish;
            VerifySkirmishStats(skirmish2, 5649, 0, 0, 0);

            Assert.AreEqual(1, skirmish2.Fights.Count);

        }

        [TestMethod]
        public void DoTAfterMobDeathShouldGoToOldMob()
        {
            var battle = SetupNewBattle();

            AddBattleLine(battle, "[Sun Apr 12 15:33:08 2020] A moss viper bites YOU for 23 points of damage.");
            AddBattleLine(battle, "[Sun Apr 12 15:33:10 2020] You crush a moss viper for 44 points of damage.");
            AddBattleLine(battle, "[Sun Apr 12 15:33:10 2020] a moss viper hit you for 30 points of poison damage by Strong Poison.");
            AddBattleLine(battle, "[Sun Apr 12 15:33:10 2020] You have been poisoned.");
            AddBattleLine(battle, "[Sun Apr 12 15:33:12 2020] A moss viper has taken 11 damage from your Engulfing Darkness.");
            AddBattleLine(battle, "[Sun Apr 12 15:33:15 2020] You have taken 14 damage from Strong Poison by a moss viper.");
            AddBattleLine(battle, "[Sun Apr 12 15:33:18 2020] A moss viper has taken 11 damage from your Engulfing Darkness.");
            AddBattleLine(battle, "[Sun Apr 12 15:33:19 2020] You try to crush a moss viper, but a moss viper dodges!");
            AddBattleLine(battle, "[Sun Apr 12 15:33:23 2020] You crush a moss viper for 45 points of damage.");
            AddBattleLine(battle, "[Sun Apr 12 15:33:23 2020] You gain experience (with a bonus)!");
            AddBattleLine(battle, "[Sun Apr 12 15:33:23 2020] A moss viper's corpse falls.");
            AddBattleLine(battle, "[Sun Apr 12 15:33:23 2020] You have slain a moss viper!");
            AddBattleLine(battle, "[Sun Apr 12 15:33:27 2020] You have taken 14 damage from Strong Poison by a moss viper's corpse.");
            AddBattleLine(battle, "[Sun Apr 12 15:33:33 2020] You have taken 14 damage from Strong Poison by a moss viper's corpse.");

            Assert.AreEqual(1, battle.Skirmishes.Count);

            var skirmish1 = battle.Skirmishes.First() as Skirmish;
            VerifySkirmishStats(skirmish1, -1, -1, 1, 1);

            Assert.AreEqual(1, skirmish1.Fights.Count);
            VerifyFightStatistics("a moss viper", skirmish1, 5, 0, 1, 2);

        }

        private void VerifySkirmishStats(Skirmish skirmish, int hit, int heal, int misses, int kills)
        {
            var stats = skirmish.Statistics;
            Assert.AreEqual(hit, stats.Hit.Total, $"Offensive hit");
            Assert.AreEqual(heal, stats.Heal.Total, $"Offensive heal");
            Assert.AreEqual(misses, stats.Miss.Count, $"Offensive misses");
            Assert.AreEqual(kills, stats.Kill.Count, $"Offensive kills");
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

            var stats = fight.Statistics;
            Assert.AreEqual(hit, stats.Hit.Total, $"Offensive hit - {fight.PrimaryMob.Name}");
            Assert.AreEqual(heal, stats.Heal.Total, $"Offensive heal - {fight.PrimaryMob.Name}");
            Assert.AreEqual(misses, stats.Miss.Count, $"Offensive misses - {fight.PrimaryMob.Name}");
            Assert.AreEqual(kills, stats.Kill.Count, $"Offensive kills - {fight.PrimaryMob.Name}");
        }

        private Battle SetupNewBattle()
        {
            return new Battle(YouAre);
        }
    }
}
