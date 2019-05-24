using BizObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace BizObjectsTests
{
    [TestClass]
    public class CharacterTests
    {
        [TestMethod]
        public void VerifyName()
        {
            var c = new Character("Khadaji");
            Assert.AreEqual("Khadaji", c.Name);
            Assert.IsFalse(c.IsPet);
            Assert.IsFalse(c.IsMob);
        }

        [TestMethod]
        public void ConvertUppercaseYou()
        {
            // Done in the YouResolver class called via parsers
            var c = new Character("YOU");
            Assert.AreEqual("YOU", c.Name);
            Assert.IsFalse(c.IsPet);
            Assert.IsFalse(c.IsMob);
        }

        [TestMethod]
        public void ConvertYour()
        {
            // Done in the YouResolver class called via parsers
            var c = new Character("YOUR");
            Assert.AreEqual("YOUR", c.Name);
        }

        [TestMethod]
        public void ConvertCapitalACapitalMonster()
        {
            var c = new Character("A Razorfiend Subduer");
            Assert.AreEqual("a Razorfiend Subduer", c.Name);
            Assert.IsFalse(c.IsPet);
        }

        [TestMethod]
        public void ConvertCapitalALowerMonster()
        {
            var c = new Character("A cliknar hunter");
            Assert.AreEqual("a cliknar hunter", c.Name);
        }

        [TestMethod]
        public void ConvertCapitalAnMonster()
        {
            var c = new Character("An awakened citizen");
            Assert.AreEqual("an awakened citizen", c.Name);
        }

        [TestMethod]
        public void ConvertPossesive()
        {
            // A master hunter is pierced by Movanna's thorns for 826 points of non-melee damage.
            var c = new Character("Movanna's");
            Assert.AreEqual("Movanna", c.Name);
            Assert.IsFalse(c.IsPet);
            Assert.IsFalse(c.IsMob);
        }

        [TestMethod]
        public void ConvertPetBackTickPossesive()
        {
            // Khadaji`s pet hits a master hunter for 668 points of damage.
            var c = new Character("Khadaji`s pet");
            Assert.AreEqual("Khadaji", c.Name);
        }

        [TestMethod]
        public void DetectPetCharacter()
        {
            // Khadaji`s pet hits a master hunter for 668 points of damage.
            var c = new Character("Khadaji`s pet");
            Assert.AreEqual("Khadaji", c.Name);
            Assert.IsTrue(c.IsPet);
            Assert.IsFalse(c.IsMob);
        }

        [TestMethod]
        public void EnsureObjectEquality()
        {
            var c1 = new Character("Khadaji");
            var c2 = new Character("Khadaji");
            Assert.AreEqual(c1, c2, "Object .Equals()");
        }

        [TestMethod]
        public void EnsureValueEquality()
        {
            var c1 = new Character("Khadaji");
            var c2 = new Character("Khadaji");
            Assert.IsTrue(c1 == c2, "Value equals ==");
        }

        [TestMethod]
        public void EnsureHashCodeDictionaryAdd()
        {
            var c1 = new Character("Khadaji");
            var c2 = new Character("Khadaji");

            var charDict = new Dictionary<Character, int>();
            Assert.IsTrue(charDict.TryAdd(c1, 17));
            Assert.IsFalse(charDict.TryAdd(c2, 19));
        }

        [TestMethod]
        public void VerifyNullBecomesUnknown()
        {
            var c = new Character(null);
            Assert.AreEqual("Unknown", c.Name);
            Assert.IsFalse(c.IsPet);
        }

        [TestMethod]
        public void VerifyEmptyBecomesUnknown()
        {
            var c = new Character("");
            Assert.AreEqual("Unknown", c.Name);
            Assert.IsFalse(c.IsPet);
        }

        [TestMethod]
        public void DetectWarderPetCharacter()
        {
            var c = new Character("Girnon`s warder");
            Assert.AreEqual("Girnon", c.Name);
            Assert.IsTrue(c.IsPet);
            Assert.IsFalse(c.IsMob);
        }

        [TestMethod]
        public void DetectGenericMobWithA()
        {
            var c = new Character("A dwarf disciple");
            Assert.AreEqual("a dwarf disciple", c.Name);
            Assert.IsFalse(c.IsPet);
            Assert.IsTrue(c.IsMob);
        }

        [TestMethod]
        public void DetectGenericMobWithAn()
        {
            var c = new Character("An awakened citizen");
            Assert.AreEqual("an awakened citizen", c.Name);
            Assert.IsFalse(c.IsPet);
            Assert.IsTrue(c.IsMob);
        }

        [TestMethod]
        public void DetectNamedMobWithSpaces()
        {
            var c = new Character("Ragbeard the Morose");
            Assert.AreEqual("Ragbeard the Morose", c.Name);
            Assert.IsFalse(c.IsPet);
            Assert.IsTrue(c.IsMob);
        }

        [TestMethod]
        [Ignore]
        public void DetectNamedMobWithoutSpaces()
        {
            var c = new Character("Sontalak");
            Assert.AreEqual("Sontalak", c.Name);
            Assert.IsFalse(c.IsPet);

            // No idea how to tell this NPC from a PC just by the name.
            // We can tell by looking at other log lines, but that's over conext of a whole fight, not solely the name.
            Assert.IsTrue(c.IsMob);
        }
    }
}
