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
            var logDatum = new LogDatum("[Thu Apr 04 22:16:14 2019] You try to crush a gnome servant, but miss! (Strikethrough)");

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

        [TestMethod]
        public void MonsterMissesYouSimple()
        {
            var logDatum = new LogDatum("[Fri Apr 12 18:23:26 2019] A lavakin tries to hit YOU, but misses!");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Miss);
            var entry = lineEntry as Miss;
            Assert.AreEqual("a lavakin", entry.Attacker.Name);
            Assert.AreEqual(Attack.You, entry.Defender.Name);
            Assert.AreEqual("hit", entry.Verb);
            Assert.AreEqual(AttackType.Hit, entry.Type);
            Assert.AreEqual("misses", entry.DefenseType);
            Assert.IsNull(entry.Qualifier);
        }

        [TestMethod]
        public void MonsterMissYouDefended()
        {
            var logDatum = new LogDatum("[Fri Apr 12 18:23:31 2019] A lavakin tries to hit YOU, but YOU riposte!");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Miss);
            var entry = lineEntry as Miss;
            Assert.AreEqual("a lavakin", entry.Attacker.Name);
            Assert.AreEqual(Attack.You, entry.Defender.Name);
            Assert.AreEqual("hit", entry.Verb);
            Assert.AreEqual(AttackType.Hit, entry.Type);
            Assert.AreEqual("riposte", entry.DefenseType);
            Assert.IsNull(entry.Qualifier);
        }

        [TestMethod]
        public void MonsterMissesYouWithQualifier()
        {
            var logDatum = new LogDatum("[Sat Mar 30 09:21:46 2019] A genati tries to smash YOU, but misses! (Strikethrough)");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Miss);
            var entry = lineEntry as Miss;
            Assert.AreEqual("a genati", entry.Attacker.Name);
            Assert.AreEqual(Attack.You, entry.Defender.Name);
            Assert.AreEqual("smash", entry.Verb);
            Assert.AreEqual(AttackType.Smash, entry.Type);
            Assert.AreEqual("misses", entry.DefenseType);
            Assert.AreEqual("Strikethrough", entry.Qualifier);
        }

        [TestMethod]
        public void MonsterMissesYouAbsorbedWithQualifier()
        {
            var logDatum = new LogDatum("[Fri Apr 12 18:23:28 2019] A lavakin tries to hit YOU, but YOUR magical skin absorbs the blow! (Strikethrough)");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Miss);
            var entry = lineEntry as Miss;
            Assert.AreEqual("a lavakin", entry.Attacker.Name);
            Assert.AreEqual(Attack.You, entry.Defender.Name);
            Assert.AreEqual("hit", entry.Verb);
            Assert.AreEqual(AttackType.Hit, entry.Type);
            Assert.AreEqual("magical skin absorbs the blow", entry.DefenseType);
            Assert.AreEqual("Strikethrough", entry.Qualifier);
        }

        [TestMethod]
        public void OtherMissesOtherDefended()
        {
            var logDatum = new LogDatum("[Fri Apr 12 18:26:38 2019] Khadaji`s pet tries to hit a crystalkin disciple, but a crystalkin disciple parries!");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Miss);
            var entry = lineEntry as Miss;
            Assert.AreEqual("Khadaji", entry.Attacker.Name);
            Assert.IsTrue(entry.Attacker.IsPet);
            Assert.AreEqual("a crystalkin disciple", entry.Defender.Name);
            Assert.AreEqual("hit", entry.Verb);
            Assert.AreEqual(AttackType.Hit, entry.Type);
            Assert.AreEqual("parries", entry.DefenseType);
            Assert.IsNull(entry.Qualifier);
        }

        [TestMethod]
        public void OtherMissesOtherSimple()
        {
            var logDatum = new LogDatum("[Fri Apr 12 18:26:39 2019] Khadaji`s pet tries to hit a crystalkin disciple, but misses!");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Miss);
            var entry = lineEntry as Miss;
            Assert.AreEqual("Khadaji", entry.Attacker.Name);
            Assert.IsTrue(entry.Attacker.IsPet);
            Assert.AreEqual("a crystalkin disciple", entry.Defender.Name);
            Assert.AreEqual("hit", entry.Verb);
            Assert.AreEqual(AttackType.Hit, entry.Type);
            Assert.AreEqual("misses", entry.DefenseType);
            Assert.IsNull(entry.Qualifier);
        }

        [TestMethod]
        public void OtherMissesOtherWithQualifier()
        {
            var logDatum = new LogDatum("[Sat Mar 30 08:14:42 2019] A master hunter tries to pierce Movanna, but misses! (Rampage)");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Miss);
            var entry = lineEntry as Miss;
            Assert.AreEqual("a master hunter", entry.Attacker.Name);
            Assert.AreEqual("Movanna", entry.Defender.Name);
            Assert.AreEqual("pierce", entry.Verb);
            Assert.AreEqual(AttackType.Pierce, entry.Type);
            Assert.AreEqual("misses", entry.DefenseType);
            Assert.AreEqual("Rampage", entry.Qualifier);
        }

        [TestMethod]
        public void OtherMissesOtherDefendedWithQualifier()
        {
            var logDatum = new LogDatum("[Sat Mar 30 08:15:44 2019] A master hunter tries to pierce Movanna, but Movanna parries! (Rampage)");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Miss);
            var entry = lineEntry as Miss;
            Assert.AreEqual("a master hunter", entry.Attacker.Name);
            Assert.AreEqual("Movanna", entry.Defender.Name);
            Assert.AreEqual("pierce", entry.Verb);
            Assert.AreEqual(AttackType.Pierce, entry.Type);
            Assert.AreEqual("parries", entry.DefenseType);
            Assert.AreEqual("Rampage", entry.Qualifier);
        }
    }
}
