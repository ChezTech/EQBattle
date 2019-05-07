
using LogObjects;

namespace BizObjects
{
    // TODO: Make attacker/defender a Character class with a pet property so we can know whose pet to associate

    public class Attack : Line
    {
        public const string You = "You";
        public const string Unknown = "Unknown";

        public Attack(LogDatum logLine, string attacker, string defender, bool isPet = false, Zone zone = null) : base(logLine, zone)
        {
            Attacker = ReplaceCommon(attacker);
            Defender = ReplaceCommon(defender);
            IsPet = isPet;
        }

        private string ReplaceCommon(string name)
        {
            return name
                .Replace("YOU", You)
                .Replace("A ", "a "); // Will this get only the "A monster type" at the beginning? Could use RegEx.Replace ....
        }

        public string Attacker { get; private set; }
        public string Defender { get; private set; }
        public AttackType Type { get; private set; }
        public bool IsPet { get; private set; }
    }
}
