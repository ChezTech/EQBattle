using LogObjects;

namespace BizObjects.Lines
{
    public class Zone : Line
    {
        public Zone(LogDatum logLine, string zoneName) : base(logLine)
        {
            Name = zoneName;
        }
        public string Name { get; }
    }
}
