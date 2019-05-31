using BizObjects;
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
        [DataRow("[Tue Apr 02 22:15:55 2019] [ANONYMOUS] Benza <Rumble of Distant Thunder>", "Benza", null, 0, null, null, "Rumble of Distant Thunder", null)]
        public void WhoTests(string logLine, string name, string @class, int level, string title, string race, string guild, string zone)
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
