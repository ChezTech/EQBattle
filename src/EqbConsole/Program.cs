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
        private YouResolver _youAre;
        private Battle _eqBattle;

        static async Task Main(string[] args)
        {
            var logPath = args[0];
            var numberOfParsers = args.Length > 1 ? int.Parse(args[1]) : 1;
            await new Program(logPath).RunProgramAsync(logPath, numberOfParsers);
        }

        private Program(string logPath)
        {
            _youAre = new YouResolver(WhoseLogFile(logPath));
            _eqBattle = new Battle(_youAre);

            WriteMessage("You are: {0}", _youAre.Name);
        }

        private async Task RunProgramAsync(string logPath, int numberOfParsers)
        {
            var eqJob = CreateJobProcessor(numberOfParsers);
            // eqJob.StartProcessingJob(logPath, _eqBattle);
            await eqJob.StartProcessingJobAsync(logPath, _eqBattle);

            WriteMessage("Out of order count: {0:N0}, MaxDelta: {1}", _eqBattle.OutOfOrderCount, _eqBattle.MaxDelta);

            WriteMessage("===== Skirmishes ======");
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
            // return new EQJobProcessorBlockingCollection(parser, numberOfParsers);
            // return new EQJobProcessorChannels(parser, numberOfParsers);
            return new EQJobProcessorMultipleChannels(parser, numberOfParsers);
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

        private void WriteMessage(string format, params object[] args)
        {
            Console.WriteLine("[{0:yyyy-MM-dd HH:mm:ss.fff}] ({1,6}) {2}", DateTime.Now, Thread.CurrentThread.ManagedThreadId, string.Format(format, args));
        }
    }
}
