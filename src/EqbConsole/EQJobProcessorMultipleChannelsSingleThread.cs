using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using BizObjects.Battle;
using BizObjects.Lines;
using LineParser;
using LogObjects;

namespace EqbConsole
{
    public class EQJobProcessorMultipleChannelsSingleThread : JobProcessor
    {
        // Thanks: https://gist.github.com/AlgorithmsAreCool/b0960ce8a3400305e43fe8ffdf89b32c

        private readonly Channel<LogDatum> _logLinesChannel;
        private readonly Channel<ILine> _parsedLinesChannel;

        private Stopwatch _sw;
        private int _lastLineNumber = 0;
        private int _rawLineCount = 0;

        public EQJobProcessorMultipleChannelsSingleThread(LineParserFactory parser, int parserCount = 1) : base(parser, parserCount)
        {
            _logLinesChannel = Channel.CreateUnbounded<LogDatum>(new UnboundedChannelOptions()
            {
                SingleWriter = true,
                SingleReader = true,
            });

            _parsedLinesChannel = Channel.CreateUnbounded<ILine>(new UnboundedChannelOptions()
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
            WriteMessage($"Starting to process EQBattle with {_parserCount} parsers. (EQJobProcessorMultipleChannelsSingleThread)");

            // Setup our worker blocks, they won't start until they receive input into their channels
            // var parseTask = RunTask(() => ParseLines(_logLinesChannel.Reader, _parsedLinesChannel.Writer));
            // var battleTask = RunTask(() => AddLinesToBattleAsync(_parsedLinesChannel.Reader, eqBattle));

            // Start our main guy up, this starts the whole pipeline flow going
            // var readTask = RunTask(() => ReadLines(logFilePath, _logLinesChannel.Writer));


            // // Setup our worker blocks, they'll wait until they receive input into their channels
            // var parseTask = ParseLines(_logLinesChannel.Reader, _parsedLinesChannel.Writer);
            // var battleTask = AddLinesToBattleAsync(_parsedLinesChannel.Reader, eqBattle);

            // // Start our main guy up, this starts the whole pipeline flow going
            var readTask = ReadLines(logFilePath, _logLinesChannel.Writer);

            // var readTask = WaitingTask(logFilePath, _logLinesChannel.Writer);

            // Wait for everything to finish up
            // await Task.WhenAll(readTask, parseTask, battleTask);
            await Task.WhenAll(readTask);

            WriteMessage($"Total processing EQBattle, {_sw.Elapsed} elapsed");
        }

        private async Task WaitingTask(string logPath, ChannelWriter<LogDatum> writer)
        {

            WriteMessage("Starting WaitingTask");
            int i = 0;


            using (var fs = File.Open(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            using (var sr = new StreamReader(fs))
            {
                while (!CancelSource.IsCancellationRequested && i < 4)
                {
                    i++;
                    WriteMessage($"In WaitingTask: {i}");

                    // Do some work here
                    ReadCurrentSetOfFileLines(sr, writer);

                    try
                    {
                        await Task.Delay(2000, CancelSource.Token); // Ensures early exit if cancelled
                    }
                    catch (TaskCanceledException ex)
                    {
                        WriteMessage($"EX: {ex.Message}");
                    }
                }
            }
            WriteMessage("Finished WaitingTask");
        }

        private async Task ReadLines(string logPath, ChannelWriter<LogDatum> writer)
        {
            _sw = Stopwatch.StartNew();

            int totalCount = 0;

            using (var fs = File.Open(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            using (var sr = new StreamReader(fs))
            {
                // This sets up our infinite loop of reading the file for new changes (until cancelled)
                while (!CancelSource.IsCancellationRequested)
                {
                    var count = ReadCurrentSetOfFileLines(sr, writer);
                    totalCount += count;

                    try
                    {
                        // WriteMessage("Sleeping 3s");
                        await Task.Delay(500, CancelSource.Token); // Ensures early exit if cancelled
                    }
                    catch (TaskCanceledException ex)
                    {
                        WriteMessage($"ReadLine EX: {ex.Message}");
                    }
                }
            }
            WriteMessage($"Total Lines read: {totalCount:N0}");
            WriteMessage($"IsCancelled? {CancelSource.IsCancellationRequested}");

            // We've been cancelled at this point, close out the channel
            writer.Complete();
        }

        private int ReadCurrentSetOfFileLines(StreamReader sr, ChannelWriter<LogDatum> writer)
        {
            string line;
            int sessionCount = 0;
            var count = 0;

            while ((line = sr.ReadLine()) != null && !CancelSource.IsCancellationRequested)
            {
                _rawLineCount++;
                sessionCount++;

                if (line == String.Empty)
                    continue;

                count++;

                var logLine = new LogDatum(line, _rawLineCount);
                writer.TryWrite(logLine);
            }

            // Track so we can print debug message in other channels
            _lastLineNumber = _rawLineCount;

            if (sessionCount > 0)
                WriteMessage($"Lines read: {count:N0} / {sessionCount:N0}, {_sw.Elapsed} elapsed");

            return count;
        }

        private async Task ParseLines(ChannelReader<LogDatum> reader, ChannelWriter<ILine> writer)
        {
            int count = 0;
            // https://gist.github.com/AlgorithmsAreCool/b0960ce8a3400305e43fe8ffdf89b32c
            // because async methods use a state machine to handle awaits
            // it is safe to await in an infinte loop. Thank you C# compiler gods!

            // while (await reader.WaitToReadAsync() && !CancelSource.IsCancellationRequested)
            // {
            //     while (reader.TryRead(out var logLine) && !CancelSource.IsCancellationRequested)

            while (await reader.WaitToReadAsync())
            {
                while (reader.TryRead(out var logLine))
                {
                    count++;
                    var line = _parser.ParseLine(logLine);
                    writer.TryWrite(line);
                }
                // WriteMessage($"Lines parsed: {count:N0}, {_sw.Elapsed} elapsed");
            }
            WriteMessage($"Total Lines parsed: {count:N0}");

            writer.Complete();
        }

        private async Task AddLinesToBattleAsync(ChannelReader<ILine> reader, Battle eqBattle)
        {
            int count = 0;
            // while (await reader.WaitToReadAsync() && !CancelSource.IsCancellationRequested)
            // {
            //     while (reader.TryRead(out var line) && !CancelSource.IsCancellationRequested)

            while (await reader.WaitToReadAsync())
            {
                ILine line;
                while (reader.TryRead(out line))
                {
                    count++;
                    eqBattle.AddLine(line);
                }
                // This gets called too many times since the ParseLines() method is the bottleneck, which means the parsed lines comes in small spurts into this channel
                // Just update when we process the last line we've gotten (so far)
                if (line.LogLine.LineNumber == _lastLineNumber)
                    WriteMessage($"Lines added to Battle: {count:N0}, {_sw.Elapsed} elapsed");
            }
            WriteMessage($"Total Lines added to Battle: {count:N0}");
        }

        /// <summary>
        /// Run a task using our CancellationTokenSource
        /// </summary>
        private Task RunTask(Func<Task> runnable)
        {
            return Task.Factory.StartNew(async () =>
            {
                try
                {
                    await runnable();
                }
                catch (Exception ex)
                {
                    WriteMessage(ex.Message);
                }
            }, CancelSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private Task RunTask(Action runnable)
        {
            return Task.Factory.StartNew(() =>
            {
                runnable();
            }, CancelSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private void WriteMessage(string format, params object[] args) // DI this
        {
            Console.WriteLine("[{0:yyyy-MM-dd HH:mm:ss.fff}] ({1,6}) {2}", DateTime.Now, Thread.CurrentThread.ManagedThreadId, string.Format(format, args));
        }
    }
}
