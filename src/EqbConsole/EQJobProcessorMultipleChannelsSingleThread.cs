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

        private Stopwatch _sw = new Stopwatch();
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
            // var parseTask = Task.Run(() => ParseLines(_logLinesChannel.Reader, _parsedLinesChannel.Writer));
            // var battleTask = Task.Run(() => AddLinesToBattleAsync(_parsedLinesChannel.Reader, eqBattle));

            var parseTask = Task.Run(async () =>
            {
                await ChannelReadAsync(_logLinesChannel.Reader, line => ParseLine(line, _parsedLinesChannel.Writer), "ParseLine");
                _parsedLinesChannel.Writer.Complete();
                WriteMessage($"Total Lines parsed: {_parsedLineCount:N0}, {_sw.Elapsed} elapsed");
            });

            var battleTask = Task.Run(async () =>
            {
                await ChannelReadAsync(_parsedLinesChannel.Reader, line => AddLineToBattle(line, eqBattle), "AddLineToBattle");
                WriteMessage($"Total Lines added to Battle: {_battleLinesCount:N0}, {_sw.Elapsed} elapsed");
            });


            // Start our main guy up, this starts the whole pipeline flow going
            var readTask = Task.Run(() => ReadLines(logFilePath, _logLinesChannel.Writer));

            // Wait for everything to finish up
            await Task.WhenAll(readTask, parseTask, battleTask);

            WriteMessage($"Total processing EQBattle, {_sw.Elapsed} elapsed");
            WriteMessage($"LastLineNumber read: {_lastLineNumber:N0}");
            WriteMessage($"LastLineNumber added to Battle: {_lastLineAddedToBattle?.LogLine.LineNumber:N0}");
        }

        private async Task ReadLines(string logPath, ChannelWriter<LogDatum> writer)
        {
            _sw.Start();

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
                        // Console.Write(".");
                        await Task.Delay(1000, CancelSource.Token); // Ensures early exit if cancelled
                    }
                    catch (Exception ex) when (ex is ChannelClosedException || ex is OperationCanceledException || ex is TaskCanceledException)
                    {
                        WriteMessage($"ERROR: ReadLines - {ex.Message}, {ex.GetType()}");
                    }
                }
            }
            WriteMessage($"Total Lines read: {totalCount:N0}");

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

                // Track so we can print debug message in other channels
                _lastLineNumber = logLine.LineNumber;
            }

            if (sessionCount > 0)
                WriteMessage($"Lines read: {count:N0} / {sessionCount:N0}, {_sw.Elapsed} elapsed");

            return count;
        }

        private async Task ParseLines(ChannelReader<LogDatum> reader, ChannelWriter<ILine> writer)
        {
            int count = 0;
            try
            {
                // https://gist.github.com/AlgorithmsAreCool/b0960ce8a3400305e43fe8ffdf89b32c
                // because async methods use a state machine to handle awaits
                // it is safe to await in an infinte loop. Thank you C# compiler gods!

                // while (await reader.WaitToReadAsync() && !CancelSource.IsCancellationRequested)
                // {
                //     while (reader.TryRead(out var logLine) && !CancelSource.IsCancellationRequested)

                // while (await reader.WaitToReadAsync(CancelSource.Token))
                // {
                //     while (!CancelSource.IsCancellationRequested && reader.TryRead(out var logLine))
                //     {
                //         ParseLine(logLine, writer);
                //     }
                //     WriteMessage($"Lines parsed: {count:N0}, {_sw.Elapsed} elapsed");
                // }

                while (true)
                {
                    var logLine = await reader.ReadAsync(CancelSource.Token);
                    ParseLine(logLine, writer);
                }
            }
            catch (Exception ex)
            {
                WriteMessage($"Parse ERROR: {ex.Message}, {ex.GetType()}");
            }
            finally
            {
                writer.Complete();
                WriteMessage($"Total Lines parsed: {count:N0}, {_sw.Elapsed} elapsed");
            }
        }

        private int _parsedLineCount = 0;
        private void ParseLine(LogDatum logLine, ChannelWriter<ILine> writer)
        {
            _parsedLineCount++;
            var line = _parser.ParseLine(logLine);
            writer.TryWrite(line);

            // if (_parsedLineCount % 10000 == 0)
            //     WriteMessage($"Lines parsed: {_parsedLineCount:N0}, {_sw.Elapsed} elapsed");

            if (line.LogLine.LineNumber == _lastLineNumber)
                WriteMessage($"Lines parsed: {_parsedLineCount:N0}, {_sw.Elapsed} elapsed");
        }

        private async Task AddLinesToBattleAsync(ChannelReader<ILine> reader, Battle eqBattle)
        {
            ILine line = null;
            try
            {
                int count = 0;
                // while (await reader.WaitToReadAsync() && !CancelSource.IsCancellationRequested)
                // {
                //     while (reader.TryRead(out var line) && !CancelSource.IsCancellationRequested)

                while (await reader.WaitToReadAsync(CancelSource.Token))
                {
                    while (!CancelSource.IsCancellationRequested && reader.TryRead(out line))
                    {
                        AddLineToBattle(line, eqBattle);


                        // This gets called too many times since the ParseLines() method is the bottleneck, which means the parsed lines comes in small spurts into this channel
                        // Just update when we process the last line we've gotten (so far)
                        if (line.LogLine.LineNumber == _lastLineNumber)
                            WriteMessage($"Lines added to Battle: {count:N0}, {_sw.Elapsed} elapsed");
                    }
                }
            }
            catch (Exception ex)
            {
                WriteMessage($"Battle ERROR: {ex.Message}, {ex.GetType()}");
                WriteMessage(ex.StackTrace);
            }
            finally
            {
                WriteMessage($"Total Lines added to Battle: {_battleLinesCount:N0}, {_sw.Elapsed} elapsed");
                WriteMessage($"LastLineNumber added to Battle: {line?.LogLine.LineNumber}");
            }
        }

        private async Task ChannelRead<T>(Func<ChannelReader<T>, Action<T>, Task> readLoop, ChannelReader<T> reader, Action<T> action, string title)
        {
            try
            {
                await readLoop(reader, action);
            }
            catch (Exception ex) when (ex is ChannelClosedException || ex is OperationCanceledException || ex is TaskCanceledException)
            {
                WriteMessage($"ERROR: {title} - {ex.Message}, {ex.GetType()}");
            }
        }

        private async Task ChannelReadAsync<T>(ChannelReader<T> reader, Action<T> action, string title)
        {
            await ChannelRead(async (r, a) =>
            {
                while (true)
                {
                    var item = await reader.ReadAsync(CancelSource.Token);
                    action(item);
                }
            }, reader, action, title);
        }

        private async Task ChannelWaitToReadAsync<T>(ChannelReader<T> reader, Action<T> action, string title)
        {
            await ChannelRead(async (r, a) =>
            {
                while (await reader.WaitToReadAsync(CancelSource.Token))
                {
                    while (!CancelSource.IsCancellationRequested && reader.TryRead(out var item))
                    {
                        action(item);
                    }
                }
            }, reader, action, title);
        }

        private int _battleLinesCount = 0;
        private ILine _lastLineAddedToBattle = null;
        private void AddLineToBattle(ILine line, Battle eqBattle)
        {
            _battleLinesCount++;
            eqBattle.AddLine(line);
            _lastLineAddedToBattle = line;

            // if (_battleLinesCount % 10000 == 0)
            //     WriteMessage($"Lines added to Battle: {_battleLinesCount:N0}, {_sw.Elapsed} elapsed");

            if (line.LogLine.LineNumber == _lastLineNumber)
                WriteMessage($"Lines added to Battle: {_battleLinesCount:N0}, {_sw.Elapsed} elapsed");
        }
        private void WriteMessage(string format, params object[] args) // DI this
        {
            Console.WriteLine("[{0:yyyy-MM-dd HH:mm:ss.fff}] ({1,6}) {2}", DateTime.Now, Thread.CurrentThread.ManagedThreadId, string.Format(format, args));
        }
    }
}
