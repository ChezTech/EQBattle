using System;
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
using Serilog;
using Serilog.Events;

namespace EQJobService
{
    public class EQJobEvenMoreChannels : JobProcessor
    {
        // Thanks: https://gist.github.com/AlgorithmsAreCool/b0960ce8a3400305e43fe8ffdf89b32c

        private readonly Channel<RawLogLineInfo> _rawLinesChannel;
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

        private class RawLogLineInfo
        {
            public string LogLine;
            public int LineNumber;
        }

        public EQJobEvenMoreChannels(LineParserFactory parser) : base(parser)
        {
            _rawLinesChannel = Channel.CreateUnbounded<RawLogLineInfo>(new UnboundedChannelOptions()
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
            WriteLog(LogEventLevel.Verbose, $"Job status: log lines read: {_rawLineCount,10:N0}, datum'd: {_datumLineCount,10:N0}, parsed {_parsedLineCount,10:N0}, battled: {_battleLinesCount,10:N0}, elapsed: {_sw.Elapsed}");
        }

        public async override Task StartProcessingJobAsync(string logFilePath, Battle eqBattle)
        {
            // WatchFile = false;

            Log.Debug($"Starting to process EQBattle. ({this.GetType().Name})");

            var ctSource = CancelSource;

            var cp = new ChannelProcessor(ctSource.Token);

            var datumTask = cp.Process(_rawLinesChannel.Reader, _logLinesChannel.Writer, item => TransformLogLineToDatum(item), "Datum");
            var dtCancel = datumTask.ContinueWith(_ => Log.Verbose("DatumTask cancelled"), TaskContinuationOptions.OnlyOnCanceled);
            var dtComplete = datumTask.ContinueWith(_ => Log.Verbose("DatumTask complete"), TaskContinuationOptions.OnlyOnRanToCompletion);
            var dtContinue = datumTask.ContinueWith(_ =>
            {
                Log.Verbose($"DatumTask done. {_datumLineCount:N0} datum lines, {_sw.Elapsed} elapsed");
                _logLinesChannel.Writer.TryComplete();
            });

            var parseTask = cp.Process(_logLinesChannel.Reader, _parsedLinesChannel.Writer, item => TransformDatumToLine(item), "Parser");
            var ptCancel = parseTask.ContinueWith(_ => Log.Verbose("ParseTask cancelled"), TaskContinuationOptions.OnlyOnCanceled);
            var ptComplete = parseTask.ContinueWith(_ => Log.Verbose("ParseTask complete"), TaskContinuationOptions.OnlyOnRanToCompletion);
            var ptContinue = parseTask.ContinueWith(_ =>
            {
                Log.Verbose($"ParseTask done. {_parsedLineCount:N0} parse lines, {_sw.Elapsed} elapsed");
                _parsedLinesChannel.Writer.TryComplete();
            });

            var battleTask = cp.Process(_parsedLinesChannel.Reader, item => AddLineToBattle(item, eqBattle), "Battle");
            var btCancel = battleTask.ContinueWith(_ => Log.Verbose("BattleTask cancelled"), TaskContinuationOptions.OnlyOnCanceled);
            var btComplete = battleTask.ContinueWith(_ => Log.Verbose("BattleTask complete"), TaskContinuationOptions.OnlyOnRanToCompletion);
            var btContinue = battleTask.ContinueWith(_ =>
            {
                Log.Verbose($"BattleTask done. {_battleLinesCount:N0} battle lines, {_sw.Elapsed} elapsed");
            });

            // Start our main guy up, this starts the whole pipeline flow going
            _sw.Start();
            // var readTask = cp.Process(_rawLinesChannel.Writer, ct => ReadLogLines(logFilePath, ct));
            var readTask = ReadLogLinesWrapper(_rawLinesChannel.Writer, logFilePath, ctSource.Token, 250);
            var rtCancel = readTask.ContinueWith(_ => Log.Verbose("ReadTask cancelled"), TaskContinuationOptions.OnlyOnCanceled);
            var rtComplete = readTask.ContinueWith(_ => Log.Verbose("ReadTask complete"), TaskContinuationOptions.OnlyOnRanToCompletion);
            var rtContinue = readTask.ContinueWith(_ =>
            {
                Log.Verbose($"ReadTask done. {_rawLineCount:N0} read lines, {_sw.Elapsed} elapsed");
                _rawLinesChannel.Writer.TryComplete();
            });

            try
            {
                // Wait for everything to finish up
                await Task.WhenAll(readTask, datumTask, parseTask, battleTask);

                Log.Verbose("All tasks that we're waiting for are done");
            }
            catch (OperationCanceledException)
            {
                Log.Verbose("EQ Job Operation Cancelled");
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "EQ Job");
                throw;
            }
            finally
            {
                Log.Verbose("Job done");

                // Give all continuation tasks a chance to finish
                await Task.WhenAll(rtContinue, dtContinue, ptContinue, btContinue);

                DumpTaskInfo(readTask, "readTask");
                DumpTaskInfo(datumTask, "datumTask");
                DumpTaskInfo(parseTask, "parseTask");
                DumpTaskInfo(battleTask, "battleTask");

                DumpChannelInfo(_rawLinesChannel, "_rawLinesChannel");
                DumpChannelInfo(_logLinesChannel, "_logLinesChannel");
                DumpChannelInfo(_parsedLinesChannel, "_parsedLinesChannel");

                Log.Debug($"Total processing EQBattle, {_sw.Elapsed} elapsed");
                Log.Verbose($"LastLineNumber read: {_lastLineNumber:N0}");
                Log.Verbose($"LastLineNumber added to Battle: {_lastLineAddedToBattle?.LogLine.LineNumber:N0}");
            }
        }

