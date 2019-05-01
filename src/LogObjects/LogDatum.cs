using System;
using System.Globalization;

namespace LogObjects
{
    public class LogDatum
    {
        // This should be constant.
        // Q: What about log files in other languages/locales?
        private const int DateLength = 24;
        private const int MessageStart = 27;

        public string RawLogLine { get; private set; }
        public DateTime LogTime { get; private set; }
        public string LogMessage { get; private set; }
        public int LineNumber { get; private set; }


        public LogDatum(string logLine, int lineNumber = -1)
        {
            RawLogLine = logLine;
            LineNumber = lineNumber;

            LogMessage = RawLogLine.Substring(MessageStart);
            LogTime = GetTime();
        }

        private DateTime GetTime()
        {
            // [Fri Apr 26 10:17:02 2019]
            // [Sat Apr 20 19:23:28 2019]

            //int length = RawLogLine.IndexOf(']') - 1;
            var datePortion = RawLogLine.Substring(1, DateLength);
            return DateTime.ParseExact(datePortion, "ddd MMM dd HH:mm:ss yyyy", CultureInfo.InstalledUICulture);
        }
    }
}
