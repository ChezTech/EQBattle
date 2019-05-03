
using LogObjects;

namespace BizObjects
{
    public class Hit : Attack
    {
        public Hit(LogDatum logLine, string attacker, string defender, Zone zone = null) : base(logLine, attacker, defender, zone)
        {
        }

        public int Damage { get; private set; }
    }
}
