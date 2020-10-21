using BizObjects.Converters;
using BizObjects.Lines;
using LineParser.Parsers;
using LogObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LineParserTests
{
    [TestClass]
    public class WhoParserTests
    {
        private WhoParser _parser = new WhoParser(new YouResolver("Khadaji"));

        [DataTestMethod]
        [DataRow("[Tue Apr 02 22:15:55 2019] [110 Ashenhand (Monk)] Tefka (Werewolf) <Rumble of Distant Thunder> ZONE: arthicrex", "Tefka", "Monk", 110, "Ashenhand", "Werewolf", "Rumble of Distant Thunder", "arthicrex")]
        [DataRow("[Tue Apr 02 22:15:55 2019] [ANONYMOUS] Benza <Rumble of Distant Thunder>", "Benza", null, 0, null, null, "Rumble of Distant Thunder", null)] // Role playing
        [DataRow("[Tue Apr 02 22:15:55 2019] [ANONYMOUS] Benza", "Benza", null, 0, null, null, null, null)] // Anonymous

        [DataRow("[Wed Oct 14 19:39:01 2020] [115 Spiritwatcher (Shaman)] Chestis (Barbarian)  ZONE: guildlobby  ", "Chestis", "Shaman", 115, "Spiritwatcher", "Barbarian", null, "guildlobby")] // No guild
        [DataRow("[Wed Oct 14 19:39:01 2020] [65 Forest Stalker (Ranger)] Konndor (Wood Elf) <Wicked Game> ZONE: guildlobby  ", "Konndor", "Ranger", 65, "Forest Stalker", "Wood Elf", "Wicked Game", "guildlobby")] // Spaces are at the end of the actual log line
        [DataRow("[Wed Oct 14 19:39:01 2020]  AFK [ANONYMOUS] Zugarbokx LFG", "Zugarbokx", null, 0, null, null, null, null, true, true)]
        [DataRow("[Wed Oct 14 19:39:01 2020]  AFK [110 Ashenhand (Monk)] Khadaji (Human) <Shamba Scum> ZONE: guildlobby  ", "Khadaji", "Monk", 110, "Ashenhand", "Human", "Shamba Scum", "guildlobby", true)]
        [DataRow("[Sat Oct 17 23:23:10 2020]  AFK [ANONYMOUS] Khronick <Shamba Scum>  LFG", "Khronick", null, 0, null, null, "Shamba Scum", null, true)]
        [DataRow("[Sat Oct 17 23:23:16 2020] [100 Spiritwatcher (Shaman)] Khronick (Barbarian) <Shamba Scum> ZONE: guildlobby   LFG", "Khronick", "Shaman", 100, "Spiritwatcher", "Barbarian", "Shamba Scum", "guildlobby", true, true)]
        [DataRow("[Wed Oct 14 19:39:35 2020] [ANONYMOUS] Sabredancer <Root Down>  LFG", "Sabredancer", null, 0, null, null, "Root Down", null, false, true)]

        public void WhoTests(string logLine, string name, string @class, int level, string title, string race, string guild, string zone, bool isAfk = false, bool isLfg = false)
        {
            var logDatum = new LogDatum(logLine);

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result, logLine);
            var entry = lineEntry as Who;
            Assert.IsNotNull(entry);
            Assert.AreEqual(name, entry.Character.Name, string.Format("Failing line: {0}", logLine));
            Assert.AreEqual(level, entry.Level, string.Format("Failing line: {0}", logLine));
            Assert.AreEqual(title, entry.Title, string.Format("Failing line: {0}", logLine));
            Assert.AreEqual(@class, entry.Class, string.Format("Failing line: {0}", logLine));
            Assert.AreEqual(race, entry.Race, string.Format("Failing line: {0}", logLine));
            Assert.AreEqual(guild, entry.Guild, string.Format("Failing line: {0}", logLine));
            Assert.AreEqual(level == 0, entry.IsAnonymous, string.Format("Failing line: {0}", logLine));

            if (zone == null)
                Assert.IsNull(entry.Zone, string.Format("Failing line: {0}", logLine));
            else
                Assert.AreEqual(zone, entry.Zone.Name, string.Format("Failing line: {0}", logLine));
        }
    }
}
