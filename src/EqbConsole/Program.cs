using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BizObjects.Battle;
using BizObjects.Converters;
using BizObjects.Lines;
using LineParser;
using LineParser.Parsers;
using LogFileReader;
using LogObjects;

namespace EqbConsole
{
    // Chart controls
    // https://github.com/Microsoft/InteractiveDataDisplay.WPF
    // https://github.com/Live-Charts/Live-Charts
    // https://github.com/oxyplot/oxyplot/

    class Program
    {
        enum ProgramMode
        {
            JobBlockingConnection,
            JobBasicChannel,
            JobMultiChannelMultiParser,
            JobMultiChannelSingleParser,
            JobMoreChannel,
        }

        private static ProgramMode _programMode = ProgramMode.JobMoreChannel;

        private YouResolver _youAre;
        private Battle _eqBattle;
        IJobProcessor _eqJob;

        static async Task Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Please specify a log file to read");
                return;
            }

            _programMode = GetProgramMode(args);
            var logPath = args[0];
            var numberOfParsers = args.Length > 2 ? int.Parse(args[2]) : 1;
            await new Program(logPath).RunProgramAsync(logPath, numberOfParsers);
        }

        private static ProgramMode GetProgramMode(string[] args)
        {
            if (args.Length < 2)
                return ProgramMode.JobMoreChannel;

            switch (args[1])
            {
                case "bc":
                    return ProgramMode.JobBlockingConnection;
                case "ba":
                    return ProgramMode.JobBasicChannel;
                case "mm":
                    return ProgramMode.JobMultiChannelMultiParser;
                case "ms":
                    return ProgramMode.JobMultiChannelSingleParser;
                case "mo":
                default:
                    return ProgramMode.JobMoreChannel;
            }
        }

        private Program(string logPath)
        {
            _youAre = new YouResolver(WhoseLogFile(logPath));
            _eqBattle = new Battle(_youAre);

            WriteMessage("You are: {0}", _youAre.Name);
        }

        private async Task RunProgramAsync(string logPath, int numberOfParsers)
        {
            using (var ctSource = new CancellationTokenSource())
                await RunProgramAsync(logPath, numberOfParsers, ctSource);
        }

        private async Task RunProgramAsync(string logPath, int numberOfParsers, CancellationTokenSource ctSource)
        {
            _eqJob = CreateJobProcessor(numberOfParsers);
            _eqJob.CancelSource = ctSource;

            // Get started waiting for user input
            // When the user quits, then cancel our CTS (which will cause our JobTask to be cancelled)
            var consoleTask = GetConsoleUserInputAsync(ctSource.Token);
            var ctComplete = consoleTask.ContinueWith(_ => ctSource.Cancel(), TaskContinuationOptions.OnlyOnRanToCompletion);
            // var ctContinue = consoleTask.ContinueWith(_ => { }); // Empty task to await upon

            // Start the JobProcessor, which will read from the log file continuously, parse the lines and add them to the EQBattle
            // When it's done, show the summary
            var jobTask = _eqJob.StartProcessingJobAsync(logPath, _eqBattle);
            var jtError = jobTask.ContinueWith(_ => WriteMessage($"JobTask Error: {_.Exception.InnerException.Message}"), TaskContinuationOptions.OnlyOnFaulted);

            // Either the log file wasn't found, or we finished reading the log file. It either case,
            // we need to cancel the 'consoleTask' so we don't wait for the user when we know we're done.
            var jtNotCancelled = jobTask.ContinueWith(_ => ctSource.Cancel(), TaskContinuationOptions.NotOnCanceled);

            // When everything completed successfully (which doesn't reall happen) or cancelled (which is the normal path), show the final status
            var jtComplete = jobTask.ContinueWith(_ => ShowBattleSummary(), TaskContinuationOptions.NotOnFaulted);

            ConcurrentBag<Task> tasksToWaitFor = new ConcurrentBag<Task>();

            try
            {
                // Wait for everything to finish.
                // await Task.WhenAll(ctComplete, ctContinue, jtError, jtNotCancelled, jtComplete);
                // await ctContinue;
                // ctSource.CancelAfter(7*1000);

                await Task.WhenAll(consoleTask, jobTask);

                tasksToWaitFor.Add(jtNotCancelled);
                tasksToWaitFor.Add(jtComplete);

            }
            // catch (TaskCanceledException)
            // {
            //     WriteMessage($"{ex.GetType().Name} - {ex.Message}");
            // }
            catch (OperationCanceledException ex)
            {
                WriteMessage($"Program Ex: {ex.GetType().Name} - {ex.Message}");

                if (jobTask.IsCanceled)
                    tasksToWaitFor.Add(jtComplete);



            }
            catch (Exception ex)
            {
                WriteMessage($"Program Ex: {ex.GetType().Name} - {ex.Message}");
                // WriteMessage($"Exception: {ex}");
                // WriteMessage($"Inner: {ex.InnerException}");

                if (jobTask.IsFaulted)
                {
                    tasksToWaitFor.Add(jtNotCancelled);
                    tasksToWaitFor.Add(jtError);
                }
            }
            finally
            {

                WriteMessage("Program Finally Block");
                // await Task.WhenAny(jtNotCancelled, jtComplete);
                // await Task.WhenAny(jtError, jtComplete);
                // await jtComplete;
                await Task.WhenAll(tasksToWaitFor);

                DumpTaskInfo(consoleTask, "consoleTask");
                // DumpTaskInfo(ctComplete, "ctComplete");
                // DumpTaskInfo(ctContinue, "ctContinue");
                DumpTaskInfo(jobTask, "jobTask");
                DumpTaskInfo(jtError, "jtError");
                DumpTaskInfo(jtNotCancelled, "jtNotCancelled");
                DumpTaskInfo(jtComplete, "jtComplete");
            }
        }
        public static void DumpTaskInfo(Task t, string title)
        {
            WriteMessage($"Task: {title,-17} ({t.Id,2})  Cncl: {t.IsCanceled,-5}  Cmplt: {t.IsCompleted,-5}  Sccs: {t.IsCompletedSuccessfully,-5}  Flt: {t.IsFaulted,-5}  Sts: {t.Status}{(t.Exception == null ? "" : $"  Ex: {t.Exception?.Message}")}");
            // if (t.Exception != null)
            //     WriteMessage($"Task Exception: {t.Exception}");
        }
        private async Task GetConsoleUserInputAsync(CancellationToken token)
        {
            // Pop this onto another thread which gives the job task a chance to detect that the file wasn't found (before writing out this message)
            await Task.Delay(20);
            token.ThrowIfCancellationRequested();
            WriteMessage("====== Press <Esc> to quit; 's' to get status ======");

            var conReader = new ConsoleReader();

            bool done = false;
            do
            {
                var cki = await conReader.ReadKeyAsync(true, token);

                switch (cki.Key)
                {
                    case ConsoleKey.S:
                        ShowBattleStatus();
                        break;

                    case ConsoleKey.Q:
                    case ConsoleKey.Escape:
                        done = true;
                        WriteMessage("<Esc> pressed. Exiting...");
                        break;

                }
            } while (!done);
        }

        private void ShowBattleStatus()
        {
            _eqJob?.ShowStatus();
            WriteMessage($"EQBattle raw line count: {_eqBattle.RawLineCount:N0}, line count: {_eqBattle.LineCount:N0}");
        }

        private void ShowBattleSummary()
        {
            WriteMessage("=-=-=-=- EQ Battle Summary =-=-=-=-");
            ShowBattleStatus();
            WriteMessage("Out of order count: {0:N0}, MaxDelta: {1}", _eqBattle.OutOfOrderCount, _eqBattle.MaxDelta);

            WriteMessage("== Skirmishes");
            WriteMessage("Skirmish count: {0}", _eqBattle.Skirmishes.Count);
            // foreach (Skirmish skirmish in _eqBattle.Skirmishes.Where(x => x.Statistics.Duration.FightDuration > new TimeSpan(0, 0, 7)))
            // {
            //     ShowSkirmishDetail(skirmish);
            //     foreach (Fight fight in skirmish.Fights.Where(x => x.Statistics.Duration.FightDuration > new TimeSpan(0, 0, 7)))
            //         ShowFightDetail(fight);
            // }

            // ShowNamedFighters();
            // ShowMobHeals();
            // ShowUnknownDamage();
        }

        private LineParserFactory CreateLineParser(YouResolver youAre)
        {
            var parser = new LineParserFactory();
            parser.AddParser(new HitParser(youAre));
            parser.AddParser(new MissParser(youAre));
            parser.AddParser(new HealParser(youAre));
            parser.AddParser(new KillParser(youAre));
            parser.AddParser(new WhoParser(youAre));
            return parser;
        }

        private IJobProcessor CreateJobProcessor(int numberOfParsers)
        {
            var parser = CreateLineParser(_youAre);

            switch (_programMode)
            {
                case ProgramMode.JobBlockingConnection:
                    return new EQJobProcessorBlockingCollection(parser, numberOfParsers);
                case ProgramMode.JobBasicChannel:
                    return new EQJobProcessorChannels(parser, numberOfParsers);
                case ProgramMode.JobMultiChannelMultiParser:
                    return new EQJobProcessorMultipleChannels(parser, numberOfParsers);
                case ProgramMode.JobMultiChannelSingleParser:
                    return new EQJobProcessorMultipleChannelsSingleThread(parser);

                case ProgramMode.JobMoreChannel:
                default:
                    return new EQJobEvenMoreChannels(parser);
            }
        }

        // private void ShowMobHeals()
        // {
        //     WriteMessage("");
        //     WriteMessage("===== Mob Heals ======");
        //     var heals = _healCollection.Where(x => x.Patient.IsMob || x.Healer.IsMob);
        //     WriteMessage($"Count: {heals.Count()}");

        //     foreach (var heal in heals)
        //         WriteMessage($"{heal.LogLine.LogMessage}");
        // }

        // private void ShowUnknownDamage()
        // {
        //     WriteMessage("");
        //     WriteMessage("===== Unknown lines containing Damage ======");
        //     var unknownDamage = _unknownCollection.Where(x => x.LogLine.LogMessage.Contains("damage"));
        //     WriteMessage($"Count: {unknownDamage.Count()}");

        //     foreach (var dmg in unknownDamage)
        //         WriteMessage($"{dmg.LogLine.LogMessage}");
        // }

        private void ShowSkirmishDetail(Skirmish skirmish)
        {
            WriteMessage($"---- Skirmish: {skirmish.Title,-30}");
        }

        private void ShowFightDetail(Fight fight)
        {
            WriteMessage($"--------- Fight: ({fight.Statistics.Duration.FightDuration:mm\\:ss})  {fight.Title,-30}");

            foreach (var fighter in fight.Fighters.OrderBy(x => x.Character.Name))
                ShowFighterDetail(fighter);
        }

        private void ShowFighterDetail(Fighter fighter)
        {
            WriteMessage(" {0,-30}  Off: {1,8:N0} ({2,4:P0}) {3,9:N2}    Def: {4,8:N0} ({5,4:P0})    Heals: {6,8:N0} / {7,8:N0}",
                fighter.Character,
                fighter.OffensiveStatistics.Hit.Total, fighter.OffensiveStatistics.HitPercentage, fighter.OffensiveStatistics.PerTime.FightDPS,
                fighter.DefensiveStatistics.Hit.Total, fighter.DefensiveStatistics.HitPercentage,
                fighter.OffensiveStatistics.Heal.Total, fighter.DefensiveStatistics.Heal.Total);
        }

        private void ShowNamedFighters()
        {
            WriteMessage("===== Named Fights ======");
            var namedFighters = _eqBattle.Fighters
                .Where(x => !x.Name.StartsWith("a "))
                .Where(x => !x.Name.StartsWith("an "))
                .Where(x => !x.IsPet)
                .Distinct()
                .OrderBy(x => x.Name);
            WriteMessage("Named fighter count: {0}", namedFighters.Count());
            WriteMessage("Named fighter count: \n{0}", string.Join("\t\n", namedFighters.Select(x => x.Name)));
        }

        private string WhoseLogFile(string logPath)
        {
            // Log file has standard name format: 'eqlog_<charName>_<serverName>.txt'
            var firstUnder = logPath.IndexOf('_');
            if (firstUnder == -1)
                return null;

            var secondUnder = logPath.IndexOf('_', firstUnder + 1);
            if (secondUnder == -1)
                return null;

            return logPath.Substring(firstUnder + 1, secondUnder - firstUnder - 1);
        }

        public static void WriteMessage(string format, params object[] args)
        {
            Console.WriteLine("[{0:yyyy-MM-dd HH:mm:ss.fff}] ({1,6}) {2}", DateTime.Now, Thread.CurrentThread.ManagedThreadId, string.Format(format, args));
        }

        public static async Task Delay(int delayTimeMs, CancellationToken token, string title = "")
        {
            try
            {
                // WriteMessage($"Delay-Start ({delayTimeMs}ms) - {title}");
                // Wait a bit of time before looking for the next chunk of items
                await Task.Delay(delayTimeMs, token);
                // WriteMessage($"Delay-Finish - {title}");
            }
            catch (TaskCanceledException)
            {
                // Catch the cancel, handle cleanup and proper closing ...
                // WriteMessage($"Delay-Cancel - {title}: {ex.GetType().Name} - {ex.Message}");
                throw;
            }
            finally
            {
                // WriteMessage($"Delay-Finally - {title}");
            }
        }
    }
}
