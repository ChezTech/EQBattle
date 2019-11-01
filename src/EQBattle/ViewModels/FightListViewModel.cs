using BizObjects.Battle;
using System;
using System.Collections.Generic;
using System.Text;

namespace EQBattle.ViewModels
{
    class FightListViewModel : ViewModelBase
    {
        private Battle battle;
        private Skirmish selectedSkirmish;

        public FightListViewModel()
        {
            Messenger.Instance.Subscribe("NewBattle", x => NewBattle(x));
        }

        private void NewBattle(object x)
        {
            Battle = x as Battle;
        }

        public Battle Battle { get => battle; set => SetProperty(ref battle, value); }

        public Skirmish SelectedSkirmish
        {
            get => selectedSkirmish;
            set
            {
                if (SetProperty(ref selectedSkirmish, value))
                    Messenger.Instance.Publish("OnSelectedSkirmishChanged", SelectedSkirmish);
            }
        }
    }
}
