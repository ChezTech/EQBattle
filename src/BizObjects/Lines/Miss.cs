using LogObjects;

namespace BizObjects.Lines
{
    public class Miss : Attack
    {
        public string DefenseType { get; set; }
        public string Qualifier { get; }

        public Miss(LogDatum logLine, string attacker, string defender, string verb, string defenseType, string qualifier, Zone zone = null) : base(logLine, attacker, defender, verb, zone)
        {
            DefenseType = defenseType;
            Qualifier = qualifier;
        }
    }
}
