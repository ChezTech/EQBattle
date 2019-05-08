
using LogObjects;

namespace BizObjects
{
    public class Attack : Line
    {
        public const string You = "You";
        public const string Unknown = "Unknown";

        public Attack(LogDatum logLine, string attacker, string defender, bool isPet = false, Zone zone = null) : base(logLine, zone)
        {
            Attacker = new Character(attacker);
            Defender = new Character(defender);
            IsPet = isPet;
        }

        public Character Attacker { get; }
        public Character Defender { get; }
        public AttackType Type { get; private set; }
        public bool IsPet { get; private set; }
    }
}
