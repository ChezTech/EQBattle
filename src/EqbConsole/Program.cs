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
        //private const string LogFilePathName = @"C:\Program Files (x86)\Steam\steamapps\common\Everquest F2P\Logs\eqlog_Khadaji_erollisi_test.txt";
        private const string LogFilePathName = @"C:\Program Files (x86)\Steam\steamapps\common\Everquest F2P\Logs\eqlog_Khadaji_erollisi_test_medium.txt";
        //private const string LogFilePathName = @"C:\Program Files (x86)\Steam\steamapps\common\Everquest F2P\Logs\eqlog_Khadaji_erollisi_2019-03-25-182700.txt";

        private YouResolver _youAre;
        private LineParserFactory _parser;
        private ConcurrentQueue<ILine> _lineCollection = new ConcurrentQueue<ILine>();
        private ConcurrentQueue<Unknown> _unknownCollection = new ConcurrentQueue<Unknown>();
        private ConcurrentQueue<Hit> _hitCollection = new ConcurrentQueue<Hit>();
        private ConcurrentQueue<Kill> _killCollection = new ConcurrentQueue<Kill>();
        private ConcurrentQueue<Miss> _missCollection = new ConcurrentQueue<Miss>();
        private ConcurrentQueue<Heal> _healCollection = new ConcurrentQueue<Heal>();

        private BlockingCollection<LogDatum> _jobQueueLogLines = new BlockingCollection<LogDatum>();
        private ConcurrentBag<ILine> _parsedLines = new ConcurrentBag<ILine>();

        private Battle _eqBattle;

        static void Main(string[] args)
        {
            var logPath = args.Length > 0 ? args[0] : LogFilePathName;
            var numberOfParsers = args.Length > 1 ? int.Parse(args[1]) : 1;
            new Program(logPath).RunProgram(logPath, numberOfParsers);
        }

        private Program(string logPath)
        {
            _youAre = new YouResolver(WhoseLogFile(logPath));
            _eqBattle = new Battle(_youAre);

            WriteMessage("You are: {0}", _youAre.Name);

            _parser = new LineParserFactory();
            _parser.UnknownCreated += x => { _unknownCollection.Enqueue(x); };
            _parser.AddParser(new KillParser(_youAre), x => { _killCollection.Enqueue((dynamic)x); });
            _parser.AddParser(new HitParser(_youAre), x => { _hitCollection.Enqueue((dynamic)x); });
            _parser.AddParser(new MissParser(_youAre), x => { _missCollection.Enqueue((dynamic)x); });
            _parser.AddParser(new HealParser(_youAre), x => { _healCollection.Enqueue((dynamic)x); });
        }

        private void RunProgram(string logPath, int numberOfParsers)
        {
            var eqJob = new EQJobProcessorBlockingCollection(_eqBattle, logPath);
            eqJob.ParserCount = numberOfParsers;
            eqJob.StartProcessingJob();

            WriteMessage("Out of order count: {0:N0}, MaxDelta: {1}", _eqBattle.OutOfOrderCount, _eqBattle.MaxDelta);

            // WriteMessage("Line collection count: {0}", _lineCollection.Count);
            // WriteMessage("Unknown collection count: {0}", _unknownCollection.Count);
            // WriteMessage("Attack collection count: {0}", _hitCollection.Count);
            // WriteMessage("Kill collection count: {0}", _killCollection.Count);
            // WriteMessage("Miss collection count: {0}", _missCollection.Count);
            // WriteMessage("Heal collection count: {0}", _healCollection.Count);

            // WriteMessage("===== Attacks ======");
            // WriteMessage("Total damage: {0:N0}", _hitCollection.Sum(x => x.Damage));
            // DumpStatsForCharacter("Khadaji");
            // DumpStatsForCharacter("Khadaji", charOnly: true);
            // DumpStatsForCharacter("Khadaji", isPet: true);
            // DumpStatsForCharacter("Khadaji", negative: true);

            // WriteMessage("===== Battle ======");
            // WriteMessage("Skirmish count: {0}", _eqBattle.Skirmishes.Count);
            // WriteMessage("Skirmish offensive damage total: {0:N0}", _eqBattle.Skirmishes.Sum(x => x.OffensiveStatistics.Hit.Total));
            // WriteMessage("Skirmish defensive damage total: {0:N0}", _eqBattle.Skirmishes.Sum(x => x.DefensiveStatistics.Hit.Total));
            // WriteMessage("Khadaji offensive total: {0:N0}", _eqBattle.Skirmishes.SelectMany(x => x.Fighters.Where(y => y.Character.Name == "Khadaji")).Sum(y => y.OffensiveStatistics.Hit.Total));
            // WriteMessage("");
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

        private void AddParsedLinesToBattle()
        {
            // foreach (var kvLine in _parsedLines)
            //     _eqBattle.AddLine(kvLine.Value);

            // var e = _parsedLines.GetEnumerator();
            // while (e.MoveNext())
            //     _eqBattle.AddLine(e.Current);

            // Bottleneck here, having to wait for everything to be parsed, then added to the Bag, then sort all at once to add to EQBattle
            // Would like to chunk it up as we go ... get a chunk of 5000 lines, sort, take the first 1000 ... repeat
            var sortedLines = _parsedLines
                // .OrderBy(x => x.LogLine.LineNumber)
                // .ToList()
                ;

            foreach (var line in sortedLines)
                _eqBattle.AddLine(line);
        }

        private void ShowMobHeals()
        {
            WriteMessage("");
            WriteMessage("===== Mob Heals ======");
            var heals = _healCollection.Where(x => x.Patient.IsMob || x.Healer.IsMob);
            WriteMessage($"Count: {heals.Count()}");

            foreach (var heal in heals)
                WriteMessage($"{heal.LogLine.LogMessage}");
        }

        private void ShowUnknownDamage()
        {
            WriteMessage("");
            WriteMessage("===== Unknown lines containing Damage ======");
            var unknownDamage = _unknownCollection.Where(x => x.LogLine.LogMessage.Contains("damage"));
            WriteMessage($"Count: {unknownDamage.Count()}");

            foreach (var dmg in unknownDamage)
                WriteMessage($"{dmg.LogLine.LogMessage}");
        }

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

        private void DumpStatsForCharacter(string name, bool charOnly = false, bool isPet = false, bool negative = false)
        {
            Func<Character, bool> fCharAndPet = x => x.Name == name;
            Func<Character, bool> fCharOnly = x => x.Name == name && !x.IsPet;
            Func<Character, bool> fPpetOnly = x => x.Name == name && x.IsPet;
            Func<Character, bool> fNegativeChar = x => x.Name != name;

            var nameTitle = name;
            Func<Character, bool> fToUse = fCharAndPet;
            if (charOnly)
            {
                fToUse = fCharOnly;
                nameTitle = string.Format("{0} only", name);
            }
            if (isPet)
            {
                fToUse = fPpetOnly;
                nameTitle = string.Format("{0}'s pet", name);
            }
            if (negative)
            {
                fToUse = fNegativeChar;
                nameTitle = string.Format("Not {0}", name);
            }

            WriteMessage("{0,-15} : Yeet {1,12:N0}  Ouch: {2,12:N0}  Heals: {3,12:N0}",
                nameTitle,
                _hitCollection.Where(x => fToUse(x.Attacker)).Sum(x => x.Damage),
                _hitCollection.Where(x => fToUse(x.Defender)).Sum(x => x.Damage),
                _healCollection.Where(x => fToUse(x.Patient)).Sum(x => x.Amount));
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

        private void ParseLines()
        {
            WriteMessage("Starting to parse lines...");
            while (!_jobQueueLogLines.IsCompleted)
            {
                try
                {
                    var logLine = _jobQueueLogLines.Take();
                    var line = _parser.ParseLine(logLine);
                    // _eqBattle.AddLine(line);
                    _parsedLines.Add(line);

                    // if (logLine.LineNumber % 10000 == 0)
                    //     WriteMessage("Parsed {0:N0} lines...", logLine.LineNumber);
                }
                catch (InvalidOperationException) { }
            }
        }

        private TimeSpan ReadLines(string logPath, out int lineCount, Stopwatch sw)
        {
            WriteMessage("Reading log file: {0}", logPath);

            int count = 0;
            using (LogReader logReader = new LogReader(logPath))
            {
                logReader.LineRead += (s, e) =>
                {
                    count++;
                    var logLine = new LogDatum(e.LogLine, count);
                    _jobQueueLogLines.Add(logLine);
                };
                logReader.EoFReached += (s, e) => { logReader.StopReading(); };
                logReader.StartReading();
            }
            _jobQueueLogLines.CompleteAdding();
            lineCount = count;

            WriteMessage("Done reading log file");
            return sw.Elapsed;
        }

        private void WriteMessage(string format, params object[] args)
        {
            Console.WriteLine("[{0:yyyy-MM-dd HH:mm:ss.fff}] ({1,6}) {2}", DateTime.Now, Thread.CurrentThread.ManagedThreadId, string.Format(format, args));
        }
    }
}
