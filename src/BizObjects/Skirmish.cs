using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace BizObjects
{
    public class Skirmish : Fight
    {
        public Skirmish(YouResolver youAre) : base(youAre)
        {
        }

        public IList<Fight> Fights { get; }

        public override void AddLine(Attack line)
        {
            // Find which fight this Attack belongs to and put it in there...


            // var attackChar = _fighters.GetOrAdd(line.Attacker, new Fighter(line.Attacker, this));
            // attackChar.AddOffense(line);
            // OffensiveStatistics.AddLine(line);

            // var defendChar = _fighters.GetOrAdd(line.Defender, new Fighter(line.Defender, this));
            // defendChar.AddDefense(line);
            // DefensiveStatistics.AddLine(line);
        }
    }
}
