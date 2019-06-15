using BizObjects.Converters;
using BizObjects.Lines;
using LineParser.Parsers;
using LogObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LineParserTests
{
    [TestClass]
    public class MercenaryUpkeepParserTests
    {
        private MercenaryUpkeepParser _parser = new MercenaryUpkeepParser(new YouResolver("Khadaji"));

        [DataTestMethod]
        [DataRow("[Tue May 28 06:01:23 2019] You have been charged a mercenary upkeep cost of 24 plat, and 7 gold and your mercenary upkeep cost timer has been reset to 15 minutes.", "24.7", "0", 0)]
        [DataRow("[Fri Apr 12 17:10:25 2019] Your mercenary waived an upkeep cost of 24 plat, and 7 gold or 1 Bayle Mark and your mercenary upkeep cost timer has been reset to 15 minutes.", "0", "24.7", 1)]
        public void Tests(string logLine, string pszCost, string pszWaivedCost, int bayleMarks)
        {
            var cost = decimal.Parse(pszCost);
            var waivedCost = decimal.Parse(pszWaivedCost);

            var logDatum = new LogDatum(logLine);

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result, logLine);
            var entry = lineEntry as MercenaryUpkeep;
            Assert.IsNotNull(entry);
            Assert.AreEqual(cost, entry.Cost, string.Format("Failing line: {0}", logLine));
            Assert.AreEqual(waivedCost, entry.WaivedCost, string.Format("Failing line: {0}", logLine));
            Assert.AreEqual(bayleMarks, entry.BayleMarks, string.Format("Failing line: {0}", logLine));
        }
    }
}
