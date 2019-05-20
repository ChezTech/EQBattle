using System.Collections.Generic;

namespace BizObjects
{
    public class War
    {
        public IList<Fight> Fights { get; private set; }
        public IList<Character> Fighters { get; private set; }
    }
}
