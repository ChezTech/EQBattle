using BizObjects;
using LineParser;
using LineParser.Parsers;
using LogObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LineParserTests
{
    [TestClass]
    public class LineParserFactoryTests
    {
        [TestMethod]
        public void AllowForNullCreateAction()
        {
            var parser = new LineParserFactory();
            parser.AddParser(new ZoneParser(), null);

            parser.ParseLine(new LogDatum("[Sat Mar 30 10:42:39 2019] You have entered Arthicrex."));
            // If the above line doesn't throw an exception, this test passes
        }
    }
}
