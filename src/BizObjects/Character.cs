
using LogObjects;

namespace BizObjects
{
    public class Character
    {
        public const string You = "You";

        public string Name { get; }
        public bool IsPet { get; }

        public Character(string name)
        {
            Name = CleanName(name);
            IsPet = false;
        }

        private string CleanName(string name)
        {
            return name
                .Replace("YOUR", You)
                .Replace("YOU", You)
                .Replace("You", You) // This sets us up to changing what the 'You' property actually points to (once we can figure out the character's actual name)
                .Replace("A ", "a ") // Will this get only the "A monster type" at the beginning? Could use RegEx.Replace ....
                .Replace("An ", "an ")
                // // .Replace("`s", "") // backtick: better regex? (E.g. "Bob`s pet kicked ....", "Joe kicked Bob`s pet ....")
                // .Replace("'s", "") // apostrophe: Can we replace this by a better Regex? (E.g. "... pierced by a monster's thorns...")
                ;
        }
    }
}
