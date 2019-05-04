using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private Publisher _publisher = new Publisher();
        private LineParserFactory _parser;
        private List<ILine> _lineCollection = new List<ILine>();
        private List<Unknown> _unknownCollection = new List<Unknown>();
        private List<Attack> _attackCollection = new List<Attack>();
        private List<Kill> _killCollection = new List<Kill>();

        static void Main(string[] args)
        {
            new Program().RunProgram();
        }

        private Program()
        {
            _parser = new LineParserFactory(_publisher);
            _parser.AddParser(new KillParser(), x => { Console.WriteLine(x.ToString()); });
            _parser.AddParser(new HitParser(), x => {  });

            _publisher.LineCreated += x => _lineCollection.Add(x);
            _publisher.UnknownCreated += x => _unknownCollection.Add(x);
            _publisher.AttackCreated += x => _attackCollection.Add(x);
            _publisher.KillCreated += x => _killCollection.Add(x);
        }

        private void RunProgram()
        {
            Console.WriteLine("Reading log file: {0}", LogFilePathName);

            var lineCount = 0;

            var sw = Stopwatch.StartNew();

            using (LogReader logReader = new LogReader(LogFilePathName))
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
            Console.WriteLine("Attack collection count: {0}", _attackCollection.Count);
            Console.WriteLine("Kill collection count: {0}", _killCollection.Count);

            Console.WriteLine("===== Kills ======");
            foreach (var item in _killCollection)
            {
                Console.WriteLine("Kill: '{0}' killed '{1}'", item.Attacker, item.Defender);
            }
        }
    }
}
