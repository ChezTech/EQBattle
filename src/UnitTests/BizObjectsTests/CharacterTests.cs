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
    }
}
