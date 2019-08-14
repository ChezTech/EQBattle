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
using LogFileReader;
using LogObjects;

namespace EqbConsole
{
    public class EQJobEvenMoreChannels : JobProcessor
    {
        // Thanks: https://gist.github.com/AlgorithmsAreCool/b0960ce8a3400305e43fe8ffdf89b32c

        private readonly Channel<string> _rawLinesChannel;
        private readonly Channel<LogDatum> _logLinesChannel;
        private readonly Channel<ILine> _parsedLinesChannel;
        private FileInfo LogFile { get; set; }

        private Stopwatch _sw = new Stopwatch();
        private int _lastLineNumber = 0;
        private int _rawLineCount = 0;
        private int _datumLineCount = 0;
        private int _parsedLineCount = 0;
        private int _battleLinesCount = 0;
        private ILine _lastLineAddedToBattle = null;


        /// <Summary>
        /// Keep watching log file for updates after reading through once.
        /// </Summary>
        public bool WatchFile { get; set; } = true;



        public EQJobEvenMoreChannels(LineParserFactory parser) : base(parser)
        {
            _rawLinesChannel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions()
            {
                SingleWriter = true,
                SingleReader = true,
            });

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

        public override void ShowStatus()
        {
            WriteMessage($"Job status: log lines read: {_rawLineCount,10:N0}, datum'd: {_datumLineCount,10:N0}, parsed {_parsedLineCount,10:N0}, battled: {_battleLinesCount,10:N0}, elapsed: {_sw.Elapsed}");
        }

