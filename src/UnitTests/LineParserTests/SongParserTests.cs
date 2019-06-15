using BizObjects.Converters;
using BizObjects.Lines;
using LineParser.Parsers;
using LogObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LineParserTests
{
    [TestClass]
    public class SongParserTests
    {
        private SongParser _parser = new SongParser(new YouResolver("Khadaji"));

        [DataTestMethod]

        [DataRow("[Fri Apr 12 17:05:02 2019] Gottherhythm begins to sing a song. <War March of Brekt>", "Gottherhythm", "War March of Brekt")]
        public void Tests(string logLine, string name, string songName)
        {
            var logDatum = new LogDatum(logLine);

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result, logLine);
            var entry = lineEntry as Song;
            Assert.IsNotNull(entry);
            Assert.AreEqual(name, entry.Character.Name, string.Format("Failing line: {0}", logLine));
            Assert.AreEqual(songName, entry.SongName, string.Format("Failing line: {0}", logLine));
        }
    }
}
