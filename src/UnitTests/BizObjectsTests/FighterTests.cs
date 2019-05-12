using BizObjects;
using LineParser.Parsers;
using LogObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BizObjectsTests
{
    [TestClass]
    public class FighterTests
    {
        private HitParser _hitParser = new HitParser();

        [TestMethod]
        public void CreateFighterWithCharacter()
        {
            var pc = new Character("Khadaji");
            var fighter = new Fighter(pc);

            Assert.IsNotNull(fighter);
            Assert.AreEqual("Khadaji", fighter.Character.Name);
        }

        [TestMethod]
        public void FighterDamaage()
        {
            var pc = new Character("You");
            var fighter = new Fighter(pc);

            _hitParser.TryParse(new LogDatum("[Fri Apr 26 09:25:44 2019] You kick a cliknar adept for 2894 points of damage."), out ILine line);
            fighter.AddHit(line as Hit);

            _hitParser.TryParse(new LogDatum("[Fri Apr 26 09:25:44 2019] You strike a cliknar adept for 601 points of damage. (Strikethrough)"), out line);
            fighter.AddHit(line as Hit);

            Assert.AreEqual(3495, fighter.GetCurrentDamageDealtByCharacter());
        }
    }
}
