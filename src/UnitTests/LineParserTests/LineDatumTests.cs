using LogObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace LineParserTests
{
    [TestClass]
    public class LineDatumTests
    {
        [TestMethod]
        public void NullLine()
        {
            var ld = new LogDatum(null);
            Assert.AreEqual(DateTime.MinValue, ld.LogTime);
            Assert.AreEqual(string.Empty, ld.RawLogLine);
            Assert.AreEqual(string.Empty, ld.LogMessage);
        }

        [TestMethod]
        public void EmptyLine()
        {
            var ld = new LogDatum("");
            Assert.AreEqual(DateTime.MinValue, ld.LogTime);
            Assert.AreEqual(string.Empty, ld.RawLogLine);
            Assert.AreEqual(string.Empty, ld.LogMessage);
        }

        [TestMethod]
        public void GarbageLineBiggerThanDate()
        {
            var ld = new LogDatum("kqwh37c8338jasdlfkj3kj43jrkafsdnase");
            Assert.AreEqual(DateTime.MinValue, ld.LogTime);
            Assert.AreEqual("kqwh37c8338jasdlfkj3kj43jrkafsdnase", ld.RawLogLine);
            Assert.AreEqual(string.Empty, ld.LogMessage);
        }

        [TestMethod]
        public void GarbageLineShorterThanDate()
        {
            var ld = new LogDatum("kqw");
            Assert.AreEqual(DateTime.MinValue, ld.LogTime);
            Assert.AreEqual("kqw", ld.RawLogLine);
            Assert.AreEqual(string.Empty, ld.LogMessage);
        }

        [TestMethod]
        public void GetDate()
        {
            var ld = new LogDatum(@"[Sat Apr 20 19:23:28 2019] It will take about 5 more seconds to prepare your camp.");
            Assert.AreEqual(new DateTime(2019, 4, 20, 19, 23, 28), ld.LogTime);
        }

        [TestMethod]
        public void GetRawLogLine()
        {
            var ld = new LogDatum(@"[Sat Apr 20 19:23:28 2019] It will take about 5 more seconds to prepare your camp.");
            Assert.AreEqual(@"[Sat Apr 20 19:23:28 2019] It will take about 5 more seconds to prepare your camp.", ld.RawLogLine);
        }

        [TestMethod]
        public void GetLogMessage()
        {
            var ld = new LogDatum(@"[Sat Apr 20 19:23:28 2019] It will take about 5 more seconds to prepare your camp.");
            Assert.AreEqual(@"It will take about 5 more seconds to prepare your camp.", ld.LogMessage);
        }

        [TestMethod]
        public void GetLineNumber()
        {
            var ld = new LogDatum(@"[Sat Apr 20 19:23:28 2019] It will take about 5 more seconds to prepare your camp.", 17);
            Assert.AreEqual(17, ld.LineNumber);
        }
    }
}
