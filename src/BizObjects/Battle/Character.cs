
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BizObjects
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Character : IEquatable<Character>
    {
        public static Character Unknown = new Character(UnknownName);
        private const string UnknownName = "Unknown";
        public string Name { get; }
        public bool IsPet { get; }
        public bool IsMob { get; }

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
            IsMob = DetectMob(Name);
        }

        private bool DetectPet(string name)
        {
            // TODO: Listen for `/pet who leader` log message
            // https://www.reddit.com/r/everquest/comments/6rkmyd/reliable_parser/
            // This will also get confusing when an Enc charms a mob to make it their pet
            if (name.Contains("`s pet"))
                return true;
            if (name.Contains("`s warder"))
                return true;

            return false;
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
                .Replace("`s warder", "")
                .Replace("'s", "") // apostrophe: Can we replace this by a better Regex? (E.g. "... pierced by a monster's thorns...")

                ;
        }

        private bool DetectMob(string name)
        {
            // Generic mob
            if (name.StartsWith("a "))
                return true;
            if (name.StartsWith("an "))
                return true;

            // How to tell if this is a named mob?
            // - a space in its name?
            if (name.Contains(' '))
                return true;

            return false;
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

        private string DebuggerDisplay
        {
            get
            {
                return string.Format($"{Name} {(IsPet ? "P" : "")}{(IsMob ? "M" : "")}");
            }
        }

        public override string ToString()
        {
            return ToString("G");
        }

        public string ToString(string format)
        {
            switch (format)
            {
                case "G":
                default:
                    return DebuggerDisplay;
            }
        }
    }
}
