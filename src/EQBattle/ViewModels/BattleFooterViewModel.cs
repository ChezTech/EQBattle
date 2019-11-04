using BizObjects.Battle;
using EQJobService;
using System;
using System.IO;

namespace EQBattle.ViewModels
{
    public interface IRefresh
    {
        void Refresh();
    }

    class BattleFooterViewModel : ViewModelBase, IRefresh
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
        public string FileName => Path.GetFileName(eqJob?.FileName);
        public TimeSpan Elapsed => eqJob?.ProcessingElapsed ?? TimeSpan.Zero;
        public int SkirmishCount => battle?.Skirmishes.Count ?? 0;

        public void Refresh()
        {
            OnPropertyChanged(nameof(Elapsed));
            OnPropertyChanged(nameof(SkirmishCount));
        }
    }
}
