
using LogObjects;

namespace BizObjects
{
    public class Kill : Attack
    {
        public Kill(LogDatum logLine, string attacker, string defender, bool isPet = false, Zone zone = null) : base(logLine, attacker, defender, isPet, zone)
        {
        }

        public override string ToString()
        {
            return string.Format("Kill: {0} slewth {1}", Attacker, Defender);
        }
    }
}
