using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BizObjects.Battle;
using BizObjects.Lines;
using LineParser;
using LineParser.Parsers;
using LogFileReader;
using LogObjects;

namespace EqbConsole
{
    public class EQJobProcessorBlockingCollection
    {
        private readonly string _logPath;
        private BlockingCollection<LogDatum> _jobQueueLogLines = new BlockingCollection<LogDatum>();
        private ConcurrentQueue<ILine> _parsedLines = new ConcurrentQueue<ILine>();
        protected readonly LineParserFactory _parser;

        public Battle EqBattle { get; }
        public int ParserCount { get; set; } = 1;

        public EQJobProcessorBlockingCollection(Battle eqBattle, string logPath)
        {
            EqBattle = eqBattle;
            _logPath = logPath;

            _parser = new LineParserFactory();
            _parser.AddParser(new HitParser(EqBattle.YouAre));
            _parser.AddParser(new MissParser(EqBattle.YouAre));
            _parser.AddParser(new HealParser(EqBattle.YouAre));
            _parser.AddParser(new KillParser(EqBattle.YouAre));
            _parser.AddParser(new WhoParser(EqBattle.YouAre));
        }

        public void StartProcessingJob()
        {
            WriteMessage($"Starting to process EQBattle with {ParserCount} parsers.");

            var swTotal = Stopwatch.StartNew();
            var sw = Stopwatch.StartNew();
            var parserTasks = new List<Task>();

            for (int i = 0; i < ParserCount; i++)
                parserTasks.Add(Task.Run(() => ParseLines()));

            Task.Run(() => ReadLines(_logPath));

            Task.WaitAll(parserTasks.ToArray());
            sw.Stop();
            WriteMessage($"Done parsing lines. {sw.Elapsed} elapsed");

            sw = Stopwatch.StartNew();
            AddParsedLinesToBattle();
            sw.Stop();
            WriteMessage($"Done adding sorted lines. {sw.Elapsed} elapsed");

            WriteMessage($"Done processing EQBattle. {swTotal.Elapsed} elapsed");
        }

        private void ReadLines(string logPath)
        {
            WriteMessage("Reading log file: {0}", logPath);

            var sw = Stopwatch.StartNew();

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
            sw.Stop();

            WriteMessage($"Done reading log file. {count,10:N0} lines {sw.Elapsed} elapsed");
        }

        private void ParseLines()
        {
            WriteMessage("Starting to parse lines...");
            while (!_jobQueueLogLines.IsCompleted)
            {
                try
                {
                    var logLine = _jobQueueLogLines.Take();
                    var line = _parser.ParseLine(logLine);
                    AddLineToCollection(line);
                }
                catch (InvalidOperationException ex)
                {
                    WriteMessage($"ERROR: {ex.Message}");
                }
            }
        }

        private void AddLineToCollection(ILine line)
        {
            // EqBattle.AddLine(line);
            _parsedLines.Enqueue(line);
        }

        private void AddParsedLinesToBattle()
        {
            // Bottleneck here, having to wait for everything to be parsed, then added to the Bag, then sort all at once to add to EQBattle
            // Would like to chunk it up as we go ... get a chunk of 5000 lines, sort, take the first 1000 ... repeat
            var sortedLines = _parsedLines
                .OrderBy(x => x.LogLine.LineNumber)
                .ToList()
                ;

            foreach (var line in sortedLines)
                EqBattle.AddLine(line);
        }

        private void WriteMessage(string format, params object[] args) // DI this
        {
            Console.WriteLine("[{0:yyyy-MM-dd HH:mm:ss.fff}] ({1,6}) {2}", DateTime.Now, Thread.CurrentThread.ManagedThreadId, string.Format(format, args));
        }
    }
}