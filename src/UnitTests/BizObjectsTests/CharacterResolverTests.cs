using BizObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BizObjectsTests
{
    [TestClass]
    public class CharacterResolverTests
    {
        [TestMethod]
        public void UnknownNameAtStartWhenAllIsEmpty()
        {
            var cr = new CharacterResolver();
            Assert.AreEqual(CharacterResolver.Type.Unknown, cr.WhichType("Khadaji"));
        }

        [TestMethod]
        public void UnknownCharacterAtStartWhenAllIsEmpty()
        {
            var cr = new CharacterResolver();
            Assert.AreEqual(CharacterResolver.Type.Unknown, cr.WhichType(new Character("Khadaji")));
        }
    }
}
