﻿using BizObjects.Battle;
using Core;
using EQJobService;
using System;
using System.IO;
using System.Windows.Threading;

namespace EQBattle.ViewModels
{
    class BattleFooterViewModel : ViewModelBase
    {
        private EQJob eqJob;
        private Battle battle;

        public BattleFooterViewModel()
        {
            // Update the Footer view even small interval to refresh properties from the EQJob (elapsed time, number of skirmishes)
            // Elapsed property doesn't have a event or INotifyPropertyChanged going on for it (doesn't really make sense)
            // So, just poll (instead of push) the property update
            StartDispatchTimer(250, () =>
            {
                OnPropertyChanged(nameof(Elapsed));
                OnPropertyChanged(nameof(SkirmishCount));
            });

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
    }
}
