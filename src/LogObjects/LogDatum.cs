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
            RawLogLine = logLine ?? string.Empty;
            LineNumber = lineNumber;

            LogMessage = RawLogLine.Length > MessageStart ? RawLogLine.Substring(MessageStart) : string.Empty;
            LogTime = GetTime();
        }

        private DateTime GetTime()
        {
            // [Fri Apr 26 10:17:02 2019]
            // [Sat Apr 20 19:23:28 2019]

            //int length = RawLogLine.IndexOf(']') - 1;

            try
            {
                var datePortion = RawLogLine.Substring(1, DateLength);
                return DateTime.ParseExact(datePortion, "ddd MMM dd HH:mm:ss yyyy", CultureInfo.InstalledUICulture);
            }
            catch
            {
                LogMessage = string.Empty; // Not the greatest of places to override this (essentially a side-effect), but we really only want to do it in this case
                return DateTime.MinValue;
            }
        }
    }
}