        public async override Task StartProcessingJobAsync(string logFilePath, Battle eqBattle)
        {
            WatchFile = false;

            // LogFile = new FileInfo(logFilePath);

            // if (!LogFile.Exists)
            //     throw new FileNotFoundException($"File not found: {logFilePath}", logFilePath);

            WriteMessage($"Starting to process EQBattle. ({this.GetType().Name})");

            // Trying a new CTS just for these jobs, separate from the this.CancelSource
            using (var ctSource = new CancellationTokenSource())
            {
                // var ctSource = CancelSource;
                // Setup our worker blocks, they won't start until they receive input into their channels
                // var parseTask = Task.Run(() => ParseLines(_logLinesChannel.Reader, _parsedLinesChannel.Writer));
                // var battleTask = Task.Run(() => AddLinesToBattleAsync(_parsedLinesChannel.Reader, eqBattle));

                // var datumTask = Task.Run(async () =>
                // {
                //     // await ChannelReadAsync(_logLinesChannel.Reader, line => ParseLine(line, _parsedLinesChannel.Writer), "ParseLine");
                //     await ChannelWaitToReadAsync(_rawLinesChannel.Reader, line => DatumLine(line, _logLinesChannel.Writer), "DatumLine");
                //     _logLinesChannel.Writer.Complete();
                //     WriteMessage($"Total Lines datum'd: {_datumLineCount:N0}, {_sw.Elapsed} elapsed");
                // });

                // var parseTask = Task.Run(async () =>
                // {
                //     // await ChannelReadAsync(_logLinesChannel.Reader, line => ParseLine(line, _parsedLinesChannel.Writer), "ParseLine");
                //     await ChannelWaitToReadAsync(_logLinesChannel.Reader, line => ParseLine(line, _parsedLinesChannel.Writer), "ParseLine");
                //     _parsedLinesChannel.Writer.Complete();
                //     WriteMessage($"Total Lines parsed: {_parsedLineCount:N0}, {_sw.Elapsed} elapsed");
                // });

                // var battleTask = Task.Run(async () =>
                // {
                //     // await ChannelReadAsync(_parsedLinesChannel.Reader, line => AddLineToBattle(line, eqBattle), "AddLineToBattle");
                //     await ChannelWaitToReadAsync(_parsedLinesChannel.Reader, line => AddLineToBattle(line, eqBattle), "AddLineToBattle");
                //     WriteMessage($"Total Lines added to Battle: {_battleLinesCount:N0}, {_sw.Elapsed} elapsed");
                // });


                // // Start our main guy up, this starts the whole pipeline flow going
                // var readTask = Task.Run(() => ReadLines(logFilePath, _rawLinesChannel.Writer));



                // var cp = new ChannelProcessor(CancelSource.Token);
                var cp = new ChannelProcessor(ctSource.Token);

                // var cancelTask = CheckForCancel();
                var cancelTask = Task.Delay(-1, CancelSource.Token);
                var ctCancel = cancelTask.ContinueWith(_ =>
                {
                    WriteMessage("CancelTask cancelling...");

                    // Mark all the channels complete.
                    // Marking the first will cause them all to trickle down and close.
                    // However the parser is the bottle neck, so if we want them to close promptly (and abandon any raw log lines in the parser channel queue), we need to close them all here.
                    // _rawLinesChannel.Writer.TryComplete();
                    // _logLinesChannel.Writer.TryComplete();
                    // _parsedLinesChannel.Writer.TryComplete();

                    // Cancel our Token, this will cancel the Log Reader and close its channel (which will cause all others to close)
                    ctSource.Cancel();

                    WriteMessage("CancelTask cancelled");
                }, TaskContinuationOptions.OnlyOnCanceled);

                cp.Message += m => WriteMessage(m);
                var datumTask = cp.Process(_rawLinesChannel.Reader, _logLinesChannel.Writer, item => TransformLogLineToDatum(item), "Datum");
                var dtCancel = datumTask.ContinueWith(_ => WriteMessage("DatumTask cancelled"), TaskContinuationOptions.OnlyOnCanceled);
                var dtComplete = datumTask.ContinueWith(_ => WriteMessage("DatumTask complete"), TaskContinuationOptions.OnlyOnRanToCompletion);
                var dtContinue = datumTask.ContinueWith(_ =>
                {
                    WriteMessage($"DatumTask done. {_datumLineCount:N0} datum lines, {_sw.Elapsed} elapsed");
                    _logLinesChannel.Writer.TryComplete();
                });

                var parseTask = cp.Process(_logLinesChannel.Reader, _parsedLinesChannel.Writer, item => TransformDatumToLine(item), "Parser");
                var ptCancel = parseTask.ContinueWith(_ => WriteMessage("ParseTask cancelled"), TaskContinuationOptions.OnlyOnCanceled);
                var ptComplete = parseTask.ContinueWith(_ => WriteMessage("ParseTask complete"), TaskContinuationOptions.OnlyOnRanToCompletion);
                var ptContinue = parseTask.ContinueWith(_ =>
                {
                    WriteMessage($"ParseTask done. {_parsedLineCount:N0} parse lines, {_sw.Elapsed} elapsed");
                    _parsedLinesChannel.Writer.TryComplete();
                });

                var battleTask = cp.Process(_parsedLinesChannel.Reader, item => AddLineToBattle(item, eqBattle), "Battle");
                var btCancel = battleTask.ContinueWith(_ => WriteMessage("BattleTask cancelled"), TaskContinuationOptions.OnlyOnCanceled);
                var btComplete = battleTask.ContinueWith(_ => WriteMessage("BattleTask complete"), TaskContinuationOptions.OnlyOnRanToCompletion);
                var btContinue = battleTask.ContinueWith(_ =>
                {
                    WriteMessage($"BattleTask done. {_battleLinesCount:N0} battle lines, {_sw.Elapsed} elapsed");
                });


                // WriteMessage("Waiting 1 ...");
                // await Task.Delay(4000, ctSource.Token);
                // WriteMessage("Done waiting 1");


                // Start our main guy up, this starts the whole pipeline flow going
                _sw.Start();
                // var readTask = cp.Process(_rawLinesChannel.Writer, ct => ReadLogLines(logFilePath, ct));
                var readTask = ReadLogLinesWrapper(_rawLinesChannel.Writer, logFilePath, ctSource.Token, 10 * 1000);
                var rtCancel = readTask.ContinueWith(_ => WriteMessage("ReadTask cancelled"), TaskContinuationOptions.OnlyOnCanceled);
                var rtComplete = readTask.ContinueWith(_ => WriteMessage("ReadTask complete"), TaskContinuationOptions.OnlyOnRanToCompletion);
                var rtContinue = readTask.ContinueWith(_ =>
                {
                    WriteMessage($"ReadTask done. {_rawLineCount:N0} read lines, {_sw.Elapsed} elapsed");
                    _rawLinesChannel.Writer.TryComplete();
                });



                try
                {
                    // WriteMessage("Waiting 2");
                    // await Program.Delay(3000, ctSource.Token, "Waiting 2");
                    // WriteMessage("Done waiting 2");


                    // Wait for everything to finish up
                    await Task.WhenAll(readTask, datumTask, parseTask, battleTask);

                    WriteMessage("All tasks that we're waiting for are done");
                }
                // catch (TaskCanceledException ex)
                // {
                //     WriteMessage($"{ex.GetType().Name} - {ex.Message}");
                // }
                catch (OperationCanceledException ex)
                {
                    WriteMessage($"Operation Cancelled: {ex.GetType().Name} - {ex.Message}\n{ex}");
                    // throw;
                }
                catch (Exception ex)
                {
                    WriteMessage($"Job Exception: {ex.GetType().Name} - {ex.Message}\n{ex}");
                    WriteMessage($"Inner: {ex.InnerException}");
                }
                finally
                {
                    WriteMessage("Job done");
                    await Task.Delay(20); // Give all continuation tasks a chance to finish

                    Program.DumpTaskInfo(readTask, "readTask");
                    Program.DumpTaskInfo(datumTask, "datumTask");
                    Program.DumpTaskInfo(parseTask, "parseTask");
                    Program.DumpTaskInfo(battleTask, "battleTask");

                    DumpChannelInfo(_rawLinesChannel, "_rawLinesChannel");
                    DumpChannelInfo(_logLinesChannel, "_logLinesChannel");
                    DumpChannelInfo(_parsedLinesChannel, "_parsedLinesChannel");
                }
            }

            WriteMessage($"Total processing EQBattle, {_sw.Elapsed} elapsed");
            WriteMessage($"LastLineNumber read: {_lastLineNumber:N0}");
            WriteMessage($"LastLineNumber added to Battle: {_lastLineAddedToBattle?.LogLine.LineNumber:N0}");
            ShowStatus();

            CancelSource.Token.ThrowIfCancellationRequested();
        }

