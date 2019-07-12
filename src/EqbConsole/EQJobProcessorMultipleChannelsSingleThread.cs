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

        private const int SortBatchSize = 5000;

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
            var parseTask = Task.Run(() => ParseLines(_logLinesChannel.Reader, _parsedLinesChannel.Writer));
            var battleTask = Task.Run(() => AddLinesToBattleAsync(_parsedLinesChannel.Reader, eqBattle));

            // Start our main guy up, this starts the whole pipeline flow going
            var readTask = Task.Run(() => ReadLines(logFilePath, _logLinesChannel.Writer));

            // Wait for everything to finish up
            await readTask;
            await parseTask;
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

        private async Task ParseLines(ChannelReader<LogDatum> reader, ChannelWriter<ILine> writer)
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

            writer.Complete();
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
