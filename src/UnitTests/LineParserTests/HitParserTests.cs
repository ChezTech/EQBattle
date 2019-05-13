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

        [TestMethod]
        public void YouAttackOtherSimple()
        {
            var logDatum = new LogDatum("[Fri Apr 26 09:26:33 2019] You punch a cliknar adept for 1277 points of damage.");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Hit);
            var hitEntry = lineEntry as Hit;
            Assert.AreEqual("Khadaji", hitEntry.Attacker.Name);
            Assert.AreEqual("a cliknar adept", hitEntry.Defender.Name);
            Assert.AreEqual(1277, hitEntry.Damage);
            Assert.AreEqual("punch", hitEntry.Verb);
            Assert.IsNull(hitEntry.DamageType);
            Assert.IsNull(hitEntry.DamageBy);
            Assert.IsNull(hitEntry.DamageQualifier);
        }

        [TestMethod]
        public void YouAttackOtherSingular()
        {
            var logDatum = new LogDatum("[Sat Apr 10 21:49:26 2004] You bash a Witness of Hate informer for 1 point of damage.");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Hit);
            var hitEntry = lineEntry as Hit;
            Assert.AreEqual("Khadaji", hitEntry.Attacker.Name);
            Assert.AreEqual("a Witness of Hate informer", hitEntry.Defender.Name);
            Assert.AreEqual(1, hitEntry.Damage);
            Assert.AreEqual("bash", hitEntry.Verb);
            Assert.IsNull(hitEntry.DamageType);
            Assert.IsNull(hitEntry.DamageBy);
            Assert.IsNull(hitEntry.DamageQualifier);
        }

        [TestMethod]
        public void YouAttackOtherComplex()
        {
            var logDatum = new LogDatum("[Fri Apr 26 09:26:34 2019] You hit a cliknar adept for 1180 points of chromatic damage by Lynx Maw. (Critical)");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Hit);
            var hitEntry = lineEntry as Hit;
            Assert.AreEqual("Khadaji", hitEntry.Attacker.Name);
            Assert.AreEqual("a cliknar adept", hitEntry.Defender.Name);
            Assert.AreEqual(1180, hitEntry.Damage);
            Assert.AreEqual("hit", hitEntry.Verb);
            Assert.AreEqual("chromatic", hitEntry.DamageType);
            Assert.AreEqual("Lynx Maw", hitEntry.DamageBy);
            Assert.AreEqual("Critical", hitEntry.DamageQualifier);
        }

        [TestMethod]
        public void YourDamageShield()
        {
            var logDatum = new LogDatum("[Fri Apr 26 09:26:36 2019] A cliknar adept is burned by YOUR flames for 896 points of non-melee damage.");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Hit);
            var hitEntry = lineEntry as Hit;
            Assert.AreEqual("Khadaji", hitEntry.Attacker.Name);
            Assert.AreEqual("a cliknar adept", hitEntry.Defender.Name);
            Assert.AreEqual(896, hitEntry.Damage);
            Assert.AreEqual("burned", hitEntry.Verb);
            Assert.AreEqual("non-melee", hitEntry.DamageType);
            Assert.AreEqual("flames", hitEntry.DamageBy);
            Assert.IsNull(hitEntry.DamageQualifier);
        }

        [TestMethod]
        public void DamageShieldOnYou()
        {
            var logDatum = new LogDatum("[Fri Apr 26 09:26:36 2019] YOU are pierced by a cliknar adept's thorns for 70 points of non-melee damage!");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Hit);
            var hitEntry = lineEntry as Hit;
            Assert.AreEqual("Khadaji", hitEntry.Defender.Name);
            Assert.AreEqual("a cliknar adept", hitEntry.Attacker.Name);
            Assert.AreEqual(70, hitEntry.Damage);
            Assert.AreEqual("pierced", hitEntry.Verb);
            Assert.AreEqual("non-melee", hitEntry.DamageType);
            Assert.AreEqual("thorns", hitEntry.DamageBy);
            Assert.IsNull(hitEntry.DamageQualifier);
        }

        [TestMethod]
        public void OtherAttacksYou()
        {
            var logDatum = new LogDatum("[Fri Apr 26 09:26:36 2019] A cliknar adept pierces YOU for 865 points of damage. (Strikethrough)");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Hit);
            var hitEntry = lineEntry as Hit;
            Assert.AreEqual("Khadaji", hitEntry.Defender.Name);
            Assert.AreEqual("a cliknar adept", hitEntry.Attacker.Name);
            Assert.AreEqual(865, hitEntry.Damage);
            Assert.AreEqual("pierces", hitEntry.Verb);
            Assert.IsNull(hitEntry.DamageType);
            Assert.IsNull(hitEntry.DamageBy);
            Assert.AreEqual("Strikethrough", hitEntry.DamageQualifier);
        }

        [TestMethod]
        public void YourPetHitsOther()
        {
            var logDatum = new LogDatum("[Fri Apr 26 09:25:47 2019] Khadaji`s pet hits a cliknar adept for 597 points of damage.");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Hit);
            var hitEntry = lineEntry as Hit;
            Assert.AreEqual("Khadaji", hitEntry.Attacker.Name);
            Assert.IsTrue(hitEntry.Attacker.IsPet);
            Assert.AreEqual("a cliknar adept", hitEntry.Defender.Name);
            Assert.AreEqual(597, hitEntry.Damage);
            Assert.AreEqual("hits", hitEntry.Verb);
            Assert.IsNull(hitEntry.DamageType);
            Assert.IsNull(hitEntry.DamageBy);
            Assert.IsNull(hitEntry.DamageQualifier);
        }

        [TestMethod]
        public void OtherHitsPet()
        {
            var logDatum = new LogDatum("[Sat Mar 30 08:49:07 2019] A cliknar hunter pierces Khadaji`s pet for 2035 points of damage. (Riposte)");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Hit);
            var hitEntry = lineEntry as Hit;
            Assert.AreEqual("a cliknar hunter", hitEntry.Attacker.Name);
            Assert.AreEqual("Khadaji", hitEntry.Defender.Name);
            Assert.IsTrue(hitEntry.Defender.IsPet);
            Assert.AreEqual(2035, hitEntry.Damage);
            Assert.AreEqual("pierces", hitEntry.Verb);
            Assert.IsNull(hitEntry.DamageType);
            Assert.IsNull(hitEntry.DamageBy);
            Assert.AreEqual("Riposte", hitEntry.DamageQualifier);
        }

        [TestMethod]
        public void OtherHitsOther()
        {
            var logDatum = new LogDatum("[Sat May 17 15:28:23 2003] Zangum slashes A Razorfiend Subduer for 14 points of damage.");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Hit);
            var hitEntry = lineEntry as Hit;
            Assert.AreEqual("Zangum", hitEntry.Attacker.Name);
            Assert.AreEqual("a Razorfiend Subduer", hitEntry.Defender.Name);
            Assert.AreEqual(14, hitEntry.Damage);
            Assert.AreEqual("slashes", hitEntry.Verb);
            Assert.IsNull(hitEntry.DamageType);
            Assert.IsNull(hitEntry.DamageBy);
            Assert.IsNull(hitEntry.DamageQualifier);
        }

        [TestMethod]
        public void OtherHitsOtherComplex()
        {
            var logDatum = new LogDatum("[Sat Mar 30 08:48:54 2019] Khadaji hit a cliknar hunter for 388 points of poison damage by Strike of Venom IV.");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Hit);
            var hitEntry = lineEntry as Hit;
            Assert.AreEqual("Khadaji", hitEntry.Attacker.Name);
            Assert.AreEqual("a cliknar hunter", hitEntry.Defender.Name);
            Assert.AreEqual(388, hitEntry.Damage);
            Assert.AreEqual("hit", hitEntry.Verb);
            Assert.AreEqual("poison", hitEntry.DamageType);
            Assert.AreEqual("Strike of Venom IV", hitEntry.DamageBy);
            Assert.IsNull(hitEntry.DamageQualifier);
        }

        [TestMethod]
        public void OtherHitsOtherMultipleQualifiers()
        {
            var logDatum = new LogDatum("[Fri Apr 12 18:23:12 2019] A lavakin hits Khadaji`s pet for 1284 points of damage. (Riposte Strikethrough)");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Hit);
            var hitEntry = lineEntry as Hit;
            Assert.AreEqual("a lavakin", hitEntry.Attacker.Name);
            Assert.AreEqual("Khadaji", hitEntry.Defender.Name);
            Assert.IsTrue(hitEntry.Defender.IsPet);
            Assert.AreEqual(1284, hitEntry.Damage);
            Assert.AreEqual("hits", hitEntry.Verb);
            Assert.AreEqual(AttackType.Hit, hitEntry.Type);
            Assert.IsNull(hitEntry.DamageType);
            Assert.IsNull(hitEntry.DamageBy);
            Assert.AreEqual("Riposte Strikethrough", hitEntry.DamageQualifier); // TODO: make into enum flags
        }


        // [Sat Mar 30 07:37:19 2019] Bealica hit a cliknar hunter for 6426 points of magic damage by Pure Wildmagic.
        // [Sat Mar 30 07:37:22 2019] Bealica hit a cliknar hunter for 4513 points of fire damage by Chaos Combustion.
        // [Sat Mar 30 07:40:13 2019] Bealica hit a cliknar hunter for 11481 points of cold damage by Glacial Cascade.
        // [Sat Mar 30 07:37:24 2019] Khadaji hit a cliknar hunter for 1148 points of magic damage by Demon Crusher.
        // [Sat Mar 30 07:37:21 2019] A cliknar hunter has taken 11494 damage from Nectar of the Slitheren by Khronick. (Critical)
        // [Tue Apr 02 22:36:18 2019] A sporali disciple has taken 1969 damage from Breath of Queen Malarian by Khronick.

    }
}
