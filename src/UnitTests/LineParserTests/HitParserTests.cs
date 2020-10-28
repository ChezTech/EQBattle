using BizObjects.Converters;
using BizObjects.Lines;
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
        [DataRow("[Mon May 27 07:20:04 2019] You hit a cliknar skirmish drone for 22528 points of physical damage by Five Point Palm VI.", "Khadaji", false, "a cliknar skirmish drone", false, 22528, "hit", AttackType.Hit, "physical", "Five Point Palm VI", null)]
        [DataRow("[Mon May 27 07:20:04 2019] You hit yourself for 1112 points of unresistable damage by Five Point Palm Focusing.", "Khadaji", false, "Khadaji", false, 1112, "hit", AttackType.Hit, "unresistable", "Five Point Palm Focusing", null)]
        [DataRow("[Tue Apr 01 21:58:49 2003] A Razorfiend Subduer was hit by non-melee for 83 points of damage.", "Unknown", false, "a Razorfiend Subduer", false, 83, "hit", AttackType.Hit, "non-melee", null, null)]
        [DataRow("[Sat Oct 17 16:08:41 2020] Destructivex frenzies on a sepulcher skeleton for 4061 points of damage. (Riposte Strikethrough)", "Destructivex", false, "a sepulcher skeleton", false, 4061, "frenzies on", AttackType.Frenzy, null, null, "Riposte Strikethrough")]
        [DataRow("[Fri Oct 23 20:59:13 2020] A drolvarg gnasher has taken 46487 damage from Phase Spider Blood by Khronick. (Critical)", "Khronick", false, "a drolvarg gnasher", false, 46487, "DamageOverTime", AttackType.DamageOverTime, null, "Phase Spider Blood", "Critical")]
        [DataRow("[Fri Oct 23 21:52:17 2020] Kelanna is tormented by a tundra yeti's frost for 775 points of non-melee damage.", "a tundra yeti", false, "Kelanna", false, 775, "tormented", AttackType.Torment, "non-melee", "frost", null)]
        [DataRow("[Sat Jun 13 22:17:04 2020] A mature oashim is tormented by YOUR frost for 340 points of non-melee damage.", "Khadaji", false, "a mature oashim", false, 340, "tormented", AttackType.Torment, "non-melee", "frost", null)]
        [DataRow("[Mon Oct 26 20:55:26 2020] A Syldon burninator is tormented by Footlouse's frost for 97 points of non-melee damage.", "Footlouse", false, "a Syldon burninator", false, 97, "tormented", AttackType.Torment, "non-melee", "frost", null)]
        [DataRow("[Fri Oct 23 22:12:28 2020] YOU are tormented by a tundra yeti's frost for 771 points of non-melee damage!", "a tundra yeti", false, "Khadaji", false, 771, "tormented", AttackType.Torment, "non-melee", "frost", null)]
        [DataRow("[Wed Oct 21 20:41:24 2020] A bottomless gnawer is pierced by Siralae's thorns for 1838 points of non-melee damage.", "Siralae", false, "a bottomless gnawer", false, 1838, "pierced", AttackType.Pierce, "non-melee", "thorns", null)]
        [DataRow("[Wed Oct 21 20:40:56 2020] An Arisen skeleton is burned by Oriaxxe's flames for 5415 points of non-melee damage.", "Oriaxxe", false, "an Arisen skeleton", false, 5415, "burned", AttackType.Burn, "non-melee", "flames", null)]
        [DataRow("[Mon Oct 26 20:21:49 2020] A Syldon drill sergeant has taken 49611 damage from Phase Spider Blood by Khronick. (Critical)", "Khronick", false, "a Syldon drill sergeant", false, 49611, "DamageOverTime", AttackType.DamageOverTime, null, "Phase Spider Blood", "Critical")]
        [DataRow("[Mon Oct 26 20:21:55 2020] A Syldon drill sergeant has taken 44906 damage from Phase Spider Blood by Khronick.", "Khronick", false, "a Syldon drill sergeant", false, 44906, "DamageOverTime", AttackType.DamageOverTime, null, "Phase Spider Blood", null)]
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

        [DataRow("[Mon May 27 07:14:39 2019] Your skin ignites in a pyroclastic fire.  You have taken 1278 points of damage.", "Khadaji", 1277, "ignites", "pyroclastic fire", "skin")]
        [DataRow("[Mon May 27 07:15:57 2019] You are caught in a rain of molten scoria.  You have taken 399 points of damage.", "Khadaji", 1277, "caught", "rain of molten scoria", null)]
        [DataRow("[Mon May 27 07:30:50 2019] You are struck by a phantasmal blade of discordant steel.  You have taken 1271 points of damage.", "Khadaji", 1277, "struck", "phantasmal blade of discordant steel", null)]
        [DataRow("[Mon May 27 07:41:34 2019] You stagger in awe.  You have taken 259 points of damage.", "Khadaji", 1277, "stagger", "awe", null)]
        [DataRow("[Mon May 27 08:47:04 2019] You are covered in crystals of rime.  You have taken 1185 points of damage.", "Khadaji", 1277, "covered", "crystals of rime", null)]
        [DataRow("[Mon May 27 08:48:58 2019] You are consumed in a sunbeam.  You have taken 864 points of damage.", "Khadaji", 1277, "consumed", "sunbeam", null)]
        [DataRow("[Mon May 27 09:21:56 2019] A blazing wave of heat engulfs you.  You have taken 741 points of damage.", "Khadaji", 1277, "engulfs", "blazing wave of heat", null)]
        [DataRow("[Thu Apr 04 21:59:42 2019] You are denounced.  You have taken 664 points of damage.", "Khadaji", 1277, "denounced", "spell", null)]
        [DataRow("[Thu Apr 04 22:14:50 2019] You are consumed in raging mana.  You have taken 1898 points of damage.", "Khadaji", 1277, "consumed", "raging mana", null)]
        [DataRow("[Thu Apr 04 22:23:54 2019] You have been struck by a bolt of molten scoria.  You have taken 3358 points of damage.", "Khadaji", 1277, "struck", "bolt of molten scoria", null)]
        [DataRow("[Thu Apr 04 22:30:15 2019] You are struck by the claw of Gorenaire.  You have taken 359 points of damage.", "Khadaji", 1277, "struck", "claw of Gorenaire", null)]
        [DataRow("[Thu Apr 04 22:30:31 2019] You are burned by the breath of Klixcxyk.  You have taken 4381 points of damage.", "Khadaji", 1277, "burned", "breath of Klixcxyk", null)]
        [DataRow("[Fri Apr 05 15:06:42 2019] The Ukun's bite sends poison coursing through your veins.  You have taken 806 points of damage.", "Khadaji", 1277, "sends poison coursing", "Ukun's bite", "veins")]
        [DataRow("[Sat Mar 30 08:18:43 2019] You are struck by a masterful warrior.  You have taken 13656 points of damage.", "Khadaji", 1277, "struck", "masterful warrior", null)]
        public void SpellDamage(string logLine, string attacker, int damage, string verb, string by, string spellTarget)
        {
            var logDatum = new LogDatum(logLine);

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result, logLine);
            Assert.IsTrue(lineEntry is Hit, logLine);
            var hitEntry = lineEntry as Hit;
            Assert.AreEqual(attacker, hitEntry.Attacker.Name, string.Format("Failing line: {0}", logLine));
            Assert.AreEqual(damage, hitEntry.Damage, string.Format("Failing line: {0}", logLine));
            // Assert.AreEqual(verb, hitEntry.Verb, string.Format("Failing line: {0}", logLine));
            // Assert.AreEqual(by, hitEntry.By, string.Format("Failing line: {0}", logLine));
            // Assert.AreEqual(spellTarget, hitEntry.xxx, string.Format("Failing line: {0}", logLine));
        }

        [DataTestMethod]

        // This one actually parses now ... that's ok, but will need to not double count the damage during a Fight. See `FightTests.TestCannibalization()`
        // [DataRow("[Tue May 28 06:48:42 2019] Your body aches as your mind clears.  You have taken 8000 points of damage.")] // Part of Cannibalization

        [DataRow("[Mon May 27 07:20:04 2019]   You have taken 1112 points of damage.")] // Part of Five Point Palm
        public void NullHitTests(string logLine)
        {
            var logDatum = new LogDatum(logLine);

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsFalse(result, logLine);
            Assert.IsNull(lineEntry, logLine);
        }
    }
}
