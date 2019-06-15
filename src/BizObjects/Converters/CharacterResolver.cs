
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BizObjects.Battle;
using LogObjects;

namespace BizObjects.Converters
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

        public void SetPlayer(Character c, bool sticky = false, bool overwrite = true) => SetPlayer(c.Name, sticky, overwrite);
        public void SetPlayer(string name, bool sticky = false, bool overwrite = true) => SetCharacter(name, Type.Player, sticky, overwrite);
        public void SetNonPlayer(Character c, bool sticky = false, bool overwrite = true) => SetNonPlayer(c.Name, sticky, overwrite);
        public void SetNonPlayer(string name, bool sticky = false, bool overwrite = true) => SetCharacter(name, Type.NonPlayerCharacter, sticky, overwrite);
        public void SetPet(Character c, bool sticky = false, bool overwrite = true) => SetPet(c.Name, sticky, overwrite);
        public void SetPet(string name, bool sticky = false, bool overwrite = true) => SetCharacter(name, Type.Pet, sticky, overwrite);
        public void SetMercenary(Character c, bool sticky = false, bool overwrite = true) => SetMercenary(c.Name, sticky, overwrite);
        public void SetMercenary(string name, bool sticky = false, bool overwrite = true) => SetCharacter(name, Type.Mercenary, sticky, overwrite);
        private void SetCharacter(string name, Type charType, bool sticky = false, bool overwrite = true)
        {
            _namesToMetaData.AddOrUpdate(name,
                key => new CharacterMetaData(charType, sticky),
                (key, oldValue) => oldValue.Sticky ? oldValue : overwrite ? new CharacterMetaData(charType, sticky) : oldValue);
        }
    }
}
