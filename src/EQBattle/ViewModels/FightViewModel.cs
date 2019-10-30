using BizObjects.Battle;
using System;
using System.Collections.Generic;
using System.Text;

namespace EQBattle.ViewModels
{
    class FightViewModel : ViewModelBase
    {
        private Battle battle;

        public FightViewModel()
        {
            Messenger.Instance.Subscribe("NewBattle", x => NewBattle(x));
        }

        private void NewBattle(object x)
        {
            Battle = x as Battle;
        }

        public Battle Battle { get => battle; set => SetProperty(ref battle, value); }
    }
}
