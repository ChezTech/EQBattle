using BizObjects;
using LineParser.Parsers;
using LogObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LineParserTests
{
    [TestClass]
    public class SpellParserTests
    {
        private SpellParser _parser = new SpellParser(new YouResolver("Khadaji"));

        [DataTestMethod]
        [DataRow("[Sat Mar 30 08:24:42 2019] Khronick begins to cast a spell. <Blood of Jaled'Dar>", "Khronick", "Blood of Jaled'Dar")]
        [DataRow("[Mon Apr 15 21:58:45 2019] A dwarf disciple begins to cast a spell. <Aweshake>", "a dwarf disciple", "Aweshake")]
        [DataRow("[Mon Apr 15 21:58:42 2019] You begin casting Geomantra V.", "Khadaji", "Geomantra V")]
        [DataRow("[Mon Apr 15 22:13:35 2019] Khronick begins to cast a spell. <Silent Presence>", "Khronick", "Silent Presence")]
        [DataRow("[Fri May 24 18:33:20 2019] Khronick begins casting Silent Presence.", "Khronick", "Silent Presence")]
        [DataRow("[Mon May 27 07:20:45 2019] You begin casting Silent Presence.", "Khadaji", "Silent Presence")]
        [DataRow("[Mon Mar 25 18:28:01 2019] Vamileah begins to cast a spell. <Mass Group Buff>", "Vamileah", "Mass Group Buff")]
        [DataRow("[Mon Mar 25 18:28:03 2019] Vamileah begins to cast a spell. <Dead Men Floating>", "Vamileah", "Dead Men Floating")]
        [DataRow("[Fri May 24 18:23:17 2019] Harmonious Prana begins casting Zealous Light Rk. II.", "Harmonious Prana", "Zealous Light Rk. II")] // Merc name .. interesting
        [DataRow("[Mon May 27 07:20:13 2019] Movanna begins casting Armor of Vie.", "Movanna", "Armor of Vie")]
        [DataRow("[Mon May 27 09:56:28 2019] Gomphus begins casting Noxious Visions.", "Gomphus", "Noxious Visions")]
        public void Tests(string logLine, string name, string spellName)
        {
            var logDatum = new LogDatum(logLine);

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result, logLine);
            var entry = lineEntry as Spell;
            Assert.IsNotNull(entry);
            Assert.AreEqual(name, entry.Character.Name, string.Format("Failing line: {0}", logLine));
            Assert.AreEqual(spellName, entry.SpellName, string.Format("Failing line: {0}", logLine));
        }
    }
}