        public static void DumpTaskInfo(Task t, string title)
        {
            Log.Verbose($"Task: {title,-17} ({t.Id,2})  Cncl: {t.IsCanceled,-5}  Cmplt: {t.IsCompleted,-5}  Sccs: {t.IsCompletedSuccessfully,-5}  Flt: {t.IsFaulted,-5}  Sts: {t.Status}{(t.Exception == null ? "" : $"  Ex: {t.Exception?.Message}")}");
            // if (t.Exception != null)
            //     WriteMessage($"Task Exception: {t.Exception}");
        }

        public static void DumpChannelInfo<T>(Channel<T> channel, string title)
        {
            Log.Verbose($"Channel: {title,-20} reader status: {channel.Reader.Completion.Status}");
        }

        private async Task ReadLogLinesWrapper(ChannelWriter<RawLogLineInfo> writer, string logPath, CancellationToken token, int delayTimeMs = 250)
        {
            Log.Verbose("Writing channel: Reader");
            var startingLineCount = 0;

            var lr = new LogReader(logPath, token);
            lr.StartReading += () =>
            {
                Log.Verbose($"LogReader starting to read the next chunk. {_rawLineCount:N0} read lines, {_sw.Elapsed} elapsed");
                OnStartReading();
            };

            lr.LineRead += line =>
            {
                _rawLineCount++;

                // Filter out the occasional totally blank line (not even a timestamp)
                if (!String.IsNullOrEmpty(line))
                {
                    writer.TryWrite(new RawLogLineInfo() { LogLine = line, LineNumber = _rawLineCount, });

                    // Track so we can print debug message in other channels
                    _lastLineNumber = _rawLineCount;
                }
            };

            // lr.EoFReached += async () => await Delay(delayTimeMs, token);
            lr.EoFReached += async () =>
            {
                var eofLineCount = _rawLineCount - startingLineCount;
                if (eofLineCount > 0)
                    Log.Verbose($"LogReader EOF - waiting to read next chunk. {_rawLineCount:N0} read lines, {_sw.Elapsed} elapsed");

                startingLineCount = _rawLineCount;

                OnEoFReached();

                if (WatchFile)
                    await Task.Delay(delayTimeMs, token);
                else
                    CancelSource.Cancel(); // Not the best way I don't think. Perhaps just a return of 'False'
            };

            Log.Verbose("About to start reading");
            startingLineCount = _rawLineCount;
            var lrTask = lr.StartReadingAsync();
            try
            {
                Log.Verbose("Task'd LR reading");
                await lrTask;
                Log.Verbose("LR reading completed successfully");
            }
            catch (OperationCanceledException)
            {
                Log.Verbose("ReadLogLinesWrapper");
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ReadLogLinesWrapper");
                throw;
            }
            finally
            {
                Log.Verbose("Finally done with LR reading");
                DumpTaskInfo(lrTask, "lrTask");
            }
        }

        private LogDatum TransformLogLineToDatum(RawLogLineInfo lineInfo)
        {
            _datumLineCount++;
            var datum = new LogDatum(lineInfo.LogLine, lineInfo.LineNumber);

            if (datum.LineNumber == _lastLineNumber)
                Log.Verbose($"Log lines transformed into LogDatums: {_datumLineCount:N0}, {_sw.Elapsed} elapsed");

            return datum;
        }

        private ILine TransformDatumToLine(LogDatum datum)
        {
            _parsedLineCount++;
            var line = _parser.ParseLine(datum);

            if (line.LogLine.LineNumber == _lastLineNumber)
                Log.Verbose($"LogDatum's parsed into ILine's: {_parsedLineCount:N0}, {_sw.Elapsed} elapsed");

            return line;
        }

        private void AddLineToBattle(ILine line, Battle eqBattle)
        {
            _battleLinesCount++;
            eqBattle.AddLine((dynamic)line);
            _lastLineAddedToBattle = line;

            if (line.LogLine.LineNumber == _lastLineNumber)
            {
                Log.Verbose($"Lines added to Battle: {_battleLinesCount:N0}, {_sw.Elapsed} elapsed");
                OnEoFBattle();
            }
        }

        public static void WriteLog(LogEventLevel logLevel, string format, params object[] args)
        {
            Console.WriteLine(format, args);
            Log.Write(logLevel, format, args);
        }
    }
}
