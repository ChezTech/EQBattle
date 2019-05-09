using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            Console.WriteLine("Reading log file: {0}", logPath);

            var lineCount = 0;

            var sw = Stopwatch.StartNew();
            var parseElapsed = sw.Elapsed;


            Task.Run(() =>
            {
                while (!_jobQueueLogLines.IsCompleted)
                {
                    try
                    {
                        var logLine = _jobQueueLogLines.Take();
                        _parser.ParseLine(logLine);

                        if (logLine.LineNumber % 10000 == 0)
                            Console.WriteLine("Parsed {0} lines...", logLine.LineNumber);
                    }
                    catch (InvalidOperationException) { }
                }
                parseElapsed = sw.Elapsed;

                Console.WriteLine("Done parsing.");
                Console.WriteLine();
            });

            using (LogReader logReader = new LogReader(logPath))
            {
                logReader.LineRead += (s, e) =>
                {
                    lineCount++;
                    var logLine = new LogDatum(e.LogLine, lineCount);
                    _jobQueueLogLines.Add(logLine);
                };
                logReader.EoFReached += (s, e) => { logReader.StopReading(); };
                logReader.StartReading();
            }
            _jobQueueLogLines.CompleteAdding();

            var readElapsed = sw.Elapsed;

            // Keep the console window open while the
            // consumer thread completes its output.
            Console.WriteLine("Press any key to halt parsing. Wait for the 'done' message to finish");
            Console.ReadKey(true);


            Console.WriteLine("Read Elapsed: {0}", readElapsed);
            Console.WriteLine("Parse Elapsed: {0}", parseElapsed);
            Console.WriteLine("Line count: {0:N0}", lineCount);
            Console.WriteLine("Job Queue: {0:N0}", _jobQueueLogLines.Count);

            Console.WriteLine("Line collection count: {0}", _lineCollection.Count);
            Console.WriteLine("Unknown collection count: {0}", _unknownCollection.Count);
            Console.WriteLine("Attack collection count: {0}", _hitCollection.Count);
            Console.WriteLine("Kill collection count: {0}", _killCollection.Count);
            Console.WriteLine("Miss collection count: {0}", _missCollection.Count);

            Console.WriteLine("===== Attacks ======");
            Console.WriteLine("Total: {0:N0}", _hitCollection.Sum(x => x.Damage));
            Console.WriteLine("You: {0:N0}  Ouch: {1:N0}",
                _hitCollection.Where(x => x.Attacker.Name == Attack.You).Sum(x => x.Damage),
                _hitCollection.Where(x => x.Defender.Name == Attack.You).Sum(x => x.Damage));
            Console.WriteLine("Pet: {0:N0}  Ouch: {1:N0}",
                _hitCollection.Where(x => x.Attacker.Name == "Khadaji" && x.Attacker.IsPet).Sum(x => x.Damage),
                _hitCollection.Where(x => x.Defender.Name == "Khadaji" && x.Defender.IsPet).Sum(x => x.Damage));
            Console.WriteLine("Mob: {0:N0}  Ouch: {1:N0}",
                _hitCollection.Where(x => x.Attacker.Name == "a cliknar adept").Sum(x => x.Damage),
                _hitCollection.Where(x => x.Defender.Name == "a cliknar adept").Sum(x => x.Damage));
        }
    }
}
