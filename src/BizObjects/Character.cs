
using System;
using LogObjects;

namespace BizObjects
{
    public class Character
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
    }
}
