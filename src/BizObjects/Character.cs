
using System;
using System.Collections.Generic;

namespace BizObjects
{
    public class Character : IEquatable<Character>
    {
        private const string UnknownName = "Unknown";
        public string Name { get; }
        public bool IsPet { get; }

        public Character(string name)
        {
            // Better to make this a builder pattern and return a static character instance of "Unknown"
            if (string.IsNullOrEmpty(name))
            {
                Name = UnknownName;
                IsPet = false;
                return;
            }

            IsPet = DetectPet(name);
            Name = CleanName(name);
        }

        private bool DetectPet(string name)
        {
            // TODO: Listen for `/pet who leader` log message
            // https://www.reddit.com/r/everquest/comments/6rkmyd/reliable_parser/
            return name.Contains("`s pet");
        }

        private string CleanName(string name)
        {
            return name
                .Replace("A ", "a ") // Will this get only the "A monster type" at the beginning? Could use RegEx.Replace ....
                .Replace("An ", "an ")

                // Note: this is for an apostrophe corpse ... there are actual mobs that are corpses. Those names use a backtick, e.g. "a mercenary`s corpse".
                // We don't want to remove that term from their name as it is part of their proper name, not a state of being.
                .Replace("'s corpse", "") // dead mobs can still have a DoT in effect, "You have taken 1960 damage from Nature's Searing Wrath by a cliknar sporali farmer's corpse."

                .Replace("`s pet", "") // backtick: better regex? (E.g. "Bob`s pet kicked ....", "Joe kicked Bob`s pet ....")
                .Replace("'s", "") // apostrophe: Can we replace this by a better Regex? (E.g. "... pierced by a monster's thorns...")

                ;
        }

        #region Equality
        public override bool Equals(object obj)
        {
            return Equals(obj as Character);
        }

        public bool Equals(Character other)
        {
            return other != null &&
                   Name == other.Name &&
                   IsPet == other.IsPet;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, IsPet);
        }

        public static bool operator ==(Character left, Character right)
        {
            return EqualityComparer<Character>.Default.Equals(left, right);
        }

        public static bool operator !=(Character left, Character right)
        {
            return !(left == right);
        }
        #endregion
    }
}
