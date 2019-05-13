using BizObjects;
using LineParser.Parsers;
using LogObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LineParserTests
{
    [TestClass]
    public class HealParserTests
    {
        private HealParser _parser = new HealParser(new YouResolver("Khadaji"));

        [TestMethod]
        public void YouGotHealedMaxed()
        {
            var logDatum = new LogDatum("[Sat Mar 30 07:34:54 2019] You are bathed in a zealous light. Movanna healed you for 22144 (23333) hit points by Zealous Light.");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Heal);
            var entry = lineEntry as Heal;
            Assert.AreEqual("Movanna", entry.Healer.Name);
            Assert.AreEqual("Khadaji", entry.Patient.Name);
            Assert.AreEqual(22144, entry.Amount);
            Assert.AreEqual(23333, entry.MaxAmount);
            Assert.AreEqual("Zealous Light", entry.SpellName);
            Assert.IsFalse(entry.isHealOverTime);
            Assert.IsNull(entry.Qualifier);
        }

        [TestMethod]
        public void YouAreHealed()
        {
            var logDatum = new LogDatum("[Fri Apr 26 09:26:22 2019] You are splashed by healing light. Movanna healed you for 7296 hit points by Healing Splash.");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Heal);
            var entry = lineEntry as Heal;
            Assert.AreEqual("Movanna", entry.Healer.Name);
            Assert.AreEqual("Khadaji", entry.Patient.Name);
            Assert.AreEqual(7296, entry.Amount);
            Assert.AreEqual(-1, entry.MaxAmount);
            Assert.AreEqual("Healing Splash", entry.SpellName);
            Assert.IsFalse(entry.isHealOverTime);
            Assert.IsNull(entry.Qualifier);
        }

        [TestMethod]
        public void YouHealYourselfKinda()
        {
            var logDatum = new LogDatum("[Fri Apr 26 09:25:53 2019] You wither under a vampiric strike. You healed Khadaji for 483 hit points by Vampiric Strike II.");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Heal);
            var entry = lineEntry as Heal;
            Assert.AreEqual("Khadaji", entry.Healer.Name);
            Assert.AreEqual("Khadaji", entry.Patient.Name);
            Assert.AreEqual(483, entry.Amount);
            Assert.AreEqual(-1, entry.MaxAmount);
            Assert.AreEqual("Vampiric Strike II", entry.SpellName);
            Assert.IsFalse(entry.isHealOverTime);
            Assert.IsNull(entry.Qualifier);
        }

        [TestMethod]
        public void YouGotHotted()
        {
            var logDatum = new LogDatum("[Fri Apr 26 09:25:59 2019] Khronick healed you over time for 1518 hit points by Healing Counterbias Effect.");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Heal);
            var entry = lineEntry as Heal;
            Assert.AreEqual("Khronick", entry.Healer.Name);
            Assert.AreEqual("Khadaji", entry.Patient.Name);
            Assert.AreEqual(1518, entry.Amount);
            Assert.AreEqual(-1, entry.MaxAmount);
            Assert.AreEqual("Healing Counterbias Effect", entry.SpellName);
            Assert.IsTrue(entry.isHealOverTime);
            Assert.IsNull(entry.Qualifier);
        }

        [TestMethod]
        public void HealerHealedPatient()
        {
            var logDatum = new LogDatum("[Mon Mar 25 18:42:19 2019] Velvytte feels a healing touch. Thomazz healed Velvytte for 122605 hit points by Hand of Piety XXXVII.");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Heal);
            var entry = lineEntry as Heal;
            Assert.AreEqual("Thomazz", entry.Healer.Name);
            Assert.AreEqual("Velvytte", entry.Patient.Name);
            Assert.AreEqual(122605, entry.Amount);
            Assert.AreEqual(-1, entry.MaxAmount);
            Assert.AreEqual("Hand of Piety XXXVII", entry.SpellName);
            Assert.IsFalse(entry.isHealOverTime);
            Assert.IsNull(entry.Qualifier);
        }

        [TestMethod]
        public void HealerHottedPatientCritical()
        {
            var logDatum = new LogDatum("[Mon Mar 25 18:42:21 2019] Thomazz healed Velvytte over time for 2882 (43000) hit points by Hand of Piety XXXVII. (Critical)");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Heal);
            var entry = lineEntry as Heal;
            Assert.AreEqual("Thomazz", entry.Healer.Name);
            Assert.AreEqual("Velvytte", entry.Patient.Name);
            Assert.AreEqual(2882, entry.Amount);
            Assert.AreEqual(43000, entry.MaxAmount);
            Assert.AreEqual("Hand of Piety XXXVII", entry.SpellName);
            Assert.IsTrue(entry.isHealOverTime);
            Assert.AreEqual("Critical", entry.Qualifier);
        }

        [TestMethod]
        public void HealerHealedPatientMaxed()
        {
            var logDatum = new LogDatum("[Mon Mar 25 18:51:05 2019] Tweedlede is strengthened by insistent spirits. Fincez healed Tweedlede for 5532 (5831) hit points by Wulthan Focusing Rk. II.");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Heal);
            var entry = lineEntry as Heal;
            Assert.AreEqual("Fincez", entry.Healer.Name);
            Assert.AreEqual("Tweedlede", entry.Patient.Name);
            Assert.AreEqual(5532, entry.Amount);
            Assert.AreEqual(5831, entry.MaxAmount);
            Assert.AreEqual("Wulthan Focusing Rk. II", entry.SpellName);
            Assert.IsFalse(entry.isHealOverTime);
            Assert.IsNull(entry.Qualifier);
        }

        [TestMethod]
        public void MercHealedSelf()
        {
            var logDatum = new LogDatum("[Mon Mar 25 19:29:35 2019] a clockwork soother CMXXI is surrounded by ardent armor. a clockwork soother CMXXI healed itself for 3594 (3821) hit points by Armor of the Ardent Rk. II.");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Heal);
            var entry = lineEntry as Heal;
            Assert.AreEqual("a clockwork soother CMXXI", entry.Healer.Name);
            Assert.AreEqual("a clockwork soother CMXXI", entry.Patient.Name);
            Assert.AreEqual(3594, entry.Amount);
            Assert.AreEqual(3821, entry.MaxAmount);
            Assert.AreEqual("Armor of the Ardent Rk. II", entry.SpellName);
            Assert.IsFalse(entry.isHealOverTime);
            Assert.IsNull(entry.Qualifier);
        }
    }
}
