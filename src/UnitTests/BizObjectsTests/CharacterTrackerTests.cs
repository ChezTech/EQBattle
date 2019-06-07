using BizObjects;
using LineParser;
using LineParser.Parsers;
using LogObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BizObjectsTests
{
    [TestClass]
    public class CharacterTrackerTests
    {
        private static readonly YouResolver YouAre = new YouResolver("Khadaji");
        // private static readonly CharacterResolver CharResolver = new CharacterResolver();
        private LineParserFactory _parser = new LineParserFactory();
        private readonly IParser _hitParser = new HitParser(YouAre);
        private readonly IParser _missParser = new MissParser(YouAre);
        private readonly IParser _healParser = new HealParser(YouAre);
        private readonly IParser _killParser = new KillParser(YouAre);

        public CharacterTrackerTests()
        {
            _parser.AddParser(_hitParser, null);
            _parser.AddParser(_missParser, null);
            _parser.AddParser(_healParser, null);
            _parser.AddParser(_killParser, null);
        }

        [TestMethod]
        public void IdentifyCharYoureAttackingAsAMob()
        {
            CharacterResolver charResolver = new CharacterResolver();
            charResolver.SetPlayer(YouAre.Name); // This is normally done at the start of a Battle

            var tracker = new CharacterTracker(YouAre, charResolver);
            tracker.TrackLine((dynamic)_parser.ParseLine(new LogDatum("[Mon May 27 09:56:06 2019] Khadaji kicks Gomphus for 871 points of damage. (Critical)")));

            Assert.AreEqual(CharacterResolver.Type.Player, charResolver.WhichType("Khadaji"));
            Assert.AreEqual(CharacterResolver.Type.NonPlayerCharacter, charResolver.WhichType("Gomphus"));
        }

        [TestMethod]
        public void YouAreNotAMonster()
        {
            CharacterResolver charResolver = new CharacterResolver();
            charResolver.SetPlayer(YouAre.Name); // This is normally done at the start of a Battle

            var tracker = new CharacterTracker(YouAre, charResolver);
            tracker.TrackLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:10:45 2019] You hit yourself for 8000 points of unresistable damage by Cannibalization V.")));

            Assert.AreEqual(CharacterResolver.Type.Player, charResolver.WhichType("Khadaji"));
        }

        [TestMethod]
        public void WhoLineSticks()
        {
            CharacterResolver charResolver = new CharacterResolver();
            charResolver.SetPlayer(YouAre.Name); // This is normally done at the start of a Battle

            var tracker = new CharacterTracker(YouAre, charResolver);
            tracker.TrackLine((dynamic)_parser.ParseLine(new LogDatum("[Fri May 24 18:26:18 2019] [86 Spiritwatcher (Shaman)] Khronick (Barbarian) <Siblings of the Shroud> ZONE: arthicrex")));

            Assert.AreEqual(CharacterResolver.Type.Player, charResolver.WhichType("Khadaji"));
        }

        [TestMethod]
        public void WhoLineDetectsMercenary()
        {
            CharacterResolver charResolver = new CharacterResolver();
            charResolver.SetPlayer(YouAre.Name); // This is normally done at the start of a Battle

            var tracker = new CharacterTracker(YouAre, charResolver);
            tracker.TrackLine((dynamic)_parser.ParseLine(new LogDatum("[Fri May 24 18:26:18 2019] [86 Spiritwatcher (Shaman)] Khronick (Barbarian) <Siblings of the Shroud> ZONE: arthicrex")));

            Assert.AreEqual(CharacterResolver.Type.Player, charResolver.WhichType("Khadaji"));
        }
    }
}
