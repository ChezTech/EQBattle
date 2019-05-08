
using LogObjects;

namespace BizObjects
{
    public class DamageOverTime : Magic
    {
        // [Fri Apr 26 09:40:42 2019] You have taken 1960 damage from Nature's Searing Wrath by a cliknar sporali farmer.
        // [Fri Apr 26 09:40:54 2019] You have taken 1960 damage from Nature's Searing Wrath by a cliknar sporali farmer's corpse.
        public DamageOverTime(LogDatum logLine, string attacker, string defender, Zone zone = null) : base(logLine, attacker, defender, zone)
        {
        }
    }
}
