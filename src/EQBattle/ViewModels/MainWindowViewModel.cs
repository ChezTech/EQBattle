﻿using BizObjects.Battle;
using BizObjects.Converters;
using System;

namespace EQBattle.ViewModels
{
    class MainWindowViewModel : ViewModelBase
    {
        private ViewModelBase _headerVM;
        private ViewModelBase _listVM;
        private ViewModelBase _detailVM;
        private ViewModelBase _footerVM;

        private Battle _battle;

        public MainWindowViewModel()
        {
            // Register view models here
            // Ugh .. DI this

            HeaderVM = new BattleHeaderViewModel();
            FooterVM = new BattleFooterViewModel();
            ListVM = new FightListViewModel();
            DetailVM = new FightViewModel();

            Messenger.Instance.Subscribe("OpenFile", x => CreateNewBattle(x));
        }

        private void CreateNewBattle(object fileName)
        {
            //var _youAre = new YouResolver(WhoseLogFile(fileName));
            var _youAre = new YouResolver("Khadaji");
            _battle = new Battle(_youAre);

            Messenger.Instance.Publish("NewBattle", _battle);
        }

        public ViewModelBase HeaderVM
        {
            get => _headerVM;
            set => SetProperty(ref _headerVM, value);
        }

        public ViewModelBase ListVM
        {
            get => _listVM;
            set => SetProperty(ref _listVM, value);
        }

        public ViewModelBase DetailVM
        {
            get => _detailVM;
            set => SetProperty(ref _detailVM, value);
        }

        public ViewModelBase FooterVM
        {
            get => _footerVM;
            set => SetProperty(ref _footerVM, value);
        }
    }
}
