using BizObjects;
using LineParser.Parsers;
using LogObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BizObjectsTests
{
    [TestClass]
    public class FighterTests
    {
        private static readonly YouResolver YouAre = new YouResolver("Khadaji");
        private HitParser _hitParser = new HitParser(YouAre);

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

            _hitParser.TryParse(new LogDatum("[Fri Apr 26 09:25:44 2019] You kick a cliknar adept for 2894 points of damage."), out ILine line);
            fighter.AddOffense(line);

            _hitParser.TryParse(new LogDatum("[Fri Apr 26 09:25:44 2019] You strike a cliknar adept for 601 points of damage. (Strikethrough)"), out line);
            fighter.AddOffense(line);

            _hitParser.TryParse(new LogDatum("[Fri Apr 26 09:25:50 2019] You punch a cliknar adept for 1092 points of damage."), out line);
            fighter.AddOffense(line);

            Assert.AreEqual(4587, fighter.OffensiveStatistics.Hit.Total);
            Assert.AreEqual(601, fighter.OffensiveStatistics.Hit.Min);
            Assert.AreEqual(2894, fighter.OffensiveStatistics.Hit.Max);
        }
    }
}
