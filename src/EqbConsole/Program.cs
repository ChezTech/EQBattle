using System;
using System.Diagnostics;
using LogFileReader;

namespace EqbConsole
{
    class Program
    {
        private const string LogFilePathName = @"C:\Program Files (x86)\Steam\steamapps\common\Everquest F2P\Logs\eqlog_Khadaji_erollisi.txt";

        static void Main(string[] args)
        {
            Console.WriteLine("Reading log file: {0}", LogFilePathName);

            var lineCount = 0;

            var sw = Stopwatch.StartNew();

            using (LogReader logReader = new LogReader(LogFilePathName))
            {
                logReader.LineRead += (s, e) => { lineCount++; };
                logReader.EoFReached += (s, e) => { logReader.StopReading(); };
                logReader.StartReading();
            }

            sw.Stop();

            Console.WriteLine("Elapsed: {0}", sw.Elapsed);
            Console.WriteLine("Line count: {0}", lineCount);
        }
    }
}
