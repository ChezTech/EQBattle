
using LogObjects;

namespace BizObjects
{
    public class Attack : Line
    {
        public const string You = "You";
        public const string Unknown = "Unknown";

        private static AttackTypeConverter ATConverter = new AttackTypeConverter(); // TODO: DI this

        public Attack(LogDatum logLine, string attacker, string defender, string verb, Zone zone = null) : base(logLine, zone)
        {
            Attacker = new Character(attacker);
            Defender = new Character(defender);
            Verb = verb;
            Type = ATConverter.Convert(Verb);
        }

        public Character Attacker { get; }
        public Character Defender { get; }
        public AttackType Type { get; }

        public string Verb { get; }
    }
}
