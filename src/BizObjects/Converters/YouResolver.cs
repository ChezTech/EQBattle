
using System;
using System.Collections.Generic;
using System.Linq;
using BizObjects.Battle;
using LogObjects;

namespace BizObjects.Converters
{
    public class YouResolver
    {
        private readonly List<string> reflexivePronouns = new List<string>() { "you", "your", "yourself" };
        public const string You = "You";

        public string Name { get; }

        public YouResolver(string name = You)
        {
            Name = name;
        }

        public string WhoAreYou(string argName)
        {
            if (argName == null)
                return argName;

            if (reflexivePronouns.Any(x => x.Equals(argName, StringComparison.InvariantCultureIgnoreCase)))
                return Name;

            return argName;
        }

        public bool IsThisYou(Character c) => IsThisYou(c.Name);
        public bool IsThisYou(string name)
        {
            return Name == name;
        }
    }
}
