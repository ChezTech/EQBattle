
using LogObjects;

namespace BizObjects
{
    public class Magic : Attack
    {
        public Magic(LogDatum logLine, string attacker, string defender, Zone zone = null) : base(logLine, attacker, defender, zone)
        {
        }
    }
}
