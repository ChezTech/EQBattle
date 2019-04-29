using LineParser;
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
    }
}
