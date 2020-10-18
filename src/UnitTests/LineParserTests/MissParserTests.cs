using BizObjects.Converters;
using BizObjects.Lines;
using LineParser.Parsers;
using LogObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LineParserTests
{
    [TestClass]
    public class MissParserTests
    {
        private MissParser _parser = new MissParser(new YouResolver("Khadaji"));

        [DataTestMethod]
        [DataRow("[Thu Apr 04 22:11:57 2019] You try to crush a gnome servant, but miss!", "Khadaji", false, "a gnome servant", false, "crush", AttackType.Crush, "miss", null)]
        [DataRow("[Thu Apr 04 22:16:14 2019] You try to crush a gnome servant, but miss! (Strikethrough)", "Khadaji", false, "a gnome servant", false, "crush", AttackType.Crush, "miss", "Strikethrough")]
        [DataRow("[Thu Apr 04 22:23:23 2019] You try to crush a bellikos recruiter, but miss! (Riposte Strikethrough)", "Khadaji", false, "a bellikos recruiter", false, "crush", AttackType.Crush, "miss", "Riposte Strikethrough")]
        [DataRow("[Thu Apr 04 22:24:02 2019] You try to strike a gnome servant, but a gnome servant dodges!", "Khadaji", false, "a gnome servant", false, "strike", AttackType.Strike, "dodges", null)]
        [DataRow("[Fri Apr 05 15:12:53 2019] You try to kick a telmira disciple, but a telmira disciple's magical skin absorbs the blow!", "Khadaji", false, "a telmira disciple", false, "kick", AttackType.Kick, "magical skin absorbs the blow", null)]
        [DataRow("[Fri Apr 12 18:23:26 2019] A lavakin tries to hit YOU, but misses!", "a lavakin", false, "Khadaji", false, "hit", AttackType.Hit, "misses", null)]
        [DataRow("[Fri Apr 12 18:23:31 2019] A lavakin tries to hit YOU, but YOU riposte!", "a lavakin", false, "Khadaji", false, "hit", AttackType.Hit, "riposte", null)]
        [DataRow("[Sat Mar 30 09:21:46 2019] A genati tries to smash YOU, but misses! (Strikethrough)", "a genati", false, "Khadaji", false, "smash", AttackType.Smash, "misses", "Strikethrough")]
        [DataRow("[Fri Apr 12 18:23:28 2019] A lavakin tries to hit YOU, but YOUR magical skin absorbs the blow! (Strikethrough)", "a lavakin", false, "Khadaji", false, "hit", AttackType.Hit, "magical skin absorbs the blow", "Strikethrough")]
        [DataRow("[Fri Apr 12 18:26:38 2019] Khadaji`s pet tries to hit a crystalkin disciple, but a crystalkin disciple parries!", "Khadaji", true, "a crystalkin disciple", false, "hit", AttackType.Hit, "parries", null)]
        [DataRow("[Fri Apr 12 18:26:39 2019] Khadaji`s pet tries to hit a crystalkin disciple, but misses!", "Khadaji", true, "a crystalkin disciple", false, "hit", AttackType.Hit, "misses", null)]
        [DataRow("[Sat Mar 30 08:14:42 2019] A master hunter tries to pierce Movanna, but misses! (Rampage)", "a master hunter", false, "Movanna", false, "pierce", AttackType.Pierce, "misses", "Rampage")]
        [DataRow("[Sat Mar 30 08:15:44 2019] A master hunter tries to pierce Movanna, but Movanna parries! (Rampage)", "a master hunter", false, "Movanna", false, "pierce", AttackType.Pierce, "parries", "Rampage")]
        [DataRow("[Sat Oct 17 16:09:22 2020] Destructivex tries to frenzy on a mortiferous golem, but misses!", "Destructivex", false, "a mortiferous golem", false, "frenzy", AttackType.Frenzy, "misses", null)]
        //[DataRow("LLLLLLLL", "Khadaji", false, "defender", false, "verb", AttackType.Unknown, "defense", null)]
        public void Tests(string logLine, string attacker, bool isAttackerPet, string defender, bool isDefenderPet, string verb, AttackType attackType, string defense, string qualifier)
        {
            var logDatum = new LogDatum(logLine);

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result, logLine, string.Format("Failing line: {0}", logLine));
            var entry = lineEntry as Miss;
            Assert.IsNotNull(entry, string.Format("Failing line: {0}", logLine));
            Assert.AreEqual(attacker, entry.Attacker.Name, string.Format("Failing line: {0}", logLine));
            Assert.AreEqual(isAttackerPet, entry.Attacker.IsPet, string.Format("Failing line: {0}", logLine));
            Assert.AreEqual(defender, entry.Defender.Name, string.Format("Failing line: {0}", logLine));
            Assert.AreEqual(isDefenderPet, entry.Defender.IsPet, string.Format("Failing line: {0}", logLine));
            Assert.AreEqual(verb, entry.Verb, string.Format("Failing line: {0}", logLine));
            Assert.AreEqual(attackType, entry.Type, string.Format("Failing line: {0}", logLine));
            Assert.AreEqual(defense, entry.DefenseType, string.Format("Failing line: {0}", logLine));
            Assert.AreEqual(qualifier, entry.Qualifier, string.Format("Failing line: {0}", logLine));
        }
    }
}
