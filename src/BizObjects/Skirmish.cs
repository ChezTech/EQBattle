using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace BizObjects
{
    public class Skirmish : IFight
    {
        private readonly YouResolver YouAre;

        public Skirmish(YouResolver youAre)
        {
            YouAre = youAre;
        }

        public IList<IFight> Fights { get; } = new List<IFight>();

        public bool IsFightOver => Fights.All(x => x.IsFightOver);

        private ConcurrentDictionary<Character, Fighter> _fighters = new ConcurrentDictionary<Character, Fighter>();
        public IEnumerable<Fighter> Fighters => _fighters.Values;

        public FightStatistics OffensiveStatistics { get; } = new FightStatistics();
        public FightStatistics DefensiveStatistics { get; } = new FightStatistics();

        public Character PrimaryMob => throw new NotImplementedException();

        public void AddLine(Attack line)
        {
            GetAppropriateFight(line.Attacker, line.Defender).AddLine(line);

            var attackChar = _fighters.GetOrAdd(line.Attacker, new Fighter(line.Attacker, this));
            attackChar.AddOffense(line);
            OffensiveStatistics.AddLine(line);

            var defendChar = _fighters.GetOrAdd(line.Defender, new Fighter(line.Defender, this));
            defendChar.AddDefense(line);
            DefensiveStatistics.AddLine(line);
        }

        public void AddLine(Heal line)
        {
            GetAppropriateFight(line.Healer, line.Patient).AddLine(line);

            var healerChar = _fighters.GetOrAdd(line.Healer, new Fighter(line.Healer));
            healerChar.AddOffense(line);
            OffensiveStatistics.AddLine(line);

            var patientChar = _fighters.GetOrAdd(line.Patient, new Fighter(line.Patient));
            patientChar.AddDefense(line);
            DefensiveStatistics.AddLine(line);
        }

        public void AddLine(ILine line)
        {
            // GetAppropriateFight(line).AddLine(line);
        }

        private IFight GetAppropriateFight(Character char1, Character char2)
        {
            if (!Fights.Any())
                return CreateNewFight();

            // If the primary mob isn't established yet, use the first fight
            var fight = Fights.First();
            if (fight.PrimaryMob == Character.Unknown)
                return fight;

            // If the char is a MOB, find the matching fight or create a new one
            if (char1.IsMob)
                return GetOrAddFight(char1);
            if (char2.IsMob)
                return GetOrAddFight(char2);

            // See if either character is already the primary mob (we can't tell if a named mob w/ a single name is a mob, so this may catch that)
            var primaryMobMatch = Fights.Where(x => x.PrimaryMob == char1 || x.PrimaryMob == char2);
            if (primaryMobMatch.Any())
                return primaryMobMatch.First();

            // Either the characters are not MOBs or one of them is a named Mob and we don't know it, just use the first fight
            return Fights.First();
        }

        private IFight GetOrAddFight(Character mob)
        {
            return Fights
                .Where(x => x.PrimaryMob == mob)
                .FirstOrDefault()
                ?? CreateNewFight();
        }

        private IFight CreateNewFight()
        {
            var fight = new Fight(YouAre);
            Fights.Add(fight);
            return fight;
        }
    }
}
