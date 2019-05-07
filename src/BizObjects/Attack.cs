
using LogObjects;

namespace BizObjects
{
    // TODO: Make attacker/defender a Character class with a pet property so we can know whose pet to associate

    public class Attack : Line
    {
        public const string You = "You";
        public const string Unknown = "Unknown";
        private string _attacker;
        private string _defender;

        public Attack(LogDatum logLine, string attacker, string defender, bool isPet = false, Zone zone = null) : base(logLine, zone)
        {
            Attacker = attacker;
            Defender = defender;
            IsPet = isPet;
        }

        private string ReplaceCommon(string name)
        {
            return name
                .Replace("YOUR", You)
                .Replace("YOU", You)
                .Replace("You", You) // This sets us up to changing what the 'You' property actually points to (once we can figure out the character's actual name)
                .Replace("A ", "a ") // Will this get only the "A monster type" at the beginning? Could use RegEx.Replace ....
                .Replace("An ", "an ")
                // .Replace("`s", "") // backtick: better regex? (E.g. "Bob`s pet kicked ....", "Joe kicked Bob`s pet ....")
                .Replace("'s", "") // apostrophe: Can we replace this by a better Regex? (E.g. "... pierced by a monster's thorns...")
                ;
        }

        public string Attacker { get => _attacker; private set => _attacker = ReplaceCommon(value); }
        public string Defender { get => _defender; private set => _defender = ReplaceCommon(value); }
        public AttackType Type { get; private set; }
        public bool IsPet { get; private set; }
    }
}
