
using System;
using System.Collections.Generic;
using System.Linq;
using LogObjects;

namespace BizObjects
{
    /// <Summary>
    /// Figures out if the character is a PC, NPC/Mob, Pet or Mercenary. Especially useful for named mobs
    /// </Summary>
    public class CharacterResolver
    {
        public enum Type
        {
            Unknown,
            Player,
            NonPlayerCharacter,
            Pet,
            Mercenary,
        }

        public ISet<string> PCNames { get; } = new HashSet<string>();
        public IEnumerable<string> NPCNames { get; } = new HashSet<string>();
        public IEnumerable<string> PetNames { get; } = new HashSet<string>();
        public IEnumerable<string> MercenaryNames { get; } = new HashSet<string>();

        public Type WhichType(Character c) => WhichType(c.Name);
        public Type WhichType(string name)
        {
            if (PCNames.Contains(name)) return Type.Player;
            if (NPCNames.Contains(name)) return Type.NonPlayerCharacter;
            if (PetNames.Contains(name)) return Type.Pet;
            if (MercenaryNames.Contains(name)) return Type.Mercenary;
            return Type.Unknown;
        }

        public void AddPlayer(Character c) => AddPlayer(c.Name);
        public void AddPlayer(string name) => PCNames.Add(name);
    }
}
