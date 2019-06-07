
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
        public void SetPlayer(Character c) => SetPlayer(c.Name);
        public void SetPlayer(string name) => SetCharacter(name, Type.Player);
        public void SetNonPlayer(Character c) => SetNonPlayer(c.Name);
        public void SetNonPlayer(string name) => SetCharacter(name, Type.NonPlayerCharacter);
        public void SetPet(Character c) => SetPet(c.Name);
        public void SetPet(string name) => UpdateCharacter(name, Type.Pet);
        public void SetMercenary(Character c) => SetMercenary(c.Name);
        public void SetMercenary(string name) => UpdateCharacter(name, Type.Mercenary);
        private void SetCharacter(string name, Type charType) => _namesToTypes.TryAdd(name, charType);
        private void UpdateCharacter(string name, Type charType) => _namesToTypes.AddOrUpdate(name, charType, (key, oldValue) => charType);
    }
}
