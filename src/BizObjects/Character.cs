
using LogObjects;

namespace BizObjects
{
    public class Character
    {
        public string Name { get; }
        public bool IsPet { get; }

        public Character(string name)
        {
            Name = name;
            IsPet = false;
        }
    }
}
