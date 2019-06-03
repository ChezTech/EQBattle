using System;
using System.Collections.Generic;
using System.Linq;

namespace BizObjects
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

        private readonly YouResolver YouAre;
        public static readonly CharacterResolver CharResolver = new CharacterResolver(); // Uck, a global singleton, or better spin, a DI singleton :p
        private readonly CharacterTracker _charTracker;

        private IFight _currentSkirmish;

        public IList<IFight> Skirmishes { get; } = new List<IFight>();
        public IEnumerable<Character> Fighters { get => Skirmishes.SelectMany(x => x.Fighters).Select(x => x.Character); }

        public Battle(YouResolver youAre)
        {
            YouAre = youAre;
            CharResolver.AddPlayer(YouAre.Name);
            _charTracker = new CharacterTracker(YouAre, CharResolver);

            SetupNewFight();
        }

        public void AddLine(ILine line)
        {
            _charTracker.TrackLine((dynamic)line);

            if (IsNewSkirmishNeeded(line))
                SetupNewFight();

            _currentSkirmish.AddLine((dynamic)line);
        }

        private void SetupNewFight()
        {
            _currentSkirmish = new Skirmish(YouAre, CharResolver);
            Skirmishes.Add(_currentSkirmish);
        }

        private bool IsNewSkirmishNeeded(ILine line)
        {
            // We want to allow loot lines on a fight even when the fight is over (the mob is dead)
            if (_currentSkirmish.IsFightOver)
                return true;

            // If this is a new mob, but the old mob isn't dead yet, then the fight turns in to a skirmish

            return false;
        }
    }
}
