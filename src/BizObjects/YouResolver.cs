
using System;
using LogObjects;

namespace BizObjects
{
    public class YouResolver
    {
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

            if (argName.Equals("Your", StringComparison.InvariantCultureIgnoreCase))
                return Name;
            if (argName.Equals("You", StringComparison.InvariantCultureIgnoreCase))
                return Name;

            return argName;
        }
    }
}
