
using LogObjects;

namespace BizObjects
{
    public class Kill : Attack
    {
        public Kill(LogDatum logLine, string attacker, string defender, string verb, Zone zone = null) : base(logLine, attacker, defender, verb, zone)
        {
        }

        public override string ToString()
        {
            return string.Format("Kill: {0} slewth {1}", Attacker, Defender);
        }
    }
}
