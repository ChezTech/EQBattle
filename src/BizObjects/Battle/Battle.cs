using BizObjects.Converters;
using BizObjects.Lines;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace BizObjects.Battle
{
    public class Battle
    {
        // Can we have multiple fights going on at a time? Sure, if you get adds or have spawns or boss fights with minions
        // How would we distinguish the fights from each other? By mob you're attacking, or any PC is attacking.
        // Mobs may be parked (mezzed, rooted) until the group/raid can finish off the main mob
        // Or, you may split the group/raid so everyone is attacking different mobs and healers may be healing PCs from both "groups"
        // Should this be multiple Fight instance, or one Fight instance with multiple mobs?
        // Can we have a Fight and sub-Fight (skirmish)? Or Battle and Fights?
        // How to show on UI?
        // I like the idea of a Fight w/ Skirmishes ... should I internally represent each fight with at least one skirmish?
        // Should a fight turn into a skirmish if there's more than one mob? I think a skirmish should be bigger than a fight, but
        // I don't want to represent the most common object as a Skirmish, but rather as a Fight.
        // When I list the fights in the log file, I can use a different icon or name format to indicate more than one mob
        // How can I split out different mobs into different fights?
        // Can I make the Skirmish object, inherit from a fight, then replace the Fight object with a Skirmish one in the fight list?

        public readonly YouResolver YouAre;
        public static readonly CharacterResolver CharResolver = new CharacterResolver(); // Uck, a global singleton, or better spin, a DI singleton :p
        private readonly CharacterTracker _charTracker;
        private readonly TimeSpan _skirmishGap = new TimeSpan(0, 0, 8); // Make this configurable

        private ISkirmish _currentSkirmish;

        public ObservableCollection<ISkirmish> Skirmishes { get; } = new ObservableCollection<ISkirmish>();
        public int LineCount => Skirmishes.Sum(x => x.LineCount);
        public int RawLineCount = 0;
        public IEnumerable<Character> Fighters { get => Skirmishes.SelectMany(x => x.Fighters).Select(x => x.Character); }

        public Zone CurrentZone { get; private set; } = Zone.Unknown;
        public Dictionary<Zone, List<ILine>> ZoneLineMap { get; private set; } = new Dictionary<Zone, List<ILine>>() { { Zone.Unknown, new List<ILine>() } };

        private static object _lock = new object();

        public Battle(YouResolver youAre)
        {
            YouAre = youAre;
            CharResolver.SetPlayer(YouAre.Name);
            _charTracker = new CharacterTracker(YouAre, CharResolver);
        }

        public void AddLine(ILine line)
        {
            lock (_lock)
            {
                AddLineToBattle(line);

                VerifyLineOrder(line);

                _charTracker.TrackLine((dynamic)line);

                if (IsNewSkirmishNeeded((dynamic)line))
                    SetupNewFight();

                _currentSkirmish.AddLine((dynamic)line);
            }
        }

        public void AddLine(Zone line)
        {
            lock (_lock)
            {
                CurrentZone = line;

                if (!ZoneLineMap.ContainsKey(CurrentZone))
                    ZoneLineMap[CurrentZone] = new List<ILine>();

                AddLineToBattle(line as ILine);
            }
        }

        private void AddLineToBattle(ILine line)
        {
            RawLineCount++;

            ZoneLineMap[CurrentZone].Add(line);
        }

        private ILine _previousLine = null;
        public int OutOfOrderCount = 0;
        public int MaxDelta = 0;

        private void VerifyLineOrder(ILine line)
        {
            if (_previousLine != null)
            {
                if (line.LogLine.LineNumber < _previousLine.LogLine.LineNumber)
                {
                    int delta = _previousLine.LogLine.LineNumber - line.LogLine.LineNumber;
                    MaxDelta = Math.Max(MaxDelta, delta);
                    OutOfOrderCount++;
                    // System.Console.WriteLine("Line out of order:\n   Prev: [{2,5}] {0}\n   Cur:  [{3,5}] {1}", _previousLine.LogLine.RawLogLine, line.LogLine.RawLogLine, _previousLine.LogLine.LineNumber, line.LogLine.LineNumber);
                }
            }

            _previousLine = line;
        }

        private void SetupNewFight()
        {
            _currentSkirmish = new Skirmish(YouAre, CharResolver);
            Skirmishes.Add(_currentSkirmish);
        }

        private bool IsNewSkirmishNeeded(ILine line)
        {
            if (_currentSkirmish == null)
                return true;

            return false;
        }

        private bool IsNewSkirmishNeeded(Attack line)
        {
            if (_currentSkirmish == null)
                return true;

            // If the current Skirmish has nothing in it, we don't need a new one
            // TODO: a better way to see if a skirmish 'isEmpty'
            if (!_currentSkirmish.Statistics.Lines.Any())
                return false;

            // If the current Skirmish (most recent fight) looks like a Pull, then don't start a new skirmish
            // Or, move that pull line into a new skirmish for this new fight
            // Q: what does a pull look like?
            // A: one or two hits of low damage
            // A: maybe not necessarily low damage (bow shot pull, spell pull)
            // A: still within a period of time, just a longer one

            // A new Skirmish is needed if it's been too long since the last mob died
            // If you're fighting mobs without a break, they all get grouped into one skirmish
            // Loot lines, roll-offs, etc go with that last fight

            // A new mob (after time period) is a new skirmish

            // Just because you die doesn't mean the fight is over .... you can be rezzed back into fight
            // Maybe this is a special case .. if you get rezzed and still fighting the same mob...

            // If it's been enough time since the last attack and this attack, then a new Skirmish is needed
            if (line.Time - _currentSkirmish.LastAttackTime > _skirmishGap)
                return true;

            // We want to allow loot lines on a fight even when the fight is over (the mob is dead)
            // if (_currentSkirmish.IsFightOver)
            //     return true;

            // If this is a new mob, but the old mob isn't dead yet, then the fight turns in to a skirmish

            return false;
        }

        /// <summary>
        /// Freeze Updates to the current fight so that the UI can refresh itself without underlying collections changing
        /// </summary>
        public class Freeze : IDisposable
        {
            public Freeze()
            {
                Monitor.Enter(_lock);
            }

            public void Dispose()
            {
                Monitor.Exit(_lock);
            }
        }
    }
}
