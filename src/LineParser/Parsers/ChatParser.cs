using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using BizObjects;
using LogObjects;

namespace LineParser.Parsers
{
    public class ChatParser : IParser
    {
        private readonly ChatChannelConverter ChatConverter = new ChatChannelConverter();
        private readonly Regex RxChat = new Regex("(.+) (tells|tell|told|says|say|shout|shouts|auctions|auction)(?: (?:your |to your |the )?(.+))?, {1,2}'(.+)'", RegexOptions.Compiled); // https://regex101.com/r/aojusQ/3
        private readonly YouResolver YouAre;

        private readonly IDictionary<string, string> ChannelMap = new Dictionary<string, string>();

        public ChatParser(YouResolver youAre)
        {
            YouAre = youAre;
        }

        public bool TryParse(LogDatum logDatum, out ILine lineEntry)
        {
            if (TryParseChat(logDatum, out lineEntry))
                return true;

            return false;
        }
        private bool TryParseChat(LogDatum logDatum, out ILine lineEntry)
        {
            var match = RxChat.Match(logDatum.LogMessage);

            if (!match.Success)
            {
                lineEntry = null;
                return false;
            }

            var who = match.Groups[1].Value;
            var verb = match.Groups[2].Value;
            var channel = match.Groups[3].Value;
            var text = match.Groups[4].Value;

            who = YouAre.WhoAreYou(who);
            channel = ModifyChannel(string.IsNullOrEmpty(channel) ? verb : channel);
            var server = CheckNameForServer(ref who);

            lineEntry = new Chat(logDatum, who, channel, text, server);

            return true;
        }

        private string CheckNameForServer(ref string who)
        {
            // User channels can be used cross server: "<serverName>.<charName> tells Channel:#, 'blah blah'."
            var split = who.Split('.');

            if (split.Length == 2)
            {
                who = split[1];
                return split[0];
            }
            return null;
        }

        private string ModifyChannel(string channel)
        {
            // If it's a user channel it'll have a :# at the end, "monk:4", strip that off
            int colonIndex = channel.IndexOf(':');
            if (colonIndex != -1)
                return channel.Substring(0, colonIndex);

            return ChatConverter.Convert(channel);
        }
    }
}
