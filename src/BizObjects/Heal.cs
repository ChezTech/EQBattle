

using LogObjects;

namespace BizObjects
{
    public class Heal : Line
    {
        public Character Healer { get; }
        public Character Patient { get; }
        public int Amount { get; }
        public int MaxAmount { get; }
        public string SpellName { get; }
        public bool isHealOverTime { get; }
        public string Qualifier { get; }

        public Heal(LogDatum logLine, string healer, string patient, int amount, int maxAmount, string spellName, bool isHot, string qualifier, Zone zone = null) : base(logLine, zone)
        {
            Healer = new Character(healer);
            Patient = new Character(patient);
            Amount = amount;
            MaxAmount = maxAmount;
            SpellName = spellName;
            isHealOverTime = isHot;
            Qualifier = qualifier;
        }

    }
}
