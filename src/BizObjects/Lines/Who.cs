using BizObjects.Battle;
using LogObjects;

namespace BizObjects.Lines
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
        public bool IsAfk { get; }
        public bool IsLfg { get; }

        public Who(LogDatum logLine, string name, int level, string title, string @class, string race, string guild, bool isAnon, bool isAfk, bool isLfg, Zone zone = null) : base(logLine, zone)
        {
            Character = new Character(name);
            Level = level;
            Title = title;
            Class = @class;
            Race = race;
            Guild = guild;
            IsAnonymous = isAnon;
            IsAfk = isAfk;
            IsLfg = isLfg;
        }
    }
}
