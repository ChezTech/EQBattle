using System;
using BizObjects;
using LineParser;
using LineParser.Parsers;
using LogObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BizObjectsTests
{
    [TestClass]
    public class FighterTests : ParserTestBase
    {
        private readonly Func<string, ILine> ParseLine = logLine => _parser.ParseLine(new LogDatum(logLine));

        [TestMethod]
        public void CreateFighterWithCharacter()
        {
            var pc = new Character(YouAre.Name);
            var fighter = new Fighter(pc);

            Assert.IsNotNull(fighter);
            Assert.AreEqual("Khadaji", fighter.Character.Name);
        }

        [TestMethod]
        public void FighterDamaage()
        {
            var pc = new Character(YouAre.Name);
            var fighter = new Fighter(pc);

            fighter.AddOffense(ParseLine("[Fri Apr 26 09:25:44 2019] You kick a cliknar adept for 2894 points of damage."));
            fighter.AddOffense(ParseLine("[Fri Apr 26 09:25:44 2019] You strike a cliknar adept for 601 points of damage. (Strikethrough)"));
            fighter.AddOffense(ParseLine("[Fri Apr 26 09:25:50 2019] You punch a cliknar adept for 1092 points of damage."));

            Assert.AreEqual(4587, fighter.OffensiveStatistics.Hit.Total);
            Assert.AreEqual(601, fighter.OffensiveStatistics.Hit.Min);
            Assert.AreEqual(2894, fighter.OffensiveStatistics.Hit.Max);
        }

        [TestMethod]
        public void EnsureFighterIsDead()
        {
            var npc = new Character("A gnome disciple");
            var fighter = new Fighter(npc);

            fighter.AddDefense(ParseLine("[Fri Apr 12 18:32:36 2019] You kick a gnome disciple for 75472 points of damage. (Finishing Blow)"));
            fighter.AddDefense(ParseLine("[Fri Apr 12 18:32:36 2019] You have slain a gnome disciple!"));

            Assert.IsTrue(fighter.IsDead);
        }

        [TestMethod]
        public void YouDied()
        {
            var pc = new Character(YouAre.Name);
            var fighter = new Fighter(pc);

            fighter.AddDefense(ParseLine("[Fri Apr 05 17:13:24 2019] You have been slain by an enraged disciple!"));

            Assert.IsTrue(fighter.IsDead);
        }

        [TestMethod]
        public void SomeoneDied()
        {
            var pc = new Character("Movanna");
            var fighter = new Fighter(pc);

            fighter.AddDefense(ParseLine("[Fri Apr 05 16:24:05 2019] Movanna has been slain by a dwarf disciple!"));

            Assert.IsTrue(fighter.IsDead);
        }

        [TestMethod]
        public void YouKilledMob()
        {
            var pc = new Character(YouAre.Name);
            var fighter = new Fighter(pc);

            fighter.AddOffense(ParseLine("[Fri Apr 05 16:43:42 2019] You have slain a dwarf disciple!"));

            Assert.IsFalse(fighter.IsDead);
        }

        [TestMethod]
        [Ignore]
        public void MultiplePetsAttackingAndDying()
        {
            var pc = new Character("Khadaji`s pet");
            var fighter = new Fighter(pc);

            fighter.AddDefense(ParseLine("[Fri Apr 05 16:56:33 2019] A bellikos disciple slashes Khadaji`s pet for 2332 points of damage. (Riposte)"));
            fighter.AddDefense(ParseLine("[Fri Apr 05 16:56:33 2019] Khadaji`s pet has been slain by a bellikos disciple!"));

            Assert.IsTrue(fighter.IsDead);

            fighter.AddOffense(ParseLine("[Fri Apr 05 16:56:33 2019] Khadaji`s pet tries to hit a bellikos disciple, but a bellikos disciple ripostes!"));

            Assert.IsFalse(fighter.IsDead);
        }
    }
}
