
using LogObjects;

namespace BizObjects
{
    public class Miss : Attack
    {
        public Miss(LogDatum logLine, Zone zone = null) : base(logLine, zone)
        {
        }
    }
}
