
using LogObjects;

namespace BizObjects
{
    public class Attack : Line
    {
        public Attack(LogDatum logLine, string attacker, string defender, Zone zone = null) : base(logLine, zone)
        {
            Attacker = attacker;
            Defender = defender;
        }

        public string Attacker { get; private set; }
        public string Defender { get; private set; }
    }
}
