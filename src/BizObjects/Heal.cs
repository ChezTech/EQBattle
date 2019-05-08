

using LogObjects;

namespace BizObjects
{
    public class Heal : Line
    {
        public Heal(LogDatum logLine, Zone zone = null) : base(logLine, zone)
        {
        }

        public string Healer { get; }
        public string Patient { get; }
        public int Amount { get; }
        public int OverAmount { get; }
        public string SpellName { get; }

    }
}
