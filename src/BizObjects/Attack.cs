
using LogObjects;

namespace BizObjects
{
    public class Attack : Line
    {
        public Attack(LogDatum logLine, Zone zone = null) : base(logLine, zone)
        {
        }

        public string Attacker { get; private set; }
        public string Defender { get; private set; }
    }
}
