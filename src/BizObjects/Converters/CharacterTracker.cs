
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BizObjects.Battle;
using BizObjects.Lines;
using LogObjects;

namespace BizObjects.Converters
{
    public class CharacterTracker
    {
        private const string PetPhrase = "My leader is";
        private readonly YouResolver YouAre;
        private readonly CharacterResolver CharResolver;
        private static List<CharacterResolver.Type> GoodCharacterTypes = new List<CharacterResolver.Type>() { CharacterResolver.Type.Player, CharacterResolver.Type.Mercenary, CharacterResolver.Type.Pet };

        public CharacterTracker(YouResolver youAre, CharacterResolver charResolver)
        {
            YouAre = youAre;
            CharResolver = charResolver;
        }

        public void TrackLine(ILine line)
        {
            // Do nothing in this general one
        }

        public void TrackLine(Attack line)
        {
            TrackCharacter(line.Attacker);
            TrackCharacter(line.Defender);

            // If "You" are attacking/defending then the other character is a mob (as long as it's also not yourself, which can happen witha Cannibalize spell)!
            if (YouAre.IsThisYou(line.Attacker) && !YouAre.IsThisYou(line.Defender))
                CharResolver.SetNonPlayer(line.Defender);
            if (YouAre.IsThisYou(line.Defender) && !YouAre.IsThisYou(line.Attacker))
                CharResolver.SetNonPlayer(line.Attacker);
        }

        private bool IsGoodPlayer(Character c) => GoodCharacterTypes.Contains(CharResolver.WhichType(c));

        public void TrackLine(Heal line)
        {
            TrackCharacter(line.Healer);
            TrackCharacter(line.Patient);

            // If "You" are healing/being healed then the other character is a PC or Merc
            if (YouAre.IsThisYou(line.Healer))
                CharResolver.SetMercenary(line.Patient);
            if (YouAre.IsThisYou(line.Patient))
                CharResolver.SetMercenary(line.Healer);

            // If either char is a PC/merc/pet, then the other is a Merc. This won't override a Player type (if detect by /who line)
            if (IsGoodPlayer(line.Healer))
                CharResolver.SetMercenary(line.Patient, overwrite: false);
            if (IsGoodPlayer(line.Patient))
                CharResolver.SetMercenary(line.Healer, overwrite: false);
        }

        public void TrackLine(Song line)
        {
            TrackCharacter(line.Character);
        }

        public void TrackLine(Spell line)
        {
            TrackCharacter(line.Character);
        }

        public void TrackLine(Chat chatLine)
        {
            switch (chatLine.Channel)
            {
                // These channels are always PCs (are they?)
                case "ooc":
                case "shout":
                case "guild":
                case "raid": // Do merc's chat on raid channel?
                case "General":
                    CharResolver.SetPlayer(chatLine.Who);
                    break;

                case "group": // could be a mercenary, maybe even a pet?
                    CharResolver.SetPlayer(chatLine.Who);
                    break;

                case "say":
                    if (chatLine.Text.StartsWith(PetPhrase))
                    {
                        CharResolver.SetPet(chatLine.Who);
                        var master = chatLine.Text.Substring(PetPhrase.Length + 1, chatLine.Text.Length - PetPhrase.Length - 2);
                        CharResolver.SetPlayer(master);
                    }
                    break;
            }
        }

        public void TrackLine(Who whoLine)
        {
            // Anyone listed via the "/who" command will be Player Characters
            CharResolver.SetPlayer(whoLine.Character, true);
        }

        private void TrackCharacter(Character c)
        {
            if (c.IsPet)
                CharResolver.SetPet(c);
            if (c.IsMob)
                CharResolver.SetNonPlayer(c);
        }
    }
}
