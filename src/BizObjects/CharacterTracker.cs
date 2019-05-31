
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
        private readonly CharacterResolver _charResolver;

        public CharacterTracker(CharacterResolver cr)
        {
            _charResolver = cr;
        }

        public void TrackLine(ILine line)
        {
            // Do nothing in this general one
        }

        public void TrackLine(Attack line)
        {
            TrackCharacter(line.Attacker);
            TrackCharacter(line.Defender);
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
                    _charResolver.AddPlayer(chatLine.Who);
                    break;

                case "group": // could be a mercenary, maybe even a pet?
                    _charResolver.AddPlayer(chatLine.Who);
                    break;

                case "say":
                    if (chatLine.Text.StartsWith(PetPhrase))
                    {
                        _charResolver.AddPet(chatLine.Who);
                        var master = chatLine.Text.Substring(PetPhrase.Length + 1, chatLine.Text.Length - PetPhrase.Length - 2);
                        _charResolver.AddPlayer(master);
                    }
                    break;
            }
        }

        public void TrackLine(Who whoLine)
        {
            _charResolver.AddPlayer(whoLine.Character);
        }

        private void TrackCharacter(Character c)
        {
            if (c.IsPet)
                _charResolver.AddPet(c);
            if (c.IsMob)
                _charResolver.AddNonPlayer(c);
        }
    }
}
