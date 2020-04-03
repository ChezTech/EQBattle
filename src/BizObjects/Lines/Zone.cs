using LogObjects;

namespace BizObjects.Lines
{
    public class Zone : Line
    {
        public static Zone Unknown = new Zone(null, "Unknown");

        public Zone(LogDatum logLine, string zoneName) : base(logLine)
        {
            Name = zoneName;
        }
        public string Name { get; }
    }
}
