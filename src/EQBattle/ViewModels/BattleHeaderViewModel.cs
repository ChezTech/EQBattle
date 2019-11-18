using Core;
using System;
using System.Windows.Input;
using WpfUIServices;

namespace EQBattle.ViewModels
{
    class BattleHeaderViewModel : PropertyChangeBase
    {
        private ICommand open;
        private ICommand run;
        private ICommand pause;
        private ICommand refresh;

        public ICommand Open { get => open ?? (open = new RelayCommand(x => OpenFile())); }
        public ICommand Run { get => run ?? (run = new RelayCommand(x => RunFileRead())); }
        public ICommand Pause { get => pause ?? (pause = new RelayCommand(x => PauseFileRead())); }
        public ICommand Refresh { get => refresh ?? (refresh = new RelayCommand(x => RefreshBattle())); }

        private void OpenFile()
        {
            var initDir = @"C:\Users\Public\Daybreak Game Company\Installed Games\EverQuest\Logs";
            var title = "Open Everquest log file";
            var fileName = UIService.Instance.GetFileNameFromOpenFileDialog(initDir, title: title);

            if (fileName != null)
                Messenger.Instance.Publish("OpenFile", fileName);
        }

        private void RunFileRead()
        {
        }

        private void PauseFileRead()
        {
        }

        private void RefreshBattle()
        {
            Messenger.Instance.Publish("RefreshBattle");
        }
    }
}
