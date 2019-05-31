
using System;
using System.Collections.Concurrent;
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

        public ConcurrentDictionary<string, Type> _namesToTypes { get; } = new ConcurrentDictionary<string, Type>();

        public Type WhichType(Character c) => WhichType(c.Name);
        public Type WhichType(string name) => _namesToTypes.TryGetValue(name, out Type charType) ? charType : Type.Unknown;
        public void AddPlayer(Character c) => AddPlayer(c.Name);
        public void AddPlayer(string name) => AddCharacter(name, Type.Player);
        public void AddNonPlayer(Character c) => AddNonPlayer(c.Name);
        public void AddNonPlayer(string name) => AddCharacter(name, Type.NonPlayerCharacter);
        public void AddPet(Character c) => AddPet(c.Name);
        public void AddPet(string name) => UpdateCharacter(name, Type.Pet);
        public void AddMercenary(Character c) => AddMercenary(c.Name);
        public void AddMercenary(string name) => UpdateCharacter(name, Type.Mercenary);
        private void AddCharacter(string name, Type charType) => _namesToTypes.TryAdd(name, charType);
        private void UpdateCharacter(string name, Type charType) => _namesToTypes.AddOrUpdate(name, charType, (key, oldValue) => charType);
    }
}
