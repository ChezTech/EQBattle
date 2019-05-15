
using System;

namespace BizObjects
{
    public class Character : IEquatable<Character>
    {
        public string Name { get; }
        public bool IsPet { get; }

        public Character(string name)
        {
            IsPet = DetectPet(name);
            Name = CleanName(name);
        }

        private bool DetectPet(string name)
        {
            return name.Contains("`s pet");
        }

        private string CleanName(string name)
        {
            return name
                .Replace("A ", "a ") // Will this get only the "A monster type" at the beginning? Could use RegEx.Replace ....
                .Replace("An ", "an ")
                .Replace("`s pet", "") // backtick: better regex? (E.g. "Bob`s pet kicked ....", "Joe kicked Bob`s pet ....")
                .Replace("'s", "") // apostrophe: Can we replace this by a better Regex? (E.g. "... pierced by a monster's thorns...")
                ;
        }

        #region Equality
        // public override bool Equals(object obj)
        // {
        //     return obj is Character character &&
        //            Name == character.Name &&
        //            IsPet == character.IsPet;
        // }

        // public bool Equals(Character other)
        // {
        //     return Name == other.Name &&
        //            IsPet == other.IsPet;
        // }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, IsPet);
        }



        public override bool Equals(object other)
        {
            return Equals(other as Character);
        }


        // https://stackoverflow.com/a/4420958
        public bool Equals(Character other)
        {
            if ((object)other == null)
                return false;
            return Name == other.Name &&
                       IsPet == other.IsPet;
        }

        public static bool operator ==(Character left, Character right)
        {
            return object.Equals(left, right);
        }

        public static bool operator !=(Character left, Character right)
        {
            return !(left == right);
        }
        #endregion

    }
}
