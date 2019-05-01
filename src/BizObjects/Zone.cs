
using LogObjects;

namespace BizObjects
{
    public class Zone : Line
    {
        public Zone(LogDatum logLine, Zone zone = null) : base(logLine, zone)
        {
        }
        public string Name { get; private set; }
    }
}
