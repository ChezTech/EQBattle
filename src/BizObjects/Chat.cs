
using LogObjects;

namespace BizObjects
{
    public class Chat : Line
    {
        public Character Who { get; }

        // I suspect we're going to need a class for Channel{Name, IsUserChannel, IsCharacter}
        public string Channel { get; } // How do I know when this is a person and not a channel? How do I know it's a predefined channel (say, group, raid, ooc) vs a generic channel (General, monk)
        public string Text { get; }

        public Chat(LogDatum logLine, string who, string channel, string text, Zone zone = null) : base(logLine, zone)
        {
            Who = new Character(who);
            Channel = channel;
            Text = text;
        }
    }
}
