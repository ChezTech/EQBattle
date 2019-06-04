using BizObjects;
using LineParser.Parsers;
using LogObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LineParserTests
{
    [TestClass]
    public class HitParserTests
    {
        private HitParser _parser = new HitParser(new YouResolver("Khadaji"));

        [DataTestMethod]
        [DataRow("[Fri Apr 26 09:26:33 2019] You punch a cliknar adept for 1277 points of damage.", "Khadaji", false, "a cliknar adept", false, 1277, "punch", AttackType.Punch, null, null, null)]
        [DataRow("[Sat Apr 10 21:49:26 2004] You bash a Witness of Hate informer for 1 point of damage.", "Khadaji", false, "a Witness of Hate informer", false, 1, "bash", AttackType.Bash, null, null, null)]
        [DataRow("[Fri Apr 26 09:26:34 2019] You hit a cliknar adept for 1180 points of chromatic damage by Lynx Maw. (Critical)", "Khadaji", false, "a cliknar adept", false, 1180, "hit", AttackType.Hit, "chromatic", "Lynx Maw", "Critical")]
        [DataRow("[Fri Apr 26 09:26:36 2019] A cliknar adept is burned by YOUR flames for 896 points of non-melee damage.", "Khadaji", false, "a cliknar adept", false, 896, "burned", AttackType.Burn, "non-melee", "flames", null)]
        [DataRow("[Fri Apr 26 09:26:36 2019] YOU are pierced by a cliknar adept's thorns for 70 points of non-melee damage!", "a cliknar adept", false, "Khadaji", false, 70, "pierced", AttackType.Pierce, "non-melee", "thorns", null)]
        [DataRow("[Fri Apr 26 09:26:36 2019] A cliknar adept pierces YOU for 865 points of damage. (Strikethrough)", "a cliknar adept", false, "Khadaji", false, 865, "pierces", AttackType.Pierce, null, null, "Strikethrough")]
        [DataRow("[Fri Apr 26 09:25:47 2019] Khadaji`s pet hits a cliknar adept for 597 points of damage.", "Khadaji", true, "a cliknar adept", false, 597, "hits", AttackType.Hit, null, null, null)]
        [DataRow("[Sat Mar 30 08:49:07 2019] A cliknar hunter pierces Khadaji`s pet for 2035 points of damage. (Riposte)", "a cliknar hunter", false, "Khadaji", true, 2035, "pierces", AttackType.Pierce, null, null, "Riposte")]
        [DataRow("[Sat May 17 15:28:23 2003] Zangum slashes A Razorfiend Subduer for 14 points of damage.", "Zangum", false, "a Razorfiend Subduer", false, 14, "slashes", AttackType.Slash, null, null, null)]
        [DataRow("[Sat Mar 30 08:48:54 2019] Khadaji hit a cliknar hunter for 388 points of poison damage by Strike of Venom IV.", "Khadaji", false, "a cliknar hunter", false, 388, "hit", AttackType.Hit, "poison", "Strike of Venom IV", null)]
        [DataRow("[Fri Apr 12 18:23:12 2019] A lavakin hits Khadaji`s pet for 1284 points of damage. (Riposte Strikethrough)", "a lavakin", false, "Khadaji", true, 1284, "hits", AttackType.Hit, null, null, "Riposte Strikethrough")]
        [DataRow("[Sat Mar 30 07:37:19 2019] Bealica hit a cliknar hunter for 6426 points of magic damage by Pure Wildmagic.", "Bealica", false, "a cliknar hunter", false, 6426, "hit", AttackType.Hit, "magic", "Pure Wildmagic", null)]
        [DataRow("[Sat Mar 30 07:37:22 2019] Bealica hit a cliknar hunter for 4513 points of fire damage by Chaos Combustion.", "Bealica", false, "a cliknar hunter", false, 4513, "hit", AttackType.Hit, "fire", "Chaos Combustion", null)]
        [DataRow("[Sat Mar 30 07:40:13 2019] Bealica hit a cliknar hunter for 11481 points of cold damage by Glacial Cascade.", "Bealica", false, "a cliknar hunter", false, 11481, "hit", AttackType.Hit, "cold", "Glacial Cascade", null)]
        [DataRow("[Sat Mar 30 07:37:24 2019] Khadaji hit a cliknar hunter for 1148 points of magic damage by Demon Crusher.", "Khadaji", false, "a cliknar hunter", false, 1148, "hit", AttackType.Hit, "magic", "Demon Crusher", null)]
        [DataRow("[Sat Mar 30 07:37:21 2019] A cliknar hunter has taken 11494 damage from Nectar of the Slitheren by Khronick. (Critical)", "Khronick", false, "a cliknar hunter", false, 11494, "DamageOverTime", AttackType.DamageOverTime, null, "Nectar of the Slitheren", "Critical")]
        [DataRow("[Tue Apr 02 22:36:18 2019] A sporali disciple has taken 1969 damage from Breath of Queen Malarian by Khronick.", "Khronick", false, "a sporali disciple", false, 1969, "DamageOverTime", AttackType.DamageOverTime, null, "Breath of Queen Malarian", null)]
        [DataRow("[Fri Apr 26 09:40:54 2019] You have taken 1960 damage from Nature's Searing Wrath by a cliknar sporali farmer's corpse.", "a cliknar sporali farmer", false, "Khadaji", false, 1960, "DamageOverTime", AttackType.DamageOverTime, null, "Nature's Searing Wrath", null)]
        [DataRow("[Tue May 28 06:48:42 2019] You hit yourself for 8000 points of unresistable damage by Cannibalization V.", "Khadaji", false, "Khadaji", false, 8000, "hit", AttackType.Hit, "unresistable", "Cannibalization V", null)] // Really this is Khronick's line, but "You" for our tests refers to Khadaji
        [DataRow("[Mon May 27 07:25:58 2019] A cliknar skirmish drone has taken 2040 damage from your Breath of Queen Malarian. (Critical)", "Khadaji", false, "a cliknar skirmish drone", false, 2040, "DamageOverTime", AttackType.DamageOverTime, null, "Breath of Queen Malarian", "Critical")]
        [DataRow("[Tue May 28 06:01:47 2019] Khronick has taken 1950 damage from Noxious Visions by Gomphus.", "Gomphus", false, "Khronick", false, 1950, "DamageOverTime", AttackType.DamageOverTime, null, "Noxious Visions", null)]
        [DataRow("[Tue May 28 06:02:16 2019] Movanna has taken 3000 damage by Noxious Visions.", "Unknown", false, "Movanna", false, 3000, "DamageOverTime", AttackType.DamageOverTime, null, "Noxious Visions", null)]
        [DataRow("[Mon May 27 09:56:45 2019] You have taken 1950 damage from Noxious Visions by Gomphus.", "Gomphus", false, "Khadaji", false, 1950, "DamageOverTime", AttackType.DamageOverTime, null, "Noxious Visions", null)]
        [DataRow("[Mon May 27 06:57:15 2019] You have taken 2080 damage from Paralyzing Bite.", "Unknown", false, "Khadaji", false, 2080, "DamageOverTime", AttackType.DamageOverTime, null, "Paralyzing Bite", null)]
        [DataRow("[Mon May 27 06:57:09 2019] You have taken 2080 damage from Paralyzing Bite by a sandspinner stalker.", "a sandspinner stalker", false, "Khadaji", false, 2080, "DamageOverTime", AttackType.DamageOverTime, null, "Paralyzing Bite", null)]
        // [DataRow("LLLLLLLLLL", "Khadaji", false, "dddddd", false, 1277, "punch", AttackType.Punch, null, null, null)]

        public void HitTests(string logLine, string attacker, bool isAttackerPet, string defender, bool isDefenderPet, int damage, string verb, AttackType attackType, string type, string by, string qualifier)
        {
            var logDatum = new LogDatum(logLine);

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result, logLine);
            Assert.IsTrue(lineEntry is Hit, logLine);
            var hitEntry = lineEntry as Hit;
            Assert.AreEqual(attacker, hitEntry.Attacker.Name, string.Format("Failing line: {0}", logLine));
            Assert.AreEqual(isAttackerPet, hitEntry.Attacker.IsPet, string.Format("Failing line: {0}", logLine));
            Assert.AreEqual(defender, hitEntry.Defender.Name, string.Format("Failing line: {0}", logLine));
            Assert.AreEqual(isDefenderPet, hitEntry.Defender.IsPet, string.Format("Failing line: {0}", logLine));
            Assert.AreEqual(damage, hitEntry.Damage, string.Format("Failing line: {0}", logLine));
            Assert.AreEqual(verb, hitEntry.Verb, string.Format("Failing line: {0}", logLine));
            Assert.AreEqual(attackType, hitEntry.Type, string.Format("Failing line: {0}", logLine));
            Assert.AreEqual(type, hitEntry.DamageType, string.Format("Failing line: {0}", logLine));
            Assert.AreEqual(by, hitEntry.DamageBy, string.Format("Failing line: {0}", logLine));
            Assert.AreEqual(qualifier, hitEntry.DamageQualifier, string.Format("Failing line: {0}", logLine));
        }

        [DataTestMethod]
        [DataRow("[Tue May 28 06:48:42 2019] Your body aches as your mind clears.  You have taken 8000 points of damage.")]
        public void NullHitTests(string logLine)
        {
            var logDatum = new LogDatum(logLine);

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsFalse(result, logLine);
            Assert.IsNull(lineEntry, logLine);
        }
    }
}
