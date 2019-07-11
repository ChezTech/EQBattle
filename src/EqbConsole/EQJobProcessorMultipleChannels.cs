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

namespace EqbConsole
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

        public override void StartProcessingJob(string logFilePath, Battle eqBattle)
        {
            throw new NotImplementedException();
        }

        public async override Task StartProcessingJobAsync(string logFilePath, Battle eqBattle)
        {
            WriteMessage($"Starting to process EQBattle with {_parserCount} parsers. (EQJobProcessorChannels)");

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

                    // if (count % 100 == 0)
                    // {
                    //     WriteMessage($"Read {count} liness");
                    //     Thread.Sleep(_Random.Next(30, 70));
                    // }
                }
            }

            writer.Complete(); // We won't do this if this is a live file that's still being written to (how do we tell? Do we need to know?)

            sw.Stop();
            WriteMessage($"Done reading log file. {count,10:N0} lines {sw.Elapsed} elapsed");
        }

        private async Task ParseLines(ChannelReader<LogDatum> reader, ChannelWriter<ILine> writer, int parserID)
        {
            WriteMessage($"Starting to parse lines [{parserID}]...");
            var sw = Stopwatch.StartNew();
            int count = 0;

            // https://gist.github.com/AlgorithmsAreCool/b0960ce8a3400305e43fe8ffdf89b32c
            // because async methods use a state machine to handle awaits
            // it is safe to await in an infinte loop. Thank you C# compiler gods!
            while (await reader.WaitToReadAsync())
            {
                while (reader.TryRead(out var logLine))
                {
                    var line = _parser.ParseLine(logLine);
                    writer.TryWrite(line);
                    count++;

                    // if (count % 50 == 0)
                    //     WriteMessage($"Parsed {count} lines [{parserID}]");
                }
            }

            sw.Stop();
            WriteMessage($"Done parsing lines [{parserID}]. {count,10:N0} lines {sw.Elapsed} elapsed");
        }

        private async void SortLines(ChannelReader<ILine> reader, ChannelWriter<ILine> writer)
        {
            WriteMessage("Starting to sort lines...");
            var sw = Stopwatch.StartNew();
            int count = 0;

            var lines = new Dictionary<int, ILine>();

            // https://gist.github.com/AlgorithmsAreCool/b0960ce8a3400305e43fe8ffdf89b32c
            // because async methods use a state machine to handle awaits
            // it is safe to await in an infinte loop. Thank you C# compiler gods!
            while (await reader.WaitToReadAsync())
            {
                while (reader.TryRead(out var line))
                {
                    count++;

                    lines.Add(line.LogLine.LineNumber, line);

                    if (lines.Count >= SortBatchSize * 2)
                        SortBatch(lines, writer);



                    // don't sort for now
                    // writer.TryWrite(line);



                    // if (count % 100 == 0)
                    //     WriteMessage($"Sorted {count} lines");
                }
            }

            // Do the remainder of the lines
            SortBatch(lines, writer, true);

            // This creates a bottle neck, but we'll use it to validate
            // var sortedLines = lines
            //     .OrderBy(x => x.LogLine.LineNumber)
            //     // .ToList()
            //     ;

            // foreach (var line in sortedLines)
            //     writer.TryWrite(line);


            writer.Complete();

            sw.Stop();
            WriteMessage($"Done sorting lines. {count,10:N0} lines {sw.Elapsed} elapsed");
        }

        private void SortBatch(Dictionary<int, ILine> lines, ChannelWriter<ILine> writer, bool processAllLines = false)
        {
            // Get a double batch of lines, sort them, then take a single batch and feed it on (to the Battle)
            // Or if we need to process all lines, just sort them all w/o batches
            IEnumerable<ILine> sortedLines = lines.Values;

            if (!processAllLines)
                sortedLines = sortedLines
                    .Take(SortBatchSize * 2);

            sortedLines = sortedLines
                .OrderBy(x => x.LogLine.LineNumber);

            if (!processAllLines)
                sortedLines = sortedLines
                  .Take(SortBatchSize);

            foreach (var line in sortedLines)
            {
                writer.TryWrite(line);
                lines.Remove(line.LogLine.LineNumber); // fast: O(1)
            }
        }

        private async Task AddLinesToBattleAsync(ChannelReader<ILine> reader, Battle eqBattle)
        {
            WriteMessage("Starting adding lines to EQ Battle...");
            var sw = Stopwatch.StartNew();
            int count = 0;

            // https://gist.github.com/AlgorithmsAreCool/b0960ce8a3400305e43fe8ffdf89b32c
            // because async methods use a state machine to handle awaits
            // it is safe to await in an infinte loop. Thank you C# compiler gods!
            while (await reader.WaitToReadAsync())
            {
                while (reader.TryRead(out var line))
                {
                    eqBattle.AddLine(line);
                    count++;

                    // if (count % 100 == 0)
                    //     WriteMessage($"Added {count} lines to Battle");
                }
            }

            sw.Stop();
            WriteMessage($"Done adding lines to EQ Battle. {count,10:N0} lines {sw.Elapsed} elapsed");
        }

        private void WriteMessage(string format, params object[] args) // DI this
        {
            Console.WriteLine("[{0:yyyy-MM-dd HH:mm:ss.fff}] ({1,6}) {2}", DateTime.Now, Thread.CurrentThread.ManagedThreadId, string.Format(format, args));
        }
    }
}
