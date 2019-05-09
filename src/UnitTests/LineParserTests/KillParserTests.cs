using BizObjects;
using LineParser.Parsers;
using LogObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LineParserTests
{
    [TestClass]
    public class KillParserTests
    {
        private KillParser _parser = new KillParser();

        [TestMethod]
        public void YouKilledSomething()
        {
            var logDatum = new LogDatum("[Fri Apr 26 09:26:41 2019] You have slain a cliknar adept!");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Kill);
            var killEntry = lineEntry as Kill;
            Assert.AreEqual(Attack.You, killEntry.Attacker.Name);
            Assert.AreEqual("a cliknar adept", killEntry.Defender.Name);
            Assert.AreEqual("slain", killEntry.Verb);
            Assert.AreEqual(AttackType.Kill, killEntry.Type);
        }

        [TestMethod]
        public void YouGotKilled()
        {
            var logDatum = new LogDatum("[Fri May 16 20:23:52 2003] You have been slain by Sontalak!");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Kill);
            var killEntry = lineEntry as Kill;
            Assert.AreEqual(Attack.You, killEntry.Defender.Name);
            Assert.AreEqual("Sontalak", killEntry.Attacker.Name);
            Assert.AreEqual("slain", killEntry.Verb);
            Assert.AreEqual(AttackType.Kill, killEntry.Type);
        }

        [TestMethod]
        public void SomeoneKilledAPet()
        {
            var logDatum = new LogDatum("[Fri Apr 26 09:25:56 2019] Khadaji`s pet has been slain by a cliknar adept!");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Kill);
            var killEntry = lineEntry as Kill;
            Assert.AreEqual("Khadaji", killEntry.Defender.Name);
            Assert.IsTrue(killEntry.Defender.IsPet);
            Assert.AreEqual("a cliknar adept", killEntry.Attacker.Name);
            Assert.AreEqual("slain", killEntry.Verb);
            Assert.AreEqual(AttackType.Kill, killEntry.Type);
        }

        [TestMethod]
        public void SomeoneKilledSomething()
        {
            var logDatum = new LogDatum("[Fri Apr 26 09:25:56 2019] Movanna has been slain by a cliknar adept!");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Kill);
            var killEntry = lineEntry as Kill;
            Assert.AreEqual("Movanna", killEntry.Defender.Name);
            Assert.AreEqual("a cliknar adept", killEntry.Attacker.Name);
            Assert.AreEqual("slain", killEntry.Verb);
            Assert.AreEqual(AttackType.Kill, killEntry.Type);
        }

        [TestMethod]
        public void SomethingDied()
        {
            var logDatum = new LogDatum("[Sat May 17 17:46:01 2003] A Razorfiend Subduer died.");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Kill);
            var killEntry = lineEntry as Kill;
            Assert.AreEqual("Unknown", killEntry.Attacker.Name);
            Assert.AreEqual("a Razorfiend Subduer", killEntry.Defender.Name);
            Assert.AreEqual("died", killEntry.Verb);
            Assert.AreEqual(AttackType.Kill, killEntry.Type);
        }
    }
}
