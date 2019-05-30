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

        [TestMethod]
        public void AddPCGetPC()
        {
            var k = new Character("Khadaji");
            var cr = new CharacterResolver();
            cr.AddPlayer(k);
            Assert.AreEqual(CharacterResolver.Type.Player, cr.WhichType(k));
            Assert.AreEqual(CharacterResolver.Type.Player, cr.WhichType("Khadaji"));
        }

        [TestMethod]
        public void AddVarietyOfNamesGetThemBackCorrectly()
        {
            var cr = new CharacterResolver();
            cr.AddPlayer("Khadaji");
            cr.AddMercenary("Movanna");
            cr.AddPet("Khadaji`s pet");
            cr.AddPet("Jabantik");
            cr.AddNonPlayer("Gomphus");

            Assert.AreEqual(CharacterResolver.Type.Player, cr.WhichType("Khadaji"));
            Assert.AreEqual(CharacterResolver.Type.Mercenary, cr.WhichType("Movanna"));
            Assert.AreEqual(CharacterResolver.Type.Pet, cr.WhichType("Khadaji`s pet"));
            Assert.AreEqual(CharacterResolver.Type.Pet, cr.WhichType("Jabantik"));
            Assert.AreEqual(CharacterResolver.Type.NonPlayerCharacter, cr.WhichType("Gomphus"));
        }

        [TestMethod]
        [Ignore]
        public void AddNameToMultipleListsGetBackWhichShouldNotBeAllowed()
        {
            var cr = new CharacterResolver();
            cr.AddMercenary("Bob");
            cr.AddNonPlayer("Bob");
            // Should this fail? Exception or silent? I prefer no exception

            Assert.AreEqual(CharacterResolver.Type.Mercenary, cr.WhichType("Bob"));
        }
    }
}
