
using LogObjects;

namespace BizObjects
{
    public class Kill : Attack
    {
        public Kill(LogDatum logLine, Zone zone = null) : base(logLine, zone)
        {
        }
    }
}
