

using LogObjects;

namespace BizObjects
{
    public class Heal : Line
    {
        public const string Itself = "itself";

        public Character Healer { get; }
        public Character Patient { get; }
        public int Amount { get; }
        public int OverAmount { get; }
        public string SpellName { get; }
        public bool isHealOverTime { get; }
        public string Qualifier { get; }

        public Heal(LogDatum logLine, string healer, string patient, int amount, int maxAmount, string spellName, bool isHot, string qualifier, Zone zone = null) : base(logLine, zone)
        {
            Healer = new Character(healer);
            Patient = new Character(patient == Itself ? healer : patient);
            Amount = amount;
            OverAmount = maxAmount;
            SpellName = spellName;
            isHealOverTime = isHot;
            Qualifier = qualifier;
        }

    }
}
