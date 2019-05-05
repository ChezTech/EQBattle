
using LogObjects;

namespace BizObjects
{
    public class Miss : Attack
    {
        public Miss(LogDatum logLine, string attacker, string defender, bool isPet = false, Zone zone = null) : base(logLine, attacker, defender, isPet, zone)
        {
        }
    }
}
