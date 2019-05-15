using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace BizObjects
{
    public class Battle
    {
        public ICollection<Character> Characters { get { return _fighters.Keys; } }
        public ICollection<Fighter> Fighters { get { return _fighters.Values; } }
        public IList<ILine> Lines { get; } = new List<ILine>();
        public string Zone { get; }
        private ConcurrentDictionary<Character, Fighter> _fighters = new ConcurrentDictionary<Character, Fighter>();

        /// All damage dealt, from both PCs and NPCs (mercs + mobs)
        public int TotalDamageDealt { get => Fighters.Sum(x => x.TotalDamageDealt); }
        public int TotalDamageTaken { get => Fighters.Sum(x => x.TotalDamageTaken); }

        public void AddLine(Attack line)
        {
            var attackChar = _fighters.GetOrAdd(line.Attacker, new Fighter(line.Attacker));
            attackChar.AddLine(line);
            attackChar.AddOffense(line);

            var defendChar = _fighters.GetOrAdd(line.Defender, new Fighter(line.Defender));
            defendChar.AddLine(line);
            defendChar.AddDefense(line);
        }

        // public void AddLine(Hit line)
        // {
        //     var attackChar = _fighters.GetOrAdd(line.Attacker, new Fighter(line.Attacker));
        //     attackChar.AddOffense(line);

        //     var defendChar = _fighters.GetOrAdd(line.Defender, new Fighter(line.Defender));
        //     defendChar.AddDefense(line);
        // }
        // public void AddLine(Miss line)
        // {
        //     var attackChar = _fighters.GetOrAdd(line.Attacker, new Fighter(line.Attacker));
        //     attackChar.AddOffense(line);

        //     var defendChar = _fighters.GetOrAdd(line.Defender, new Fighter(line.Defender));
        //     defendChar.AddDefense(line);
        // }

        public void AddLine(Heal line)
        {
            var healerChar = _fighters.GetOrAdd(line.Healer, new Fighter(line.Healer));

            var patientChar = _fighters.GetOrAdd(line.Patient, new Fighter(line.Patient));
        }

        // public void AddLine(Kill line)
        // {
        //     var attackChar = _fighters.GetOrAdd(line.Attacker, new Fighter(line.Attacker));
        //     attackChar.AddOffense(line);

        //     var defendChar = _fighters.GetOrAdd(line.Defender, new Fighter(line.Defender));
        //     defendChar.AddDefense(line);
        // }

        public void AddLine(Zone line) { }
        public void AddLine(Chat line) { }

    }
}
