using BizObjects.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BizObjectsTests
{
    [TestClass]
    public class YouResolverTests
    {
        [TestMethod]
        public void VerifyName()
        {
            var yr = new YouResolver("Khadaji");
            Assert.AreEqual("Khadaji", yr.Name);
        }

        [TestMethod]
        public void VerifyYouSubstitution()
        {
            var yr = new YouResolver("Khadaji");
            Assert.AreEqual("Khadaji", yr.WhoAreYou("You"));
        }

        [TestMethod]
        public void VerifyYouSubstitutionLowercase()
        {
            var yr = new YouResolver("Khadaji");
            Assert.AreEqual("Khadaji", yr.WhoAreYou("you"));
        }

        [TestMethod]
        public void VerifyYouSubstitutionUppercase()
        {
            var yr = new YouResolver("Khadaji");
            Assert.AreEqual("Khadaji", yr.WhoAreYou("YOU"));
        }

        [TestMethod]
        public void VerifyYourSubstitutionUppercase()
        {
            var yr = new YouResolver("Khadaji");
            Assert.AreEqual("Khadaji", yr.WhoAreYou("YOUR"));
        }

        [TestMethod]
        public void VerifyNoSubstitution()
        {
            var yr = new YouResolver("Khadaji");
            Assert.AreEqual("Movanna", yr.WhoAreYou("Movanna"));
        }

        [TestMethod]
        public void VerifyTrickyNoSubstitution1()
        {
            var yr = new YouResolver("Khadaji");
            Assert.AreEqual("Healyou", yr.WhoAreYou("Healyou"));
        }

        [TestMethod]
        public void VerifyTrickyNoSubstitution2()
        {
            var yr = new YouResolver("Khadaji");
            Assert.AreEqual("Yourdoc", yr.WhoAreYou("Yourdoc"));
        }

        [TestMethod]
        public void VerifyTrickyNoSubstitution3()
        {
            var yr = new YouResolver("Khadaji");
            Assert.AreEqual("Healyougood", yr.WhoAreYou("Healyougood"));
        }

        [TestMethod]
        public void VerifyNoName()
        {
            var yr = new YouResolver();
            Assert.AreEqual("You", yr.Name);
        }

        [TestMethod]
        public void VerifyNoNameYouLowercase()
        {
            var yr = new YouResolver();
            Assert.AreEqual("You", yr.WhoAreYou("you"));
        }

        [TestMethod]
        public void VerifyNoNameYourUppercase()
        {
            var yr = new YouResolver();
            Assert.AreEqual("You", yr.WhoAreYou("YOUR"));
        }

        [TestMethod]
        public void VerifyNullName()
        {
            var yr = new YouResolver("Khadaji");
            Assert.IsNull(yr.WhoAreYou(null));
        }

        [TestMethod]
        public void VerifyEmptylName()
        {
            var yr = new YouResolver("Khadaji");
            Assert.AreEqual(string.Empty, yr.WhoAreYou(""));
        }

        [TestMethod]
        public void CheckIsThisYou()
        {
            var yr = new YouResolver("Khadaji");
            Assert.IsTrue(yr.IsThisYou("Khadaji"));
        }

        [TestMethod]
        public void CheckIsThisNotYou()
        {
            var yr = new YouResolver("Khadaji");
            Assert.IsFalse(yr.IsThisYou("Khronick"));
        }

        [TestMethod]
        public void HandleYourself()
        {
            var yr = new YouResolver("Khronick");
            Assert.AreEqual("Khronick", yr.WhoAreYou("yourself"));
        }
    }
}
