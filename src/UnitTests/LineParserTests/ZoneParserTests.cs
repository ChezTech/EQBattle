using BizObjects.Lines;
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
        [DataRow("[Sat Mar 30 07:09:27 2019] You have entered Toxxulia Forest.", "Toxxulia Forest")]
        [DataRow("[Sat Mar 30 07:11:03 2019] You have entered Paineel.", "Paineel")]
        [DataRow("[Sat Mar 30 07:12:03 2019] You have entered The Ruins of Old Paineel.", "The Ruins of Old Paineel")]
        [DataRow("[Sun Mar 22 12:21:31 2020] You have entered Valley of Lunanyn.", "Valley of Lunanyn")]
        [DataRow("[Sun Mar 22 14:01:20 2020] You have entered Guild Lobby.", "Guild Lobby")]
        [DataRow("[Sun Sep 15 08:08:57 2019] You have entered Guild Hall.", "Guild Hall")]
        [DataRow("[Sun Sep 15 08:43:52 2019] You have entered Argath, Bastion of Illdaera.", "Argath, Bastion of Illdaera")]
        public void ZoneTets(string logLine, string zoneName)
        {
            var logDatum = new LogDatum(logLine);

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Zone);
            var entry = lineEntry as Zone;
            Assert.AreEqual(zoneName, entry.Name);
        }

        [DataTestMethod]
        [DataRow("[Sat Mar 30 10:59:51 2019] You have entered the Drunken Monkey stance adequately.")]
        [DataRow("[Thu Sep 19 23:14:59 2019] You have entered an area where levitation effects do not function.")]
        public void YouEnteredStanceNotZone(string logLine)
        {
            var logDatum = new LogDatum(logLine);

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsFalse(result);
            Assert.IsNull(lineEntry);
        }
    }
}
