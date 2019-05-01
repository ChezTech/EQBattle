

namespace BizObjects
{
    public class Heal : Line
    {
        public string Healer { get; private set; }
        public string Patient { get; private set; }
        public int Amount { get; private set; }
        public int OverAmount { get; private set; }
        public string SpellName { get; private set; }

    }
}
