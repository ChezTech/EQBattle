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
        private ConcurrentQueue<ILine> _lineCollection = new ConcurrentQueue<ILine>();
        private ConcurrentQueue<Unknown> _unknownCollection = new ConcurrentQueue<Unknown>();
        private ConcurrentQueue<Hit> _hitCollection = new ConcurrentQueue<Hit>();
        private ConcurrentQueue<Kill> _killCollection = new ConcurrentQueue<Kill>();
        private ConcurrentQueue<Miss> _missCollection = new ConcurrentQueue<Miss>();
        private ConcurrentQueue<Heal> _healCollection = new ConcurrentQueue<Heal>();

        private BlockingCollection<LogDatum> _jobQueueLogLines = new BlockingCollection<LogDatum>();

        static void Main(string[] args)
        {
            var logPath = args.Length > 0 ? args[0] : LogFilePathName;
            var numberOfParsers = args.Length > 1 ? int.Parse(args[1]) : 1;
            new Program().RunProgram(logPath, numberOfParsers);
        }

        private Program()
        {
            _parser = new LineParserFactory();
            _parser.UnknownCreated += x => { _unknownCollection.Enqueue(x); };
            _parser.AddParser(new KillParser(), x => { _killCollection.Enqueue((dynamic)x); });
            _parser.AddParser(new HitParser(), x => { _hitCollection.Enqueue((dynamic)x); });
            _parser.AddParser(new MissParser(), x => { _missCollection.Enqueue((dynamic)x); });
            _parser.AddParser(new HealParser(), x => { _healCollection.Enqueue((dynamic)x); });
        }

        private void RunProgram(string logPath, int numberOfParsers)
        {
            WriteMessage("Starting EQBattle with {0} parsers", numberOfParsers);
            var lineCount = 0;

            var sw = Stopwatch.StartNew();
            var readElapsed = sw.Elapsed;

            var parserTasks = new List<Task>();

            for (int i = 0; i < numberOfParsers; i++)
                parserTasks.Add(Task.Run(() => ParseLines()));

            Task.Run(() => readElapsed = ReadLines(logPath, out lineCount, sw));

            Task.WaitAll(parserTasks.ToArray());
            var parseElapsed = sw.Elapsed;

            WriteMessage("Read Elapsed: {0}", readElapsed);
            WriteMessage("Parse Elapsed: {0}", parseElapsed);
            WriteMessage("Line count: {0:N0}", lineCount);
            WriteMessage("Job Queue: {0:N0}", _jobQueueLogLines.Count);

            WriteMessage("Line collection count: {0}", _lineCollection.Count);
            WriteMessage("Unknown collection count: {0}", _unknownCollection.Count);
            WriteMessage("Attack collection count: {0}", _hitCollection.Count);
            WriteMessage("Kill collection count: {0}", _killCollection.Count);
            WriteMessage("Miss collection count: {0}", _missCollection.Count);
            WriteMessage("Heal collection count: {0}", _healCollection.Count);

            WriteMessage("===== Attacks ======");
            WriteMessage("Total: {0:N0}", _hitCollection.Sum(x => x.Damage));
            WriteMessage("You: {0:N0}  Ouch: {1:N0}  Heals: {2:N0}",
                _hitCollection.Where(x => x.Attacker.Name == Attack.You).Sum(x => x.Damage),
                _hitCollection.Where(x => x.Defender.Name == Attack.You).Sum(x => x.Damage),
                _healCollection.Where(x => x.Patient.Name == Attack.You || x.Patient.Name == "Khadaji").Sum(x => x.Amount));
            WriteMessage("Pet: {0:N0}  Ouch: {1:N0}",
                _hitCollection.Where(x => x.Attacker.Name == "Khadaji" && x.Attacker.IsPet).Sum(x => x.Damage),
                _hitCollection.Where(x => x.Defender.Name == "Khadaji" && x.Defender.IsPet).Sum(x => x.Damage));
            WriteMessage("Mob: {0:N0}  Ouch: {1:N0}",
                _hitCollection.Where(x => x.Attacker.Name == "a cliknar adept").Sum(x => x.Damage),
                _hitCollection.Where(x => x.Defender.Name == "a cliknar adept").Sum(x => x.Damage));
        }

        private void ParseLines()
        {
            WriteMessage("Starting to parse lines...");
            while (!_jobQueueLogLines.IsCompleted)
            {
                try
                {
                    var logLine = _jobQueueLogLines.Take();
                    _parser.ParseLine(logLine);

                    // if (logLine.LineNumber % 10000 == 0)
                    //     WriteMessage("Parsed {0:N0} lines...", logLine.LineNumber);
                }
                catch (InvalidOperationException) { }
            }
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
