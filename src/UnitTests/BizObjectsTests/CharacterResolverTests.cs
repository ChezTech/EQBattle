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
            cr.SetPlayer(k);
            Assert.AreEqual(CharacterResolver.Type.Player, cr.WhichType(k));
            Assert.AreEqual(CharacterResolver.Type.Player, cr.WhichType("Khadaji"));
        }

        [TestMethod]
        public void AddVarietyOfNamesGetThemBackCorrectly()
        {
            var cr = new CharacterResolver();
            cr.SetPlayer("Khadaji");
            cr.SetMercenary("Movanna");
            cr.SetPet("Khadaji`s pet");
            cr.SetPet("Jabantik");
            cr.SetNonPlayer("Gomphus");

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
            cr.SetMercenary("Bob");
            cr.SetNonPlayer("Bob");
            // Should this fail? Exception or silent? I prefer no exception

            Assert.AreEqual(CharacterResolver.Type.Mercenary, cr.WhichType("Bob"));
        }

        [TestMethod]
        public void PetOverridesPlayer()
        {
            var cr = new CharacterResolver();
            cr.SetPlayer("Bob");
            cr.SetPet("Bob");

            Assert.AreEqual(CharacterResolver.Type.Pet, cr.WhichType("Bob"));
        }

        [TestMethod]
        public void MercenaryOverridesPlayer()
        {
            var cr = new CharacterResolver();
            cr.SetPlayer("Bob");
            cr.SetMercenary("Bob");

            Assert.AreEqual(CharacterResolver.Type.Mercenary, cr.WhichType("Bob"));
        }
    }
}
