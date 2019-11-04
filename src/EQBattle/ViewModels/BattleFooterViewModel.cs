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

        public BattleFooterViewModel()
        {
            Messenger.Instance.Subscribe("NewEQJob", x =>
            {
                eqJob = x as EQJob;
                OnPropertyChanged(nameof(FileName));
            });
            Messenger.Instance.Subscribe("NewBattle", x => Battle = x as Battle);
        }

        public Battle Battle { get => battle; set => SetProperty(ref battle, value); }
        public string FileName { get => Path.GetFileName(eqJob.FileName); }
        public TimeSpan Elapsed { get => eqJob.ProcessingElapsed; }
        public int SkirmishCount { get => battle.Skirmishes.Count; }
    }
}
