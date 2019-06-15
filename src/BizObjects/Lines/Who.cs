
using LogObjects;

namespace BizObjects
{
    public class Who : Line
    {
        public Character Character { get; }
        public int Level { get; }
        public string Title { get; }
        public string Class { get; }
        public string Race { get; }
        public string Guild { get; }
        public bool IsAnonymous { get; }

        public Who(LogDatum logLine, string name, int level, string title, string @class, string race, string guild, bool isAnon, Zone zone = null) : base(logLine, zone)
        {
            Character = new Character(name);
            Level = level;
            Title = title;
            Class = @class;
            Race = race;
            Guild = guild;
            IsAnonymous = isAnon;
        }
    }
}
