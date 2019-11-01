using BizObjects.Battle;
using EQJobService;
using System;
using System.IO;

namespace EQBattle.ViewModels
{
    class BattleFooterViewModel : ViewModelBase
    {
        private EQJob eqJob;
        private Battle battle;

        private string fileName;
        private TimeSpan elapsed = new TimeSpan(0, 0, 0, 4, 807);
        private int skirmishCount = 917;

        public BattleFooterViewModel()
        {
            Messenger.Instance.Subscribe("NewEQJob", x =>
            {
                eqJob = x as EQJob;
                FileName = Path.GetFileName(eqJob.FileName);
            });
            Messenger.Instance.Subscribe("NewBattle", x => Battle = x as Battle);
        }

        public Battle Battle { get => battle; set => SetProperty(ref battle, value); }
        public string FileName { get => fileName; set => SetProperty(ref fileName, value); }
        public TimeSpan Elapsed { get => elapsed; set => SetProperty(ref elapsed, value); }
        public int SkirmishCount { get => skirmishCount; set => SetProperty(ref skirmishCount, value); }
    }
}
