
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using LogObjects;

namespace BizObjects
{
    public class CharacterTracker
    {
        private const string PetPhrase = "My leader is";
        private readonly YouResolver YouAre;
        private readonly CharacterResolver CharResolver;

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
                CharResolver.AddNonPlayer(line.Defender);
            if (YouAre.IsThisYou(line.Defender) && !YouAre.IsThisYou(line.Attacker))
                CharResolver.AddNonPlayer(line.Attacker);
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
                    CharResolver.AddPlayer(chatLine.Who);
                    break;

                case "group": // could be a mercenary, maybe even a pet?
                    CharResolver.AddPlayer(chatLine.Who);
                    break;

                case "say":
                    if (chatLine.Text.StartsWith(PetPhrase))
                    {
                        CharResolver.AddPet(chatLine.Who);
                        var master = chatLine.Text.Substring(PetPhrase.Length + 1, chatLine.Text.Length - PetPhrase.Length - 2);
                        CharResolver.AddPlayer(master);
                    }
                    break;
            }
        }

        public void TrackLine(Who whoLine)
        {
            CharResolver.AddPlayer(whoLine.Character);
        }

        private void TrackCharacter(Character c)
        {
            if (c.IsPet)
                CharResolver.AddPet(c);
            if (c.IsMob)
                CharResolver.AddNonPlayer(c);
        }
    }
}
