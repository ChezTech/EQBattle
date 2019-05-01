
using LogObjects;

namespace BizObjects
{
    public class Chat : Line
    {
        public Chat(LogDatum logLine, Zone zone = null) : base(logLine, zone)
        {
        }
    }
}