        public static void DumpChannelInfo<T>(Channel<T> channel, string title)
        {
            WriteMessage($"Channel: {title,-20} reader status: {channel.Reader.Completion.Status}");
        }

        private async Task ReadLogLinesWrapper(ChannelWriter<string> writer, string logPath, CancellationToken token, int delayTimeMs = 250)
        {
            WriteMessage("Writing channel: Reader");
            var startingLineCount = 0;

            var lr = new LogReader(logPath, token);
            lr.LineRead += line =>
            {
                _rawLineCount++;
                writer.TryWrite(line);
            };

            // int delayCount = 0;
            // lr.EoFReached += async () => await Delay(delayTimeMs, token);
            lr.EoFReached += async () =>
            {
                // delayCount++;
                // if (delayCount <= 4)
                // {
                var eofLineCount = _rawLineCount - startingLineCount;
                if (eofLineCount > 0)
                    WriteMessage($"LogReader EOF - waiting to read next chunk. {_rawLineCount:N0} read lines, {_sw.Elapsed} elapsed");

                startingLineCount = _rawLineCount;

                if (WatchFile)
                    await Program.Delay(5000, token, "EOF");
                else
                    CancelSource.Cancel(); // Not the best way I don't think. Perhaps just a return of 'False'

                //     // Thread.Sleep(10 * 1000);
                // }
                // else
                // {
                //     // WriteMessage("Stopping the LogReader");
                //     // lr.StopReading();
                //     // ctSource.Cancel();
                // }
            };

            // lr.EoFReached += async () => WriteMessage("Eof Handler #2");
            // lr.EoFReached += async () => WriteMessage("Eof Handler #3");

            // var cancelTask = Task
            //     .Delay(-1, CancelSource.Token)
            //     .ContinueWith(_ =>
            //     {
            //         WriteMessage("Stopping LogReader");
            //         // lr.StopReading();
            //         ctSource.Cancel();
            //     });

            // await Program.Delay(3000, ctSource.Token, "Before start reading");s

            WriteMessage("About to start reading");
            startingLineCount = _rawLineCount;
            var lrTask = lr.StartReadingAsync();
            try
            {
                WriteMessage("Task'd LR reading");
                await lrTask;
                WriteMessage("LR reading completed successfully");

            }
            catch (OperationCanceledException ex)
            {
                WriteMessage($"ReadLogLinesWrapper - Operation Cancelled: {ex.GetType().Name} - {ex.Message}\n{ex}");
                throw;
            }
            catch (Exception ex)
            {
                WriteMessage($"ReadLogLinesWrapper - Job Exception: {ex.GetType().Name} - {ex.Message}\n{ex}");
            }
            finally
            {
                WriteMessage("Finally done with LR reading");
                Program.DumpTaskInfo(lrTask, "lrTask");
            }
        }

