using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace BizObjects
{
    public class Skirmish : IFight
    {
        private readonly YouResolver YouAre;
        private readonly CharacterResolver CharResolver;

        public Skirmish(YouResolver youAre, CharacterResolver charResolver)
        {
            YouAre = youAre;
            CharResolver = charResolver;
            CreateNewFight();
        }

        public IList<IFight> Fights { get; } = new List<IFight>();

        public bool IsFightOver => Fights.All(x => x.IsFightOver);

        public DateTime LastAttackTime => Fights.Max(x => x.LastAttackTime);

        private ConcurrentDictionary<Character, Fighter> _fighters = new ConcurrentDictionary<Character, Fighter>();
        public IEnumerable<Fighter> Fighters => _fighters.Values;

        public FightStatistics OffensiveStatistics { get; } = new FightStatistics();
        public FightStatistics DefensiveStatistics { get; } = new FightStatistics();

        public Character PrimaryMob => Character.Unknown;
        public string Title => string.Join(", ", Fights.Select(x => x.PrimaryMob.Name));

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

        // This is the function we'll use to see if a fight is a valid match (mob match and fight not over)
        private readonly Func<IFight, Character, bool> IsValidFight = (f, c) => f.PrimaryMob == c && !f.IsFightOver;

        private IFight GetAppropriateFight(Character char1, Character char2)
        {
            // If the primary mob isn't established yet, use the first fight
            var fight = Fights.First();
            if (IsValidFight(fight, Character.Unknown))
                return fight;

            // If the char is a MOB, find the matching fight or create a new one
            if (CharResolver.WhichType(char1) == CharacterResolver.Type.NonPlayerCharacter)
                return GetOrAddFight(char1);
            if (CharResolver.WhichType(char2) == CharacterResolver.Type.NonPlayerCharacter)
                return GetOrAddFight(char2);

            // See if either character is already the primary mob (we can't tell if a named mob w/ a single name is a mob, so this may catch that)
            var primaryMobMatch = Fights.Where(x => IsValidFight(x, char1) || IsValidFight(x, char2));
            if (primaryMobMatch.Any())
                return primaryMobMatch.First();

            // Either the characters are not MOBs or one of them is a named Mob and we don't know it, just use the first fight that's still ongoing
            var firstActiveFight = Fights.Where(x => !x.IsFightOver);
            if (firstActiveFight.Any())
                return Fights.First();

            return CreateNewFight();
        }

        private IFight GetOrAddFight(Character mob)
        {
            return Fights
                .Where(x => IsValidFight(x, mob))
                .FirstOrDefault()
                ?? CreateNewFight();
        }

        private IFight CreateNewFight()
        {
            var fight = new Fight(YouAre, CharResolver);
            Fights.Add(fight);
            return fight;
        }

        public bool SimilarDamage(Hit line, bool looseMatch = false)
        {
            throw new NotImplementedException();
        }
    }
}
