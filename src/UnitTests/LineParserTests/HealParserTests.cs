using BizObjects.Converters;
using BizObjects.Lines;
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
        [DataRow("[Mon Mar 25 19:52:35 2019] Jaminai feels the touch of resurgence. Jaminai healed herself for 1422 (8738) hit points by Word of Resurgence Rk. II.", "Jaminai", "Jaminai", 1422, 8738, "Word of Resurgence Rk. II", false, null)]
        [DataRow("[Mon Mar 25 19:52:36 2019] Buddahbean`s warder feels the touch of vivification. Seissyll healed Buddahbean`s warder for 903 (4137) hit points by Word of Vivification.", "Seissyll", "Buddahbean", 903, 4137, "Word of Vivification", false, null)]
        [DataRow("[Sat Mar 30 10:10:41 2019] Khronick is infused by a divine restitution. Khronick healed himself for 4652 (47334) hit points by Promised Restitution Trigger I. (Critical)", "Khronick", "Khronick", 4652, 47334, "Promised Restitution Trigger I", false, "Critical")]
        [DataRow("[Fri Apr 12 11:32:11 2019] Mordsith has been healed over time for 345 (9525) hit points by Elixir of Wulthan Rk. II.", "Unknown", "Mordsith", 345, 9525, "Elixir of Wulthan Rk. II", true, null)]
        [DataRow("[Fri May 24 18:23:19 2019] Vatalae is bathed in a zealous light. Harmonious Prana healed Vatalae for 24502 hit points by Zealous Light Rk. II.", "Harmonious Prana", "Vatalae", 24502, -1, "Zealous Light Rk. II", false, null)]
        [DataRow("[Fri May 24 18:23:33 2019] Harmonious Prana healed Eruzen over time for 556 (3849) hit points by Zealous Elixir Rk. II.", "Harmonious Prana", "Eruzen", 556, 3849, "Zealous Elixir Rk. II", true, null)]
        [DataRow("[Fri May 24 18:23:40 2019] Vatalae has been healed with frenzied life-giving energy. Harmonious Prana healed Vatalae for 21367 (24855) hit points by Frenzied Renewal Rk. II.", "Harmonious Prana", "Vatalae", 21367, 24855, "Frenzied Renewal Rk. II", false, null)]

        // Old school log messages
        [DataRow("[Fri May 09 22:42:30 2003] You have been healed for 1451 points of damage.", "Unknown", "Khadaji", 1451, -1, null, false, null)]
        // [Fri May 09 22:57:10 2003] Nair is washed over by the healing wave of Prexus.
        // [Fri May 09 23:07:22 2003] You sacrifice some of your health for the benefit of your party.
        // [Mon May 12 22:22:33 2003] Nair feels a healing touch.
        // [Mon May 12 22:20:43 2003] You mend your wounds and heal some damage.
        // [DataRow("[Tue Feb 25 18:51:03 2003]     ", "hhhh", "Khadaji", 2222, -1, "sssss", false, null)]
        // [DataRow("[Fri Jun 13 23:39:32 2003] Saxstein has healed you for 1522 points of damage.", "hhhh", "pppppp", 2222, -1, "sssss", false, null)]
        [DataRow("[Tue May 27 19:06:21 2003] Dethvegi is completely healed.", "Unknown", "Dethvegi", -1, -1, "Complete Heal", false, null)]
        [DataRow("[Mon May 12 19:42:26 2003] You are completely healed.", "Unknown", "Khadaji", -1, -1, "Complete Heal", false, null)]
        // [DataRow("[Fri May 16 17:48:54 2003] A vann geistlig is completely healed.", "hhhh", "pppppp", 2222, -1, "sssss", false, null)]
        // [DataRow("xxxx", "hhhh", "pppppp", 2222, -1, "sssss", false, null)]
        public void HealTests(string logLine, string healerName, string patientName, int amount, int maxAmount, string spellName, bool isHot, string qualifier)
        {
            var logDatum = new LogDatum(logLine);

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result, logLine);
            Assert.IsTrue(lineEntry is Heal, logLine);
            var entry = lineEntry as Heal;
            Assert.AreEqual(healerName, entry.Healer.Name, logLine);
            Assert.AreEqual(patientName, entry.Patient.Name, logLine);
            Assert.AreEqual(amount, entry.Amount, logLine);
            Assert.AreEqual(maxAmount, entry.OverAmount, logLine);
            Assert.AreEqual(spellName, entry.SpellName, logLine);
            Assert.AreEqual(isHot, entry.isHealOverTime, logLine);
            Assert.AreEqual(qualifier, entry.Qualifier, logLine);
        }
    }
}
