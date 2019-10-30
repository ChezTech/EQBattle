using BizObjects.Battle;
using System;
using System.Collections.Generic;
using System.Text;

namespace EQBattle.ViewModels
{
    class BattleFooterViewModel : ViewModelBase
    {
        private Battle battle;
        private string fileName = @"eqlog_Khadaji_test.txt";
        private TimeSpan elapsed = new TimeSpan(0, 0, 0, 4, 807);
        private int skirmishCount = 917;

        public BattleFooterViewModel()
        {
            Messenger.Instance.Subscribe("NewBattle", x => NewBattle(x));
        }

        private void NewBattle(object x)
        {
            Battle = x as Battle;
        }

        public Battle Battle { get => battle; set => SetProperty(ref battle, value); }
        public string FileName { get => fileName; set => SetProperty(ref fileName, value); }
        public TimeSpan Elapsed { get => elapsed; set => SetProperty(ref elapsed, value); }
        public int SkirmishCount { get => skirmishCount; set => SetProperty(ref skirmishCount, value); }
    }
}
