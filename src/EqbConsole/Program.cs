using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BizObjects;
using BizObjects.Parsers;
using LineParser;
using LogFileReader;
using LogObjects;

namespace EqbConsole
{
    class Program
    {
        private const string LogFilePathName = @"C:\Program Files (x86)\Steam\steamapps\common\Everquest F2P\Logs\eqlog_Khadaji_erollisi_test.txt";
        //private const string LogFilePathName = @"C:\Program Files (x86)\Steam\steamapps\common\Everquest F2P\Logs\eqlog_Khadaji_erollisi_2019-03-25-182700.txt";

        private LineParserFactory _parser;
        private List<ILine> _lineCollection = new List<ILine>();
        private List<Unknown> _unknownCollection = new List<Unknown>();
        private List<Hit> _hitCollection = new List<Hit>();
        private List<Kill> _killCollection = new List<Kill>();

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
        }

        private void RunProgram(string logPath)
        {
            Console.WriteLine("Reading log file: {0}", logPath);

            var lineCount = 0;

            var sw = Stopwatch.StartNew();

            using (LogReader logReader = new LogReader(logPath))
            {
                logReader.LineRead += (s, e) =>
                {
                    lineCount++;
                    var logLine = new LogDatum(e.LogLine, lineCount);
                    // Add this to a job queue

                    _parser.ParseLine(logLine);
                };
                logReader.EoFReached += (s, e) => { logReader.StopReading(); };
                logReader.StartReading();
            }

            sw.Stop();

            Console.WriteLine("Elapsed: {0}", sw.Elapsed);
            Console.WriteLine("Line count: {0}", lineCount);
            Console.WriteLine("Line collection count: {0}", _lineCollection.Count);
            Console.WriteLine("Unknown collection count: {0}", _unknownCollection.Count);
            Console.WriteLine("Attack collection count: {0}", _hitCollection.Count);
            Console.WriteLine("Kill collection count: {0}", _killCollection.Count);

            Console.WriteLine("===== Attacks ======");
            Console.WriteLine("Total: {0:N0}", _hitCollection.Sum(x => x.Damage));
            Console.WriteLine("You: {0:N0}  Ouch: {1:N0}",
                _hitCollection.Where(x => x.Attacker == Attack.You).Sum(x => x.Damage),
                _hitCollection.Where(x => x.Defender == Attack.You).Sum(x => x.Damage));
            Console.WriteLine("Pet: {0:N0}  Ouch: {1:N0}",
                _hitCollection.Where(x => x.Attacker == "Khadaji" && x.IsPet).Sum(x => x.Damage),
                _hitCollection.Where(x => x.Defender == "Khadaji" && x.IsPet).Sum(x => x.Damage));
            Console.WriteLine("Mob: {0:N0}  Ouch: {1:N0}",
                _hitCollection.Where(x => x.Attacker == "a cliknar adept").Sum(x => x.Damage),
                _hitCollection.Where(x => x.Defender == "a cliknar adept").Sum(x => x.Damage));
        }
    }
}
