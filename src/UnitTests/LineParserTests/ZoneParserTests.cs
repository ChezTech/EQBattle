using BizObjects;
using LineParser.Parsers;
using LogObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LineParserTests
{
    [TestClass]
    public class ZoneParserTests
    {
        private ZoneParser _parser = new ZoneParser();

        [DataTestMethod]
        [DataRow("[Sat Mar 30 10:42:39 2019] You have entered Arthicrex.", "Arthicrex")]
        [DataRow("[Sat Mar 30 10:42:39 2019] You have entered Some Complex Zone Name.", "Some Complex Zone Name")]
        // [DataRow("xxxxxx", "zzzzz")]
        public void ZoneTets(string logLine, string zoneName)
        {
            var logDatum = new LogDatum(logLine);

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Zone);
            var entry = lineEntry as Zone;
            Assert.AreEqual(zoneName, entry.Name);
        }

        [TestMethod]
        public void YouEnteredStanceNotZone()
        {
            var logDatum = new LogDatum("[Sat Mar 30 10:59:51 2019] You have entered the Drunken Monkey stance adequately.");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsFalse(result);
            Assert.IsNull(lineEntry);
        }
    }
}
