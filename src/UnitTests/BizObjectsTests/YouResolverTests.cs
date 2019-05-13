using BizObjects;
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
    }
}
