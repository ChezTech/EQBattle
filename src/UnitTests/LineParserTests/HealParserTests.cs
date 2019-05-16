﻿using BizObjects;
using LineParser.Parsers;
using LogObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LineParserTests
{
    [TestClass]
    public class HealParserTests
    {
        private HealParser _parser = new HealParser(new YouResolver("Khadaji"));

        [DataTestMethod]
        [DataRow("[Sat Mar 30 07:34:54 2019] You are bathed in a zealous light. Movanna healed you for 22144 (23333) hit points by Zealous Light.", "Movanna", "Khadaji", 22144, 23333, "Zealous Light", false, null)]
        [DataRow("[Fri Apr 26 09:26:22 2019] You are splashed by healing light. Movanna healed you for 7296 hit points by Healing Splash.", "Movanna", "Khadaji", 7296, -1, "Healing Splash", false, null)]
        [DataRow("[Fri Apr 26 09:25:53 2019] You wither under a vampiric strike. You healed Khadaji for 483 hit points by Vampiric Strike II.", "Khadaji", "Khadaji", 483, -1, "Vampiric Strike II", false, null)]
        [DataRow("[Fri Apr 26 09:25:59 2019] Khronick healed you over time for 1518 hit points by Healing Counterbias Effect.", "Khronick", "Khadaji", 1518, -1, "Healing Counterbias Effect", true, null)]
        [DataRow("[Mon Mar 25 18:42:19 2019] Velvytte feels a healing touch. Thomazz healed Velvytte for 122605 hit points by Hand of Piety XXXVII.", "Thomazz", "Velvytte", 122605, -1, "Hand of Piety XXXVII", false, null)]
        [DataRow("[Mon Mar 25 18:42:21 2019] Thomazz healed Velvytte over time for 2882 (43000) hit points by Hand of Piety XXXVII. (Critical)", "Thomazz", "Velvytte", 2882, 43000, "Hand of Piety XXXVII", true, "Critical")]
        [DataRow("[Mon Mar 25 18:51:05 2019] Tweedlede is strengthened by insistent spirits. Fincez healed Tweedlede for 5532 (5831) hit points by Wulthan Focusing Rk. II.", "Fincez", "Tweedlede", 5532, 5831, "Wulthan Focusing Rk. II", false, null)]
        [DataRow("[Mon Mar 25 19:29:35 2019] a clockwork soother CMXXI is surrounded by ardent armor. a clockwork soother CMXXI healed itself for 3594 (3821) hit points by Armor of the Ardent Rk. II.", "a clockwork soother CMXXI", "a clockwork soother CMXXI", 3594, 3821, "Armor of the Ardent Rk. II", false, null)]

        // Old school log messages
        // [DataRow("[Tue Feb 25 18:51:03 2003] You have been healed for 938 points of damage.", "hhhh", "Khadaji", 2222, -1, "sssss", false, null)]
        // [DataRow("[Fri Jun 13 23:39:32 2003] Saxstein has healed you for 1522 points of damage.", "hhhh", "pppppp", 2222, -1, "sssss", false, null)]
        // [DataRow("[Tue May 27 19:06:21 2003] Dethvegi is completely healed.", "hhhh", "pppppp", 2222, -1, "sssss", false, null)]
        // [DataRow("[Fri May 16 17:48:54 2003] A vann geistlig is completely healed.", "hhhh", "pppppp", 2222, -1, "sssss", false, null)]
        // [DataRow("xxxx", "hhhh", "pppppp", 2222, -1, "sssss", false, null)]
        public void HealTests(string logLine, string healerName, string patientName, int amount, int maxAmount, string spellName, bool isHot, string qualifier)
        {
            var logDatum = new LogDatum(logLine);

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Heal);
            var entry = lineEntry as Heal;
            Assert.AreEqual(healerName, entry.Healer.Name);
            Assert.AreEqual(patientName, entry.Patient.Name);
            Assert.AreEqual(amount, entry.Amount);
            Assert.AreEqual(maxAmount, entry.OverAmount);
            Assert.AreEqual(spellName, entry.SpellName);
            Assert.AreEqual(isHot, entry.isHealOverTime);
            Assert.AreEqual(qualifier, entry.Qualifier);
        }
    }
}