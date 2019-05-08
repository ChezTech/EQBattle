
using LogObjects;

namespace BizObjects
{
    public class Attack : Line
    {
        public const string You = "You";
        public const string Unknown = "Unknown";

        public Attack(LogDatum logLine, string attacker, string defender, Zone zone = null) : base(logLine, zone)
        {
            Attacker = new Character(attacker);
            Defender = new Character(defender);
        }

        public Character Attacker { get; }
        public Character Defender { get; }
        public AttackType Type { get; }
    }
}
