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
        }

        [TestMethod]
        public void ConvertUppercaseYou()
        {
            // Done in the YouResolver class called via parsers
            var c = new Character("YOU");
            Assert.AreEqual("YOU", c.Name);
            Assert.IsFalse(c.IsPet);
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
        }


    }
}
