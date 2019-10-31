using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using BizObjects.Battle;
using BizObjects.Lines;
using LineParser;
using LogObjects;

namespace EQJobService
{
    public class EQJobProcessorMultipleChannels : JobProcessor
    {
        // Thanks: https://gist.github.com/AlgorithmsAreCool/b0960ce8a3400305e43fe8ffdf89b32c

        private const int SortBatchSize = 5000;

        private readonly Channel<LogDatum> _logLinesChannel;
        private readonly Channel<ILine> _parsedLinesChannel;
        private readonly Channel<ILine> _sortedLinesChannel;
        private static Random _Random = new Random();

        public EQJobProcessorMultipleChannels(LineParserFactory parser, int parserCount = 1) : base(parser, parserCount)
        {
            _logLinesChannel = Channel.CreateUnbounded<LogDatum>(new UnboundedChannelOptions()
            {
                SingleWriter = true,
                SingleReader = _parserCount == 1,
            });

            _parsedLinesChannel = Channel.CreateUnbounded<ILine>(new UnboundedChannelOptions()
            {
                SingleWriter = _parserCount == 1,
                SingleReader = true,
            });

            _sortedLinesChannel = Channel.CreateUnbounded<ILine>(new UnboundedChannelOptions()
            {
                SingleWriter = true,
                SingleReader = true,
            });
        }

        public async override Task StartProcessingJobAsync(string logFilePath, Battle eqBattle)
        {
            WriteMessage($"Starting to process EQBattle with {_parserCount} parsers. ({this.GetType().Name})");

            var sw = Stopwatch.StartNew();
            var parserTasks = new List<Task>();

            // Setup our worker blocks, they won't start until they receive input into their channels
            // NOTE: it is barely worth using 2 parsers vs 1 (< 100ms for a 11MB/130k line file). More than 2 parsers is even slower.
            for (int i = 0; i < _parserCount; i++)
            {
                var parserID = i;
                parserTasks.Add(Task.Run(() => ParseLines(_logLinesChannel.Reader, _parsedLinesChannel.Writer, parserID)));
            }

            var sortTask = Task.Run(() => SortLines(_parsedLinesChannel.Reader, _sortedLinesChannel.Writer));
            var battleTask = Task.Run(() => AddLinesToBattleAsync(_sortedLinesChannel.Reader, eqBattle));

            // Start our main guy up, this starts the whole pipeline flow going
            var readTask = Task.Run(() => ReadLines(logFilePath, _logLinesChannel.Writer));

            // Wait for everything to finish up
            await readTask;
            await Task.WhenAll(parserTasks);

            // When the parser tasks are done, mark that channel as complete
            // (Can't do this in the ParseLines method since there can be multiple and it's not complete until they're all done.)
            _parsedLinesChannel.Writer.Complete();

            await sortTask;
            await battleTask;

            sw.Stop();
            WriteMessage($"Total processing EQBattle. {sw.Elapsed} elapsed");
        }

        private void ReadLines(string logPath, ChannelWriter<LogDatum> writer)
        {
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
        }

        private async Task ParseLines(ChannelReader<LogDatum> reader, ChannelWriter<ILine> writer, int parserID)
        {
            // https://gist.github.com/AlgorithmsAreCool/b0960ce8a3400305e43fe8ffdf89b32c
            // because async methods use a state machine to handle awaits
            // it is safe to await in an infinte loop. Thank you C# compiler gods!
            while (await reader.WaitToReadAsync())
            {
                while (reader.TryRead(out var logLine))
                {
                    var line = _parser.ParseLine(logLine);
                    writer.TryWrite(line);
                }
            }
        }

        private async void SortLines(ChannelReader<ILine> reader, ChannelWriter<ILine> writer)
        {
            var lines = new Dictionary<int, ILine>();

            while (await reader.WaitToReadAsync())
            {
                while (reader.TryRead(out var line))
                {
                    if (_parserCount == 1)
                        writer.TryWrite(line);
                    else
                    {
                        lines.Add(line.LogLine.LineNumber, line);

                        if (lines.Count >= SortBatchSize * 2)
                            SortBatch(lines, writer);
                    }
                }
            }

            // Do the remainder of the lines
            if (_parserCount != 1)
                SortBatch(lines, writer, true);

            writer.Complete();
        }

        private void SortBatch(Dictionary<int, ILine> lines, ChannelWriter<ILine> writer, bool processAllLines = false)
        {
            // Get a double batch of lines, sort them, then take a single batch and feed it on (to the Battle)
            // Or if we need to process all lines, just sort them all w/o batches
            IEnumerable<ILine> sortedLines = processAllLines
                ? lines.Values
                    .OrderBy(x => x.LogLine.LineNumber)
                : lines.Values
                    .Take(SortBatchSize * 2)
                    .OrderBy(x => x.LogLine.LineNumber)
                    .Take(SortBatchSize);

            foreach (var line in sortedLines)
            {
                writer.TryWrite(line);
                lines.Remove(line.LogLine.LineNumber); // fast: O(1)
            }
        }

        private async Task AddLinesToBattleAsync(ChannelReader<ILine> reader, Battle eqBattle)
        {
            while (await reader.WaitToReadAsync())
            {
                while (reader.TryRead(out var line))
                {
                    eqBattle.AddLine(line);
                }
            }
        }

        private void WriteMessage(string format, params object[] args) // DI this
        {
            Console.WriteLine("[{0:yyyy-MM-dd HH:mm:ss.fff}] ({1,6}) {2}", DateTime.Now, Thread.CurrentThread.ManagedThreadId, string.Format(format, args));
        }
    }
}
