
using LogObjects;

namespace BizObjects
{
    public class Loot : Line
    {
        public Loot(LogDatum logLine, Zone zone = null) : base(logLine, zone)
        {
        }
    }
}
