using System.Collections.Generic;

namespace BizObjects
{
    public class Battle
    {
        public IList<Fight> Fights { get; private set; }
        public IList<Character> Fighters { get; private set; }
    }
}
