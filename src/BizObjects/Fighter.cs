using System.Collections.Generic;
using System.Linq;

namespace BizObjects
{
    public class Fighter
    {
        public Character Character { get; }
        // public IList<Character> Pets { get; } = new List<Character>();
        // public IList<Character> Mercenaries { get; } = new List<Character>();
        public IList<Hit> Hits { get; } = new List<Hit>();
        public IList<Miss> Misses { get; } = new List<Miss>();
        public IList<ILine> Lines { get; } = new List<ILine>();
        // public int TotalDamageDealt { get => Hits.Sum(x => x.Damage); }
        public int TotalDamageDealt { get => Lines.Where(x => x is Hit).Select(x => x as Hit).Where(x => x.Attacker == Character).Sum(x => x.Damage); }
        // public int TotalDamageDealt { get => Lines.Where(x => x is Hit).Select(x => x as Hit).Where(x => x.Attacker.Equals(Character)).Sum(x => x.Damage); }

        public int TotalDamageTaken { get => Hits.Sum(x => x.Damage); }

        public Fighter(Character character)
        {
            Character = character;
        }

        public void AddOffense(Attack line)
        {
        }

        public void AddDefense(Attack line)
        {
        }


        public void AddHit(Hit hit)
        {
            Hits.Add(hit);
        }

        public void AddLine(ILine line)
        {
            Lines.Add(line);
        }


        // public void AddPet(Character pet)
        // {
        //     Pets.Add(pet);
        // }
        // public void AddMercenary(Character mercenary)
        // {
        //     Mercenaries.Add(mercenary);
        // }

        public int GetCurrentDamageDealtByCharacter()
        {
            return Hits
                .Where(x => x.Attacker.Name == Character.Name && !x.Attacker.IsPet)
                .Sum(x => x.Damage);
        }

        public int GetMinDamageDealtByCharacter()
        {
            return Hits
                .Where(x => x.Attacker.Name == Character.Name && !x.Attacker.IsPet)
                .Min(x => x.Damage);
        }

        public int GetMaxDamageDealtByCharacter()
        {
            return Hits
                .Where(x => x.Attacker.Name == Character.Name && !x.Attacker.IsPet)
                .Max(x => x.Damage);
        }

        public int GetCurrentDamageDealtByPet()
        {
            return Hits
                .Where(x => x.Attacker.Name == Character.Name && x.Attacker.IsPet)
                .Sum(x => x.Damage);
        }

        public int GetCurrentDamageReceivedByCharacter()
        {
            return Hits
                .Where(x => x.Defender.Name == Character.Name && !x.Defender.IsPet)
                .Sum(x => x.Damage);
        }


    }
}
