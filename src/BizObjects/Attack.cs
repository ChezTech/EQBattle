
using LogObjects;

namespace BizObjects
{
    public class Attack : Line
    {
        public const string You = "You";
        public const string Unknown = "Unknown";

        public Attack(LogDatum logLine, string attacker, string defender, Zone zone = null) : base(logLine, zone)
        {
            Attacker = ReplaceCommon(attacker);
            Defender = ReplaceCommon(defender);
        }

        private string ReplaceCommon(string name)
        {
            return name
                .Replace("A ", "a "); // Will this get only the "A monster type" at the beginning? Could use RegEx.Replace ....
        }

        public string Attacker { get; private set; }
        public string Defender { get; private set; }
    }
}
