using BizObjects.Converters;
using BizObjects.Lines;
using LineParser.Parsers;
using LogObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LineParserTests
{
    [TestClass]
    public class KillParserTests
    {
        private KillParser _parser = new KillParser(new YouResolver("Khadaji"));

        [DataTestMethod]
        [DataRow("[Fri Apr 26 09:26:41 2019] You have slain a cliknar adept!", "Khadaji", "a cliknar adept", "slain", AttackType.Kill, false)]
        [DataRow("[Fri May 16 20:23:52 2003] You have been slain by Sontalak!", "Sontalak", "Khadaji", "slain", AttackType.Kill, false)]
        [DataRow("[Fri Apr 26 09:25:56 2019] Khadaji`s pet has been slain by a cliknar adept!", "a cliknar adept", "Khadaji", "slain", AttackType.Kill, true)]
        [DataRow("[Fri Apr 26 09:25:56 2019] Movanna has been slain by a cliknar adept!", "a cliknar adept", "Movanna", "slain", AttackType.Kill, false)]
        [DataRow("[Sat May 17 17:46:01 2003] A Razorfiend Subduer died.", "Unknown", "a Razorfiend Subduer", "died", AttackType.Kill, false)]
        [DataRow("[Fri May 24 18:25:03 2019] A cliknar centurion has been slain by Vatalae!", "Vatalae", "a cliknar centurion", "slain", AttackType.Kill, false)]
        [DataRow("[Sun Sep 20 11:05:55 2020] You died.", "Unknown", "Khadaji", "died", AttackType.Kill, false)]
        public void Tests(string logLine, string attacker, string defender, string verb, AttackType attackType, bool isDefenderPet = false)
        {
            var logDatum = new LogDatum(logLine);

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result, logLine, string.Format("Failing line: {0}", logLine));
            var entry = lineEntry as Kill;
            Assert.IsNotNull(entry, string.Format("Failing line: {0}", logLine));
            Assert.AreEqual(attacker, entry.Attacker.Name, string.Format("Failing line: {0}", logLine));
            Assert.AreEqual(isDefenderPet, entry.Defender.IsPet, string.Format("Failing line: {0}", logLine));
            Assert.AreEqual(defender, entry.Defender.Name, string.Format("Failing line: {0}", logLine));
            Assert.AreEqual(verb, entry.Verb, string.Format("Failing line: {0}", logLine));
            Assert.AreEqual(attackType, entry.Type, string.Format("Failing line: {0}", logLine));
        }
    }
}
