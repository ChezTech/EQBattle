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

            var sw = Stopwatch.StartNew();
            var parserTasks = new List<Task>();

            // Setup our worker blocks, they won't start until they receive input into their channels
            var parseTask = RunTask(() => ParseLines(_logLinesChannel.Reader, _parsedLinesChannel.Writer));
            var battleTask = RunTask(() => AddLinesToBattleAsync(_parsedLinesChannel.Reader, eqBattle));

            // Start our main guy up, this starts the whole pipeline flow going
            var readTask = RunTask(() => ReadLines(logFilePath, _logLinesChannel.Writer));

            // Wait for everything to finish up
            await readTask;
            await parseTask;
            await battleTask;

            sw.Stop();
            WriteMessage($"Total processing EQBattle. {sw.Elapsed} elapsed");
        }

        private async Task ReadLines(string logPath, ChannelWriter<LogDatum> writer)
        {
            int totalCount = 0;

            using (var fs = File.Open(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            using (var sr = new StreamReader(fs))
            {
                while (!CancelSource.IsCancellationRequested)
                {
                    ReadCurrentSetOfFileLines(sr, writer, out var count);
                    totalCount += count;

                    try
                    {
                        WriteMessage("Sleeping 3s");
                        await Task.Delay(3000, CancelSource.Token); // Ensures early exit if cancelled
                    }
                    catch (TaskCanceledException ex)
                    {
                        WriteMessage($"EX: {ex.Message}");
                    }
                }
            }
            WriteMessage($"Total Lines read: {totalCount}");

            writer.Complete(); // We won't do this if this is a live file that's still being written to (how do we tell? Do we need to know?)
        }

        private void ReadCurrentSetOfFileLines(StreamReader sr, ChannelWriter<LogDatum> writer, out int count)
        {
            string line;
            count = 0;

            while ((line = sr.ReadLine()) != null && !CancelSource.IsCancellationRequested)
            {
                count++;
                if (line == String.Empty)
                    continue;
                var logLine = new LogDatum(line, count);
                writer.TryWrite(logLine);
            }

            WriteMessage($"Lines read: {count}");
        }

        private async Task ParseLines(ChannelReader<LogDatum> reader, ChannelWriter<ILine> writer)
        {
            int count = 0;
            // https://gist.github.com/AlgorithmsAreCool/b0960ce8a3400305e43fe8ffdf89b32c
            // because async methods use a state machine to handle awaits
            // it is safe to await in an infinte loop. Thank you C# compiler gods!
            while (await reader.WaitToReadAsync())
            {
                while (reader.TryRead(out var logLine))
                {
                    count++;
                    var line = _parser.ParseLine(logLine);
                    writer.TryWrite(line);
                }
            }
            WriteMessage($"Lines parsed: {count}");

            writer.Complete();
        }

        private async Task AddLinesToBattleAsync(ChannelReader<ILine> reader, Battle eqBattle)
        {
            int count = 0;
            while (await reader.WaitToReadAsync())
            {
                while (reader.TryRead(out var line))
                {
                    count++;
                    eqBattle.AddLine(line);
                }
            }
            WriteMessage($"Lines added to Battle: {count}");
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
