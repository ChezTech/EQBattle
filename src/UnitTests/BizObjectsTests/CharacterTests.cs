using BizObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BizObjectsTest
{
    [TestClass]
    public class CharacterTests
    {
        [TestMethod]
        public void VerifyName()
        {
            var c = new Character("Khadaji");
            Assert.AreEqual("Khadaji", c.Name);
        }

        [TestMethod]
        public void ConvertUppercaseYou()
        {
            var c = new Character("YOU");
            Assert.AreEqual("You", c.Name);
        }

        [TestMethod]
        public void ConvertYour()
        {
            var c = new Character("YOUR");
            Assert.AreEqual("You", c.Name);
        }

        [TestMethod]
        public void ConvertCapitalACapitalMonster()
        {
            var c = new Character("A Razorfiend Subduer");
            Assert.AreEqual("a Razorfiend Subduer", c.Name);
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
    }
}
