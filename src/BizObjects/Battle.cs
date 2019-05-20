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

        private Fight _currentFight;

        public IList<Fight> Fights { get; } = new List<Fight>();
        public IEnumerable<Character> Fighters { get => Fights.SelectMany(x => x.Fighters).Select(x => x.Character); }

        public Battle()
        {
            SetupNewFight();
        }

        public void AddLine(ILine line)
        {
            if (IsNewFightNeeded(line))
                SetupNewFight();

            _currentFight.AddLine((dynamic)line);
        }

        private void SetupNewFight()
        {
            _currentFight = new Fight();
            Fights.Add(_currentFight);
        }

        private bool IsNewFightNeeded(ILine line)
        {
            if (_currentFight.IsFightOver(line))
                return true;

            return false;
        }
    }
}
