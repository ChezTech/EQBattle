
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

        private class CharacterMetaData
        {
            public CharacterMetaData(Type charType, bool sticky)
            {
                CharType = charType;
                Sticky = sticky;
            }

            public Type CharType { get; }

            // Set this if we are confident about the classification of a character
            // Used from the "Who" line. We know those are Players
            public bool Sticky { get; }
        }

        private ConcurrentDictionary<string, CharacterMetaData> _namesToMetaData { get; } = new ConcurrentDictionary<string, CharacterMetaData>();

        public Type WhichType(Character c) => WhichType(c.Name);
        public Type WhichType(string name) => _namesToMetaData.TryGetValue(name, out CharacterMetaData charMd) ? charMd.CharType : Type.Unknown;
        public void SetPlayer(Character c, bool sticky = false) => SetPlayer(c.Name, sticky);
        public void SetPlayer(string name, bool sticky = false) => SetCharacter(name, Type.Player, sticky);
        public void SetNonPlayer(Character c, bool sticky = false) => SetNonPlayer(c.Name, sticky);
        public void SetNonPlayer(string name, bool sticky = false) => SetCharacter(name, Type.NonPlayerCharacter, sticky);
        public void SetPet(Character c, bool sticky = false) => SetPet(c.Name, sticky);
        public void SetPet(string name, bool sticky = false) => SetCharacter(name, Type.Pet, sticky);
        public void SetMercenary(Character c, bool sticky = false) => SetMercenary(c.Name, sticky);
        public void SetMercenary(string name, bool sticky = false) => SetCharacter(name, Type.Mercenary, sticky);
        private void SetCharacter(string name, Type charType, bool sticky = false)
        {
            _namesToMetaData.AddOrUpdate(name,
                key => new CharacterMetaData(charType, sticky),
                (key, oldValue) => oldValue.Sticky ? oldValue : new CharacterMetaData(charType, sticky));
        }
    }
}
