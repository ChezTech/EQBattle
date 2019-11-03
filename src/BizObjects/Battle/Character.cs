using BizObjects.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BizObjects.Battle
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

            var normalizedName = CharacterNameNormalizer.Instance.NormalizeName(name);
            Name = CleanName(normalizedName);

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
                .Replace("`s pet", "") // backtick: better regex? (E.g. "Bob`s pet kicked ....", "Joe kicked Bob`s pet ....")
                .Replace("`s warder", "")
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
