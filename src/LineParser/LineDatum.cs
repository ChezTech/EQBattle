using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace LineParser
{
    public class LineDatum
    {
        // This should be constant.
        // Q: What about log files in other languages/locales?
        private const int DateLength = 24;

        public string RawLogLine { get; private set; }
        public DateTime LogTime { get; private set; }

        public LineDatum(string logLine)
        {
            RawLogLine = logLine;
            LogTime = GetTime();
        }

        private DateTime GetTime()
        {
            // [Fri Apr 26 10:17:02 2019]
            // [Sat Apr 20 19:23:28 2019]

            //int length = RawLogLine.IndexOf(']') - 1;
            int length = DateLength;

            var datePortion = RawLogLine.Substring(1, length);
            return DateTime.ParseExact(datePortion, "ddd MMM dd HH:mm:ss yyyy", CultureInfo.InstalledUICulture);
        }
    }
}
