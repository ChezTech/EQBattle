
using LogObjects;

namespace BizObjects
{
    public class Kill : Attack
    {
        public Kill(LogDatum logLine, string attacker, string defender, Zone zone = null) : base(logLine, attacker, defender, zone)
        {
        }

        public override string ToString()
        {
            return string.Format("Kill: {0} slewth {1}", Attacker, Defender);
        }
    }
}
