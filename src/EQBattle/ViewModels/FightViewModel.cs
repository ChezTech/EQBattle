using BizObjects.Battle;
using System;
using System.Collections.Generic;
using System.Text;

namespace EQBattle.ViewModels
{
    class FightViewModel : ViewModelBase
    {
        private Battle battle;
        private Skirmish skirmish;

        public FightViewModel()
        {
            Messenger.Instance.Subscribe("NewBattle", x => NewBattle(x));
            Messenger.Instance.Subscribe("OnSelectedSkirmishChanged", x => Skirmish = x as Skirmish);
        }

        private void NewBattle(object x)
        {
            Battle = x as Battle;
        }

        public Battle Battle { get => battle; set => SetProperty(ref battle, value); }
        public Skirmish Skirmish { get => skirmish; set => SetProperty(ref skirmish, value); }
    }
}
