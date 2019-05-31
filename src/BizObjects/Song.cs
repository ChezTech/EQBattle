
using LogObjects;

namespace BizObjects
{
    public class Song : Line
    {
        public Character Character { get; }
        public string SongName { get; }

        public Song(LogDatum logLine, string name, string songName, Zone zone = null) : base(logLine, zone)
        {
            Character = new Character(name);
            SongName = songName;
        }
    }
}
