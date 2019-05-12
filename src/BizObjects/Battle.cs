using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace BizObjects
{
    public class Battle
    {
        public IList<Character> Characters { get; private set; }
        public IList<ILine> Lines { get; private set; }
        public string Zone { get; private set; }

        private ConcurrentDictionary<Character, Fighter> _fighters = new ConcurrentDictionary<Character, Fighter>();

        public void AddHit(Hit hit)
        {
            var attackChar = _fighters.GetOrAdd(hit.Attacker, new Fighter(hit.Attacker));
            attackChar.AddHit(hit);

            var defendChar = _fighters.GetOrAdd(hit.Defender, new Fighter(hit.Defender));
        }
    }
}
