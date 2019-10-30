using BizObjects.Battle;
using System;
using System.Collections.Generic;
using System.Text;

namespace EQBattle.ViewModels
{
    class FightListViewModel : ViewModelBase
    {
        private Battle battle;

        public FightListViewModel()
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
