using BizObjects;
using LineParser.Parsers;
using LogObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LineParserTests
{
        // [Fri Apr 26 09:25:53 2019] You wither under a vampiric strike. You healed Khadaji for 483 hit points by Vampiric Strike II.
        // [Fri Apr 26 09:25:59 2019] Khronick healed you over time for 1518 hit points by Healing Counterbias Effect.
        // [Fri Apr 26 09:26:22 2019] You are splashed by healing light. Movanna healed you for 7296 hit points by Healing Splash.
        // [Mon Mar 25 18:42:19 2019] Velvytte feels a healing touch. Thomazz healed Velvytte for 122605 hit points by Hand of Piety XXXVII.
        // [Mon Mar 25 18:42:21 2019] Thomazz healed Velvytte over time for 2882 (43000) hit points by Hand of Piety XXXVII. (Critical)
        // [Mon Mar 25 18:51:05 2019] Tweedlede is strengthened by insistent spirits. Fincez healed Tweedlede for 5532 (5831) hit points by Wulthan Focusing Rk. II.
        // [Mon Mar 25 19:29:35 2019] a clockwork soother CMXXI is surrounded by ardent armor. a clockwork soother CMXXI healed itself for 3594 (3821) hit points by Armor of the Ardent Rk. II.
        // [Mon Mar 25 19:52:30 2019] Vibab's eyes gleam with gallantry. Utuliel healed Vibab for 3775 hit points by Gallantry Rk. II.
        // [Sat Mar 30 07:34:41 2019] Movanna healed you over time for 3666 hit points by Zealous Elixir.
        // [Sat Mar 30 07:36:54 2019] The promise of divine restitution is fulfilled. You healed Khadaji for 18785 (23662) hit points by Promised Restitution Trigger I.
        // [Sat Mar 30 07:37:39 2019] Bealica is bathed in a zealous light. Movanna healed Bealica for 19169 (23333) hit points by Zealous Light.


    [TestClass]
    public class HealParserTests
    {
        private HealParser _parser = new HealParser();

        [TestMethod]
        public void YouGotHealed()
        {
            var logDatum = new LogDatum("[Sat Mar 30 07:34:54 2019] You are bathed in a zealous light. Movanna healed you for 22144 (23333) hit points by Zealous Light.");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Heal);
            var entry = lineEntry as Heal;
            Assert.AreEqual("Movanna", entry.Healer.Name);
            Assert.AreEqual(Character.You, entry.Patient.Name);
            Assert.AreEqual(22144, entry.Amount);
            Assert.AreEqual(23333, entry.MaxAmount);
            Assert.AreEqual("Zealous Light", entry.SpellName);
            Assert.IsFalse(entry.isHealOverTime);
            Assert.IsNull(entry.Qualifier);
        }
    }
}
