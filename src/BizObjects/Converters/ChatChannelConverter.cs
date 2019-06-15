using System.Collections.Generic;

namespace BizObjects
{
    public class ChatChannelConverter
    {
        private readonly IDictionary<string, string> _nameToChannelMap = new Dictionary<string, string>()
        {
            // NOTE: Put plural types first so the RegEx finds those complete words first...
            {"tells", "Tell" },
            {"tell", "Tell" },
            {"told", "Tell" },
            {"auctions", "Auction" },
            {"auction", "Auction" },
            {"shouts", "Shout" },
            {"shout", "Shout" },
            {"says", "Say" },
            {"say", "Say" },
            {"guild", "Guild" },
            {"raid", "Raid" },
            {"group", "Group" },
            {"party", "Group" },
            {"you", "Tell" },   // Don't want this in the list of chat verbs/names used in the RegEx
            {"out of character", "OOC" },
        };

        /// Converts the given chatType into an official chat channel or returns the original chatType
        public string Convert(string chatType)
        {
            return _nameToChannelMap.TryGetValue(chatType, out string chatChannel) ? chatChannel : chatType;
        }

        public ICollection<string> Names { get { return _nameToChannelMap.Keys; } }
    }
}
