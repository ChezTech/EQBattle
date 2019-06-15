

using System.Collections.Generic;
using LogObjects;

namespace BizObjects
{
    public class Heal : Line
    {
        private readonly List<string> reflexivePronouns = new List<string>() { "himself", "herself", "itself" };

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
            Patient = new Character(reflexivePronouns.Contains(patient) ? healer : patient);
            Amount = amount;
            OverAmount = maxAmount;
            SpellName = spellName;
            isHealOverTime = isHot;
            Qualifier = qualifier;
        }

    }
}
