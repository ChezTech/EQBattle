using BizObjects.Battle;
using Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace EQBattle.ViewModels
{
    public class FightLogLinesViewModel : PropertyChangeBase
    {
        private Fight fight;
        public Fight Fight { get => fight; set => SetProperty(ref fight, value); }

        public FightLogLinesViewModel()
        {
            Messenger.Instance.Subscribe("OnSelectedFightChanged", x => Fight = x as Fight);
        }
    }
}
