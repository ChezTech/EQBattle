namespace LogFileReader
{
    public class LineReadArgs
    {
        public string LogLine { get; set; }

        public LineReadArgs(string logLine)
        {
            LogLine = logLine;
        }
    }
}
