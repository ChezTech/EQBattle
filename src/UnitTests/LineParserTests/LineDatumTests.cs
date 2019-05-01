using LogObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace LineParserTests
{
    [TestClass]
    public class LineDatumTests
    {
        [TestMethod]
        public void GetDate()
        {
            var ld = new LineDatum(@"[Sat Apr 20 19:23:28 2019] It will take about 5 more seconds to prepare your camp.");
            Assert.AreEqual(new DateTime(2019, 4, 20, 19, 23, 28), ld.LogTime);
        }

        [TestMethod]
        public void GetRawLogLine()
        {
            var ld = new LineDatum(@"[Sat Apr 20 19:23:28 2019] It will take about 5 more seconds to prepare your camp.");
            Assert.AreEqual(@"[Sat Apr 20 19:23:28 2019] It will take about 5 more seconds to prepare your camp.", ld.RawLogLine);
        }

        [TestMethod]
        public void GetLogMessage()
        {
            var ld = new LineDatum(@"[Sat Apr 20 19:23:28 2019] It will take about 5 more seconds to prepare your camp.");
            Assert.AreEqual(@"It will take about 5 more seconds to prepare your camp.", ld.LogMessage);
        }

        [TestMethod]
        public void GetLineNumber()
        {
            var ld = new LineDatum(@"[Sat Apr 20 19:23:28 2019] It will take about 5 more seconds to prepare your camp.", 17);
            Assert.AreEqual(17, ld.LineNumber);
        }

        [TestMethod]
        public void GetZone()
        {
            var ld = new LineDatum(@"[Sat Apr 20 19:23:28 2019] It will take about 5 more seconds to prepare your camp.", currentZone: "Arthicrex");
            Assert.AreEqual("Arthicrex", ld.Zone);
        }
    }
}
