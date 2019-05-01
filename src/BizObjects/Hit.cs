
using LogObjects;

namespace BizObjects
{
    public class Hit : Attack
    {
        public Hit(LogDatum logLine, Zone zone = null) : base(logLine, zone)
        {
        }

        public int Damage { get; private set; }
    }
}
