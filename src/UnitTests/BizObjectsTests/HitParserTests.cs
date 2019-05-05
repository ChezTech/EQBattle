using BizObjects;
using BizObjects.Parsers;
using LogObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BizObjectsTests
{
    [TestClass]
    public class HitParserTests
    {
        [TestMethod]
        public void YouAttackOtherSimple()
        {
            var logDatum = new LogDatum("[Fri Apr 26 09:26:33 2019] You punch a cliknar adept for 1277 points of damage.");

            var result = new HitParser().TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Hit);
            var hitEntry = lineEntry as Hit;
            Assert.AreEqual(Attack.You, hitEntry.Attacker);
            Assert.AreEqual("a cliknar adept", hitEntry.Defender);
            Assert.AreEqual(1277, hitEntry.Damage);
            Assert.AreEqual("punch", hitEntry.AttackVerb);
            Assert.IsNull(hitEntry.DamageType);
            Assert.IsNull(hitEntry.DamageBy);
            Assert.IsNull(hitEntry.DamageQualifier);
        }

        [TestMethod]
        public void YouAttackOtherSingular()
        {
            var logDatum = new LogDatum("[Sat Apr 10 21:49:26 2004] You bash a Witness of Hate informer for 1 point of damage.");

            var result = new HitParser().TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Hit);
            var hitEntry = lineEntry as Hit;
            Assert.AreEqual(Attack.You, hitEntry.Attacker);
            Assert.AreEqual("a Witness of Hate informer", hitEntry.Defender);
            Assert.AreEqual(1, hitEntry.Damage);
            Assert.AreEqual("bash", hitEntry.AttackVerb);
            Assert.IsNull(hitEntry.DamageType);
            Assert.IsNull(hitEntry.DamageBy);
            Assert.IsNull(hitEntry.DamageQualifier);
        }

        [TestMethod]
        public void YouAttackOtherComplex()
        {
            var logDatum = new LogDatum("[Fri Apr 26 09:26:34 2019] You hit a cliknar adept for 1180 points of chromatic damage by Lynx Maw. (Critical)");

            var result = new HitParser().TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Hit);
            var hitEntry = lineEntry as Hit;
            Assert.AreEqual(Attack.You, hitEntry.Attacker);
            Assert.AreEqual("a cliknar adept", hitEntry.Defender);
            Assert.AreEqual(1180, hitEntry.Damage);
            Assert.AreEqual("hit", hitEntry.AttackVerb);
            Assert.AreEqual("chromatic", hitEntry.DamageType);
            Assert.AreEqual("Lynx Maw", hitEntry.DamageBy);
            Assert.AreEqual("Critical", hitEntry.DamageQualifier);
        }

        [TestMethod]
        public void YourDamageShield()
        {
            var logDatum = new LogDatum("[Fri Apr 26 09:26:36 2019] A cliknar adept is burned by YOUR flames for 896 points of non-melee damage.");

            var result = new HitParser().TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Hit);
            var hitEntry = lineEntry as Hit;
            Assert.AreEqual(Attack.You, hitEntry.Attacker);
            Assert.AreEqual("a cliknar adept", hitEntry.Defender);
            Assert.AreEqual(896, hitEntry.Damage);
            Assert.AreEqual("burned", hitEntry.AttackVerb);
            Assert.AreEqual("non-melee", hitEntry.DamageType);
            Assert.AreEqual("flames", hitEntry.DamageBy);
            Assert.IsNull(hitEntry.DamageQualifier);
        }

        [TestMethod]
        public void DamageShieldOnYou()
        {
            var logDatum = new LogDatum("[Fri Apr 26 09:26:36 2019] YOU are pierced by a cliknar adept's thorns for 70 points of non-melee damage!");

            var result = new HitParser().TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Hit);
            var hitEntry = lineEntry as Hit;
            Assert.AreEqual(Attack.You, hitEntry.Defender);
            Assert.AreEqual("a cliknar adept", hitEntry.Attacker);
            Assert.AreEqual(70, hitEntry.Damage);
            Assert.AreEqual("pierced", hitEntry.AttackVerb);
            Assert.AreEqual("non-melee", hitEntry.DamageType);
            Assert.AreEqual("thorns", hitEntry.DamageBy);
            Assert.IsNull(hitEntry.DamageQualifier);
        }

        [TestMethod]
        public void OtherAttacksYou()
        {
            var logDatum = new LogDatum("[Fri Apr 26 09:26:36 2019] A cliknar adept pierces YOU for 865 points of damage. (Strikethrough)");

            var result = new HitParser().TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Hit);
            var hitEntry = lineEntry as Hit;
            Assert.AreEqual(Attack.You, hitEntry.Defender);
            Assert.AreEqual("a cliknar adept", hitEntry.Attacker);
            Assert.AreEqual(865, hitEntry.Damage);
            Assert.AreEqual("pierces", hitEntry.AttackVerb);
            Assert.IsNull(hitEntry.DamageType);
            Assert.IsNull(hitEntry.DamageBy);
            Assert.AreEqual("Strikethrough", hitEntry.DamageQualifier);
        }
    }
}
