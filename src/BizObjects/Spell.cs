
using LogObjects;

namespace BizObjects
{
    public class Spell : Line
    {
        public Character Character { get; }
        public string SpellName { get; }

        public Spell(LogDatum logLine, string name, string spellName, Zone zone = null) : base(logLine, zone)
        {
            Character = new Character(name);
            SpellName = spellName;
        }
    }
}
