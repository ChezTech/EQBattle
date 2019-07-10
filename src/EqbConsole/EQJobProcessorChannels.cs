using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using BizObjects.Battle;
using BizObjects.Lines;
using LineParser;
using LogObjects;

namespace EqbConsole
{
    public class EQJobProcessorChannels : JobProcessor
    {
        // Thanks: https://gist.github.com/AlgorithmsAreCool/b0960ce8a3400305e43fe8ffdf89b32c

        private ConcurrentQueue<ILine> _parsedLines = new ConcurrentQueue<ILine>();

        public EQJobProcessorChannels(LineParserFactory parser, int parserCount = 1) : base(parser, parserCount)
        {
        }

        public override void StartProcessingJob(string logFilePath, Battle eqBattle)
        {
        }

        public async Task StartProcessingJobAsync(string logFilePath, Battle eqBattle)
        {
            WriteMessage($"Starting to process EQBattle with {_parserCount} parsers. (EQJobProcessorChannels)");

            var swTotal = Stopwatch.StartNew();
            var logLinesChannel = Channel.CreateUnbounded<LogDatum>(new UnboundedChannelOptions()
            {
                SingleWriter = true,
                SingleReader = _parserCount == 1,
            });

            var sw = Stopwatch.StartNew();
            var parserTasks = new List<Task>();

            for (int i = 0; i < _parserCount; i++)
                parserTasks.Add(Task.Run(() => ParseLines(logLinesChannel.Reader)));

            var readLinesTask = Task.Run(() => ReadLines(logFilePath, logLinesChannel.Writer));

            // Can start other code here while the ReadLines task starts going.
            // The readers/ParseLine are already waiting and will spin up

            // This is where we can put the batch sorter that then feeds into the Battle
            // (currently, this is done after the Read/Parse is complete, the await just below.)

            await readLinesTask;
            Task.WaitAll(parserTasks.ToArray());

            sw.Stop();
            WriteMessage($"Done parsing lines. {sw.Elapsed} elapsed");

            sw = Stopwatch.StartNew();
            AddParsedLinesToBattle(eqBattle);
            sw.Stop();
            WriteMessage($"Done adding sorted lines. {sw.Elapsed} elapsed");

            WriteMessage($"Total processing EQBattle. {swTotal.Elapsed} elapsed");
        }

        private void ReadLines(string logPath, ChannelWriter<LogDatum> writer)
        {
            WriteMessage("Reading log file: {0}", logPath);

            var sw = Stopwatch.StartNew();

            int count = 0;

            using (var fs = File.Open(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            using (var sr = new StreamReader(fs))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    count++;
                    if (line == String.Empty)
                        continue;
                    var logLine = new LogDatum(line, count);
                    writer.TryWrite(logLine);
                }
            }

            writer.Complete(); // We won't do this if this is a live file that's still being written to (how do we tell? Do we need to know?)
            sw.Stop();

            WriteMessage($"Done reading log file. {count,10:N0} lines {sw.Elapsed} elapsed");
        }

        private async Task ParseLines(ChannelReader<LogDatum> reader)
        {
            WriteMessage("Starting to parse lines...");

            // https://gist.github.com/AlgorithmsAreCool/b0960ce8a3400305e43fe8ffdf89b32c
            // because async methods use a state machine to handle awaits
            // it is safe to await in an infinte loop. Thank you C# compiler gods! 
            while (await reader.WaitToReadAsync())
            {
                while (reader.TryRead(out var logLine))
                {
                    var line = _parser.ParseLine(logLine);
                    AddLineToCollection(line);
                }
            }
        }

        private void AddLineToCollection(ILine line)
        {
            _parsedLines.Enqueue(line);
        }

        private void AddParsedLinesToBattle(Battle eqBattle)
        {
            // Bottleneck here, having to wait for everything to be parsed, then added to the Bag, then sort all at once to add to EQBattle
            // Would like to chunk it up as we go ... get a chunk of 5000 lines, sort, take the first 1000 ... repeat
            var sortedLines = _parsedLines
                .OrderBy(x => x.LogLine.LineNumber)
                .ToList()
                ;

            foreach (var line in sortedLines)
                eqBattle.AddLine(line);
        }

        private void WriteMessage(string format, params object[] args) // DI this
        {
            Console.WriteLine("[{0:yyyy-MM-dd HH:mm:ss.fff}] ({1,6}) {2}", DateTime.Now, Thread.CurrentThread.ManagedThreadId, string.Format(format, args));
        }
    }
}