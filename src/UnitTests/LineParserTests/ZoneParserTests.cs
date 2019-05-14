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

        [TestMethod]
        public void YouEnteredZone()
        {
            var logDatum = new LogDatum("[Sat Mar 30 10:42:39 2019] You have entered Arthicrex.");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Zone);
            var entry = lineEntry as Zone;
            Assert.AreEqual("Arthicrex", entry.Name);
        }

        [TestMethod][Ignore]
        public void YouEnteredZoneComplexName()
        {
            var logDatum = new LogDatum("[Sat Mar 30 10:42:39 2019] You have entered xxxxxx xxxx xxxx.");

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Zone);
            var entry = lineEntry as Zone;
            Assert.AreEqual("zzzz", entry.Name);
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
