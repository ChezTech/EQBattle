using System.Collections.Generic;
using System.Linq;

namespace BizObjects
{
    public class Fighter
    {
        public Character Character { get; }
        public IList<Character> Pets { get; } = new List<Character>();
        public IList<Character> Mercenaries { get; } = new List<Character>();
        public IList<Hit> Hits { get; } = new List<Hit>();
        public IList<Miss> Misses { get; } = new List<Miss>();

        public Fighter(Character character)
        {
            Character = character;
        }

        public void AddHit(Hit hit)
        {
            Hits.Add(hit);
        }

        public void AddPet(Character pet)
        {
            Pets.Add(pet);
        }
        public void AddMercenary(Character mercenary)
        {
            Mercenaries.Add(mercenary);
        }

        public int GetCurrentDamageDealtByCharacter()
        {
            return Hits
                .Where(x => x.Attacker.Name == Character.Name && !x.Attacker.IsPet)
                .Sum(x => x.Damage);
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
