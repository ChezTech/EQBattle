using BizObjects.Battle;
using BizObjects.Converters;
using LogFileReader;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using LineParser;
using LineParser.Parsers;
using System.Diagnostics;
using Core;

namespace EQJobService
{
    public class EQJob : PropertyChangeBase
    {
        public string FileName { get; private set; }
        public Battle Battle { get; private set; }
        public YouResolver YouAre { get; private set; }

        public event Action StartReading;
        public event Action EoFBattle;

        protected virtual void OnStartReading() { StartReading?.Invoke(); }
        protected virtual void OnEoFBattle() { EoFBattle?.Invoke(); }

        private CancellationTokenSource _cts;
        private Stopwatch _sw = new Stopwatch();

        public TimeSpan ProcessingElapsed => _sw.Elapsed;

        public EQJob(string filePath)
        {
            VerifyLogFile(filePath);
            FileName = filePath;

            SetupNewBattle();
        }

        private static void VerifyLogFile(string filePath)
        {
            var fi = new FileInfo(filePath);
            if (!fi.Exists)
                throw new FileNotFoundException($"File not found: {filePath}", filePath);
        }

        private void SetupNewBattle()
        {
            var yourName = new WhoseLogFile().GetCharacterNameFromLogFile(FileName);
            YouAre = new YouResolver(yourName);
            Battle = new Battle(YouAre);

            Log.Information($"Detected character name: {YouAre.Name}, from log file: {FileName}");
        }

        public void CancelJob()
        {
            if (_cts == null)
                return;

            _cts.Cancel();
        }

        public async Task ReadFileIntoBattleAsync()
        {
            using (_cts = new CancellationTokenSource())
                await ProcessJob();
        }

        private async Task ProcessJob()
        {
            Log.Information($"Starting to process EQ Log file: {FileName}");

            var parser = CreateLineParser(YouAre);
            var parserJob = new EQJobEvenMoreChannels(parser);
            parserJob.CancelSource = _cts;
            parserJob.StartReading += () =>
            {
                OnStartReading();
                _sw.Start();
            };
            parserJob.EoFBattle += () =>
            {
                _sw.Stop();
                OnPropertyChanged(nameof(ProcessingElapsed));
                OnEoFBattle();
            };

            // Start the JobProcessor, which will read from the log file continuously, parse the lines and add them to the EQBattle
            // When it's done, show the summary
            var jobTask = parserJob.StartProcessingJobAsync(FileName, Battle);
            var jtError = jobTask.ContinueWith(_ => Log.Error(_.Exception, $"JobTask Error"), TaskContinuationOptions.OnlyOnFaulted);

            try
            {
                _sw.Restart();
                await jobTask;
            }
            catch (OperationCanceledException)
            {
                Log.Information("Program OperationCanceledException");
            }
            catch (Exception ex)
            {
                Log.Warning(ex, $"Program Exception");
            }
        }

        private LineParserFactory CreateLineParser(YouResolver youAre)
        {
            var parser = new LineParserFactory();
            parser.AddParser(new HitParser(youAre));
            parser.AddParser(new MissParser(youAre));
            parser.AddParser(new HealParser(youAre));
            parser.AddParser(new KillParser(youAre));
            parser.AddParser(new WhoParser(youAre));
            parser.AddParser(new ZoneParser());
            return parser;
        }
    }
}
