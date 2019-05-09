using BizObjects;
using LineParser.Parsers;
using LogObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BizObjectsTests
{
    [TestClass]
    public class MissParserTests
    {
        private MissParser _parser = new MissParser();

        [TestMethod]
        public void YouMissSimple()
        {
            var logDatum = new LogDatum("[Thu Apr 04 22:11:57 2019] You try to crush a gnome servant, but miss!");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Miss);
            var entry = lineEntry as Miss;
            Assert.AreEqual(Attack.You, entry.Attacker.Name);
            Assert.AreEqual("a gnome servant", entry.Defender.Name);
            Assert.AreEqual("crush", entry.Verb);
            Assert.AreEqual(AttackType.Crush, entry.Type);
            Assert.AreEqual("miss", entry.DefenseType);
            Assert.IsNull(entry.Qualifier);
        }
    }
}
