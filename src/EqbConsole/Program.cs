using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BizObjects;
using LineParser;
using LineParser.Parsers;
using LogFileReader;
using LogObjects;

namespace EqbConsole
{
    class Program
    {
        //private const string LogFilePathName = @"C:\Program Files (x86)\Steam\steamapps\common\Everquest F2P\Logs\eqlog_Khadaji_erollisi_test.txt";
        private const string LogFilePathName = @"C:\Program Files (x86)\Steam\steamapps\common\Everquest F2P\Logs\eqlog_Khadaji_erollisi_test_medium.txt";
        //private const string LogFilePathName = @"C:\Program Files (x86)\Steam\steamapps\common\Everquest F2P\Logs\eqlog_Khadaji_erollisi_2019-03-25-182700.txt";

        private LineParserFactory _parser;
        private List<ILine> _lineCollection = new List<ILine>();
        private List<Unknown> _unknownCollection = new List<Unknown>();
        private List<Hit> _hitCollection = new List<Hit>();
        private List<Kill> _killCollection = new List<Kill>();
        private List<Miss> _missCollection = new List<Miss>();

        private BlockingCollection<LogDatum> _jobQueueLogLines = new BlockingCollection<LogDatum>();

        static void Main(string[] args)
        {
            var logPath = args.Length > 0 ? args[0] : LogFilePathName;
            new Program().RunProgram(logPath);
        }

        private Program()
        {
            _parser = new LineParserFactory();
            _parser.UnknownCreated += x => { _unknownCollection.Add(x); };
            _parser.AddParser(new KillParser(), x => { _killCollection.Add((dynamic)x); });
            _parser.AddParser(new HitParser(), x => { _hitCollection.Add((dynamic)x); });
            _parser.AddParser(new MissParser(), x => { _missCollection.Add((dynamic)x); });
        }

        private void RunProgram(string logPath)
        {
            WriteMessage("Starting EQBattle");
            var lineCount = 0;

            var sw = Stopwatch.StartNew();
            var parseElapsed = sw.Elapsed;
            var readElapsed = sw.Elapsed;

            Task.Run(() => parseElapsed = ParseLines(sw));
            Task.Run(() => readElapsed = ReadLines(logPath, out lineCount, sw));

            // Keep the console window open while the
            // consumer thread completes its output.
            WriteMessage("Press any key to halt parsing. Wait for the 'done' message to finish");
            Console.ReadKey(true);


            WriteMessage("Read Elapsed: {0}", readElapsed);
            WriteMessage("Parse Elapsed: {0}", parseElapsed);
            WriteMessage("Line count: {0:N0}", lineCount);
            WriteMessage("Job Queue: {0:N0}", _jobQueueLogLines.Count);

            WriteMessage("Line collection count: {0}", _lineCollection.Count);
            WriteMessage("Unknown collection count: {0}", _unknownCollection.Count);
            WriteMessage("Attack collection count: {0}", _hitCollection.Count);
            WriteMessage("Kill collection count: {0}", _killCollection.Count);
            WriteMessage("Miss collection count: {0}", _missCollection.Count);

            WriteMessage("===== Attacks ======");
            WriteMessage("Total: {0:N0}", _hitCollection.Sum(x => x.Damage));
            WriteMessage("You: {0:N0}  Ouch: {1:N0}",
                _hitCollection.Where(x => x.Attacker.Name == Attack.You).Sum(x => x.Damage),
                _hitCollection.Where(x => x.Defender.Name == Attack.You).Sum(x => x.Damage));
            WriteMessage("Pet: {0:N0}  Ouch: {1:N0}",
                _hitCollection.Where(x => x.Attacker.Name == "Khadaji" && x.Attacker.IsPet).Sum(x => x.Damage),
                _hitCollection.Where(x => x.Defender.Name == "Khadaji" && x.Defender.IsPet).Sum(x => x.Damage));
            WriteMessage("Mob: {0:N0}  Ouch: {1:N0}",
                _hitCollection.Where(x => x.Attacker.Name == "a cliknar adept").Sum(x => x.Damage),
                _hitCollection.Where(x => x.Defender.Name == "a cliknar adept").Sum(x => x.Damage));
        }

        private TimeSpan ParseLines(Stopwatch sw)
        {
            TimeSpan parseElapsed;
            WriteMessage("Starting to parse lines...");
            while (!_jobQueueLogLines.IsCompleted)
            {
                try
                {
                    var logLine = _jobQueueLogLines.Take();
                    _parser.ParseLine(logLine);

                    if (logLine.LineNumber % 10000 == 0)
                        WriteMessage("Parsed {0:N0} lines...", logLine.LineNumber);
                }
                catch (InvalidOperationException) { }
            }
            parseElapsed = sw.Elapsed;

            WriteMessage("Done parsing lines.");
            WriteMessage("");
            return parseElapsed;
        }

        private TimeSpan ReadLines(string logPath, out int lineCount, Stopwatch sw)
        {
            WriteMessage("Reading log file: {0}", logPath);

            int count = 0;
            using (LogReader logReader = new LogReader(logPath))
            {
                logReader.LineRead += (s, e) =>
                {
                    count++;
                    var logLine = new LogDatum(e.LogLine, count);
                    _jobQueueLogLines.Add(logLine);
                };
                logReader.EoFReached += (s, e) => { logReader.StopReading(); };
                logReader.StartReading();
            }
            _jobQueueLogLines.CompleteAdding();
            lineCount = count;

            WriteMessage("Done reading log file");
            return sw.Elapsed;
        }

        private void WriteMessage(string format, params object[] args)
        {
            Console.WriteLine("[{0:yyyy-MM-dd HH:mm:ss.fff}] ({1,6}) {2}", DateTime.Now, Thread.CurrentThread.ManagedThreadId, string.Format(format, args));
        }
    }
}
