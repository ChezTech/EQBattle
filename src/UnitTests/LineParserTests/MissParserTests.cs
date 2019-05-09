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

        [TestMethod]
        public void YouMissWithQualifier()
        {
            var logDatum = new LogDatum("[Thu Apr 04 22:16:14 2019] You try to crush a gnome servant, but miss! (Strikethrough) ");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Miss);
            var entry = lineEntry as Miss;
            Assert.AreEqual(Attack.You, entry.Attacker.Name);
            Assert.AreEqual("a gnome servant", entry.Defender.Name);
            Assert.AreEqual("crush", entry.Verb);
            Assert.AreEqual(AttackType.Crush, entry.Type);
            Assert.AreEqual("miss", entry.DefenseType);
            Assert.AreEqual("Strikethrough", entry.Qualifier);
        }

        [TestMethod]
        public void YouMissWithMultipleQualifiers()
        {
            var logDatum = new LogDatum("[Thu Apr 04 22:23:23 2019] You try to crush a bellikos recruiter, but miss! (Riposte Strikethrough)");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Miss);
            var entry = lineEntry as Miss;
            Assert.AreEqual(Attack.You, entry.Attacker.Name);
            Assert.AreEqual("a bellikos recruiter", entry.Defender.Name);
            Assert.AreEqual("crush", entry.Verb);
            Assert.AreEqual(AttackType.Crush, entry.Type);
            Assert.AreEqual("miss", entry.DefenseType);
            Assert.AreEqual("Riposte Strikethrough", entry.Qualifier);
        }

        [TestMethod]
        public void YouMissButDefended()
        {
            var logDatum = new LogDatum("[Thu Apr 04 22:24:02 2019] You try to strike a gnome servant, but a gnome servant dodges!");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Miss);
            var entry = lineEntry as Miss;
            Assert.AreEqual(Attack.You, entry.Attacker.Name);
            Assert.AreEqual("a gnome servant", entry.Defender.Name);
            Assert.AreEqual("strike", entry.Verb);
            Assert.AreEqual(AttackType.Strike, entry.Type);
            Assert.AreEqual("dodges", entry.DefenseType);
            Assert.IsNull(entry.Qualifier);
        }

        [TestMethod]
        public void YouMissSkinAbsorbs()
        {
            var logDatum = new LogDatum("[Fri Apr 05 15:12:53 2019] You try to kick a telmira disciple, but a telmira disciple's magical skin absorbs the blow!");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Miss);
            var entry = lineEntry as Miss;
            Assert.AreEqual(Attack.You, entry.Attacker.Name);
            Assert.AreEqual("a telmira disciple", entry.Defender.Name);
            Assert.AreEqual("kick", entry.Verb);
            Assert.AreEqual(AttackType.Kick, entry.Type);
            Assert.AreEqual("magical skin absorbs the blow", entry.DefenseType);
            Assert.IsNull(entry.Qualifier);
        }
    }
}