        private async Task ReadLogLinesWrapper1(ChannelWriter<string> writer, string logPath, CancellationToken token, int delayTimeMs = 250)
        {
            WriteMessage("Writing channel: Reader");

            // Yield to get off the main thread context right away. This will continue on a thread pool.
            await Task.Yield();
            WriteMessage("Writing channel: Reader - after yield");

            while (!token.IsCancellationRequested)
            {
                // Get all the items currently available and add them to our channel
                foreach (var item in ReadLogLines(logPath, token))
                    writer.TryWrite(item);

                try
                {
                    WriteMessage($"Waiting to read next source chunk: {delayTimeMs}ms");
                    // Wait a bit of time before looking for the next chunk of items
                    await Task.Delay(delayTimeMs, token);
                }
                catch (TaskCanceledException ex)
                {
                    // Catch the cancel, handle cleanup and proper closing ...
                    WriteMessage($"ReadLogLinesWrapper-Cancel: {ex.GetType().Name} - {ex.Message}");
                }
            }

            writer.TryComplete();
            WriteMessage("Done writing channel: Reader");

            // token.ThrowIfCancellationRequested();
        }

        private IEnumerable<string> ReadLogLines(string logPath, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                yield break;

            using (var fs = LogFile.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            using (var sr = new StreamReader(fs))
            {
                // This double loop (with yield break in the middle) allows the caller to iterate over
                // every item we have, then pause and see if we have more
                string line;
                while (!token.IsCancellationRequested)
                {
                    int sessionCount = 0;
                    while (!token.IsCancellationRequested && (line = sr.ReadLine()) != null)
                    {
                        _rawLineCount++;
                        sessionCount++;
                        yield return line;
                    }

                    if (sessionCount > 0)
                        WriteMessage($"Current lines read: {sessionCount:N0}, {_sw.Elapsed} elapsed");

                    yield break;
                }
            }
        }

        private LogDatum TransformLogLineToDatum(string line)
        {
            _datumLineCount++;

            // Filter out the occasional totally blank line (not even a timestamp)
            // The ChannelProcessor will not put <null> responses into the next Channel
            if (String.IsNullOrEmpty(line))
                return null;

            var datum = new LogDatum(line, _datumLineCount);

            // Track so we can print debug message in other channels
            _lastLineNumber = datum.LineNumber;

            // if (_datumLineCount % 10000 == 0)
            //     WriteMessage($"Lines datum'd: {_datumLineCount:N0}, {_sw.Elapsed} elapsed");

            if (_datumLineCount == _rawLineCount)
                WriteMessage($"Log lines transformed into LogDatums: {_datumLineCount:N0}, {_sw.Elapsed} elapsed");

            return datum;
        }

        private ILine TransformDatumToLine(LogDatum datum)
        {
            _parsedLineCount++;
            var line = _parser.ParseLine(datum);

            // if (_parsedLineCount % 10000 == 0)
            //     WriteMessage($"Lines parsed: {_parsedLineCount:N0}, {_sw.Elapsed} elapsed");

            if (line.LogLine.LineNumber == _lastLineNumber)
                WriteMessage($"LogDatum's parsed into ILine's: {_parsedLineCount:N0}, {_sw.Elapsed} elapsed");

            return line;
        }

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

        private static void WriteMessage(string format, params object[] args) // DI this
        {
            Program.WriteMessage(format, args);
        }
    }
}
