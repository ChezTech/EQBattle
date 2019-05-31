using System;
using System.Diagnostics;
using System.Globalization;

namespace LogObjects
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class LogDatum
    {
        // This should be constant.
        // Q: What about log files in other languages/locales?
        private const int DateLength = 24;
        private const int MessageStart = 27;

        public string RawLogLine { get; }
        public DateTime LogTime { get; }
        public string LogMessage { get; }
        public int LineNumber { get; }


        public LogDatum(string logLine, int lineNumber = -1)
        {
            RawLogLine = logLine ?? string.Empty;
            LineNumber = lineNumber;

            LogTime = GetTime();
            LogMessage = GetMessage();
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
                return DateTime.MinValue;
            }
        }

        private string GetMessage()
        {
            // This is a little annoying to have to check this each time and to have to ensure that the LogTime is set before we try to set the LogMessage
            if (LogTime == DateTime.MinValue)
                return string.Empty;

            if (RawLogLine.Length <= MessageStart)
                return string.Empty;

            return RawLogLine.Substring(MessageStart);
        }


        private string DebuggerDisplay => LogMessage;
    }
}
