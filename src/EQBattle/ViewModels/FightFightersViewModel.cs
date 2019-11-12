using Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace EQBattle.ViewModels
{
    public class FightFightersViewModel : PropertyChangeBase
    {
        private Fight fight;

        public FightFightersViewModel()
        {
            Messenger.Instance.Subscribe("OnSelectedFightChanged", x => Fight = x as Fight);
        }

        public Fight Fight { get => fight; set => SetProperty(ref fight, value); }
    }
}
