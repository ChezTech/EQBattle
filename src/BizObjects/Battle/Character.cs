using BizObjects.Converters;
using Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BizObjects.Battle
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Character : PropertyChangeBase, IEquatable<Character>
    {
        private string name;

        public static Character Unknown = new Character(UnknownName);
        private const string UnknownName = "Unknown";
        public string Name { get => name; private set => SetProperty(ref name, value); }
        public bool IsPet { get; }
        public bool IsMob { get; }
        public bool IsDead { get; }

        public Character(string name)
        {
            // Better to make this a builder pattern and return a static character instance of "Unknown"
            if (string.IsNullOrEmpty(name))
            {
                Name = UnknownName;
                IsPet = false;
                IsDead = false;
                return;
            }

            IsPet = DetectPet(name);
            IsDead = DetectDeadMob(name);

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

        private bool DetectDeadMob(string name)
        {
            // Check if a name contains "mob's corpse", just the normal apostrophe "'s corpse"
            // A mob can have corpse as part of its name (not as indication of state of living), but the game uses a backtick for that (thank goodness)
            // "a mercenary`s corpse"

            // There is one exception, "Garzicor's Corpse" that uses a normal apostrophe "'s", but it also uses a capital "Corpse" so doesn't match our pattern and we're all good.
            // https://everquest.allakhazam.com/db/npc.html?id=7789

            if (name.Contains("'s corpse"))
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
                return string.Format($"{Name} {(IsPet ? "P" : "")}{(IsMob ? "M" : "")}{(IsDead ? "D" : "")}");
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
