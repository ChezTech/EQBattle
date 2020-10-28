using BizObjects.Battle;
using BizObjects.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BizObjectsTests
{
    [TestClass]
    public class CharacterNameNormalizerTests
    {
        private CharacterNameNormalizer _cnn = CharacterNameNormalizer.Instance;

        [DataTestMethod]
        [DataRow("Khadaji", "Khadaji")]
        [DataRow("Khronick's corpse", "Khronick")]
        [DataRow("Movanna's", "Movanna")]
        [DataRow("Khadaji`s pet", "Khadaji`s pet")]
        [DataRow("Girnon`s warder", "Girnon`s warder")]
        [DataRow("a Razorfiend Subduer", "a Razorfiend Subduer")]
        [DataRow("an awakened citizen", "an awakened citizen")]
        [DataRow("a dwarf disciple", "a dwarf disciple")]
        [DataRow("a craet looter", "a craet looter")]
        [DataRow("a Blade Regulant of Erillion", "a Blade Regulant of Erillion")]
        [DataRow("a refugee of Lunanyn", "a refugee of Lunanyn")]
        [DataRow("a mercenary`s corpse", "a mercenary`s corpse")]
        [DataRow("A Razorfiend Subduer", "a Razorfiend Subduer")]
        [DataRow("An awakened citizen", "an awakened citizen")]
        [DataRow("A dwarf disciple", "a dwarf disciple")]
        [DataRow("A craet looter", "a craet looter")]
        [DataRow("A Blade Regulant of Erillion", "a Blade Regulant of Erillion")]
        [DataRow("A refugee of Lunanyn", "a refugee of Lunanyn")]
        [DataRow("A mercenary`s corpse", "a mercenary`s corpse")]
        [DataRow("Living steel", "living steel")]
        [DataRow("living steel", "living steel")]
        [DataRow("The Collector", "The Collector")]
        [DataRow("Kalkek", "Kalkek")]
        [DataRow("Kaledor the Tide Turner", "Kaledor the Tide Turner")]
        [DataRow("Steel Mass", "Steel Mass")]
        [DataRow("Seed of Battle", "Seed of Battle")]
        [DataRow("Bursin the Legend", "Bursin the Legend")]
        [DataRow("Armor of the Dead", "Armor of the Dead")]
        [DataRow("Siralae's", "Siralae")]
        [DataRow("A cliknar adept's", "a cliknar adept")]
        [DataRow("Garzicor's Corpse", "Garzicor's Corpse")]
        public void Tests(string name, string normalizedName)
        {
            Assert.AreEqual(normalizedName, _cnn.NormalizeName(name), $"Name is: {name}");
        }
    }
}
