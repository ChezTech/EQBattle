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

namespace EqbConsole
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
            WriteMessage($"Job status: log lines read: {_rawLineCount,10:N0}, datum'd: {_datumLineCount,10:N0}, parsed {_parsedLineCount,10:N0}, battled: {_battleLinesCount,10:N0}, elapsed: {_sw.Elapsed}");
        }

        public async override Task StartProcessingJobAsync(string logFilePath, Battle eqBattle)
        {
            // WatchFile = false;

            WriteMessage($"Starting to process EQBattle. ({this.GetType().Name})");

            var ctSource = CancelSource;

            var cp = new ChannelProcessor(ctSource.Token);

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
                // Wait for everything to finish up
                await Task.WhenAll(readTask, datumTask, parseTask, battleTask);

                WriteMessage("All tasks that we're waiting for are done");
            }
            catch (OperationCanceledException ex)
            {
                WriteMessage($"EQ Job Operation Cancelled: {ex.GetType().Name} - {ex.Message}");
                // WriteMessage($"Exception: {ex}");
                // WriteMessage($"Inner: {ex.InnerException}");
                throw;
            }
            catch (Exception ex)
            {
                WriteMessage($"EQ Job Exception: {ex.GetType().Name} - {ex.Message}");
                // WriteMessage($"Exception: {ex}");
                // WriteMessage($"Inner: {ex.InnerException}");
                throw;
            }
            finally
            {
                WriteMessage("Job done");

                // Give all continuation tasks a chance to finish
                await Task.WhenAll(rtContinue, dtContinue, ptContinue, btContinue);

                Program.DumpTaskInfo(readTask, "readTask");
                Program.DumpTaskInfo(datumTask, "datumTask");
                Program.DumpTaskInfo(parseTask, "parseTask");
                Program.DumpTaskInfo(battleTask, "battleTask");

                DumpChannelInfo(_rawLinesChannel, "_rawLinesChannel");
                DumpChannelInfo(_logLinesChannel, "_logLinesChannel");
                DumpChannelInfo(_parsedLinesChannel, "_parsedLinesChannel");


                WriteMessage($"Total processing EQBattle, {_sw.Elapsed} elapsed");
                WriteMessage($"LastLineNumber read: {_lastLineNumber:N0}");
                WriteMessage($"LastLineNumber added to Battle: {_lastLineAddedToBattle?.LogLine.LineNumber:N0}");
                ShowStatus();
            }
        }

        public static void DumpChannelInfo<T>(Channel<T> channel, string title)
        {
            WriteMessage($"Channel: {title,-20} reader status: {channel.Reader.Completion.Status}");
        }

        private async Task ReadLogLinesWrapper(ChannelWriter<RawLogLineInfo> writer, string logPath, CancellationToken token, int delayTimeMs = 250)
        {
            WriteMessage("Writing channel: Reader");
            var startingLineCount = 0;

            var lr = new LogReader(logPath, token);
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
                    WriteMessage($"LogReader EOF - waiting to read next chunk. {_rawLineCount:N0} read lines, {_sw.Elapsed} elapsed");

                startingLineCount = _rawLineCount;

                if (WatchFile)
                    await Program.Delay(5000, token, "EOF");
                else
                    CancelSource.Cancel(); // Not the best way I don't think. Perhaps just a return of 'False'
            };


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
                WriteMessage($"ReadLogLinesWrapper - Operation Cancelled: {ex.GetType().Name} - {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                WriteMessage($"ReadLogLinesWrapper - Job Exception: {ex.GetType().Name} - {ex.Message}");
                throw;
            }
            finally
            {
                WriteMessage("Finally done with LR reading");
                Program.DumpTaskInfo(lrTask, "lrTask");
            }
        }

        private LogDatum TransformLogLineToDatum(RawLogLineInfo lineInfo)
        {
            _datumLineCount++;
            var datum = new LogDatum(lineInfo.LogLine, lineInfo.LineNumber);

            if (datum.LineNumber == _lastLineNumber)
                WriteMessage($"Log lines transformed into LogDatums: {_datumLineCount:N0}, {_sw.Elapsed} elapsed");

            return datum;
        }

        private ILine TransformDatumToLine(LogDatum datum)
        {
            _parsedLineCount++;
            var line = _parser.ParseLine(datum);

            if (line.LogLine.LineNumber == _lastLineNumber)
                WriteMessage($"LogDatum's parsed into ILine's: {_parsedLineCount:N0}, {_sw.Elapsed} elapsed");

            return line;
        }

        private void AddLineToBattle(ILine line, Battle eqBattle)
        {
            _battleLinesCount++;
            eqBattle.AddLine(line);
            _lastLineAddedToBattle = line;

            if (line.LogLine.LineNumber == _lastLineNumber)
                WriteMessage($"Lines added to Battle: {_battleLinesCount:N0}, {_sw.Elapsed} elapsed");
        }

        private static void WriteMessage(string format, params object[] args) // DI this
        {
            Program.WriteMessage(format, args);
        }
    }
}
