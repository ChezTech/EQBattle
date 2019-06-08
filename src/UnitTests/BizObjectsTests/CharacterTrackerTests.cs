using System;
using BizObjects;
using LogObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BizObjectsTests
{

    [TestClass]
    public class CharacterTrackerTests : ParserTestBase
    {
        private readonly Action<CharacterTracker, string> TrackLine = (tracker, logLine) =>
        {
            ILine line = _parser.ParseLine(new LogDatum(logLine));
            tracker.TrackLine((dynamic)line);
        };

        [TestMethod]
        public void IdentifyCharYoureAttackingAsAMob()
        {
            var tracker = SetupNewTracker(out CharacterResolver charResolver);

            TrackLine(tracker, "[Mon May 27 09:56:06 2019] Khadaji kicks Gomphus for 871 points of damage. (Critical)");

            Assert.AreEqual(CharacterResolver.Type.Player, charResolver.WhichType("Khadaji"));
            Assert.AreEqual(CharacterResolver.Type.NonPlayerCharacter, charResolver.WhichType("Gomphus"));
        }

        [TestMethod]
        public void YouAreNotAMonster()
        {
            var tracker = SetupNewTracker(out CharacterResolver charResolver);

            TrackLine(tracker, "[Tue May 28 06:10:45 2019] You hit yourself for 8000 points of unresistable damage by Cannibalization V.");

            Assert.AreEqual(CharacterResolver.Type.Player, charResolver.WhichType("Khadaji"));
        }

        [TestMethod]
        public void WhoLineSetsPlayer()
        {
            var tracker = SetupNewTracker(out CharacterResolver charResolver);

            TrackLine(tracker, "[Fri May 24 18:26:18 2019] [86 Spiritwatcher (Shaman)] Khronick (Barbarian) <Siblings of the Shroud> ZONE: arthicrex");

            Assert.AreEqual(CharacterResolver.Type.Player, charResolver.WhichType("Khronick"));
        }

        [TestMethod]
        public void WhoLineSticks()
        {
            var tracker = SetupNewTracker(out CharacterResolver charResolver);

            TrackLine(tracker, "[Fri May 24 18:26:18 2019] [86 Spiritwatcher (Shaman)] Khronick (Barbarian) <Siblings of the Shroud> ZONE: arthicrex");

            // Fake line, but it would set Khronick to an NPC all by itself
            TrackLine(tracker, "[Mon May 27 06:40:15 2019] You bite Khronick for 2877 points of damage.");

            Assert.AreEqual(CharacterResolver.Type.Player, charResolver.WhichType("Khronick"));
        }

        [TestMethod]
        public void HealOfPlayerDetectsMercenary()
        {
            var tracker = SetupNewTracker(out CharacterResolver charResolver);

            // Set the player
            charResolver.SetPlayer("Khronick");

            // A heal involving another player defaults to a Mercenary (It could be a player, but those should be picked up via the Who command previously)
            TrackLine(tracker, "[Mon May 27 06:40:40 2019] Khronick glows with health. Movanna healed Khronick for 7034 (7296) hit points by Healing Splash.");

            Assert.AreEqual(CharacterResolver.Type.Mercenary, charResolver.WhichType("Movanna"));
        }

        [TestMethod]
        public void HealOfPlayerDoesntOverrideExistingPlayer()
        {
            var tracker = SetupNewTracker(out CharacterResolver charResolver);

            // Set the player
            charResolver.SetPlayer("Khronick");
            charResolver.SetPlayer("Movanna"); // We'll pretend this is a player already detected ... we're also not using the Sticky flag, which we already know would force it not to change

            // This heal, because it's a player, should stay as a player
            TrackLine(tracker, "[Mon May 27 06:40:40 2019] Khronick glows with health. Movanna healed Khronick for 7034 (7296) hit points by Healing Splash.");

            Assert.AreEqual(CharacterResolver.Type.Player, charResolver.WhichType("Movanna"));
        }

        [TestMethod]
        public void DetectPetViaMasterLine()
        {
            // Pet says 'My leader is Bob'

        }

        private CharacterTracker SetupNewTracker(out CharacterResolver charResolver)
        {
            charResolver = new CharacterResolver();
            charResolver.SetPlayer(YouAre.Name); // This is normally done at the start of a Battle
            return new CharacterTracker(YouAre, charResolver);
        }
    }
}
