using Microsoft.Win32;
using System.Windows.Input;

namespace EQBattle.ViewModels
{
    class BattleHeaderViewModel : ViewModelBase
    {
        private ICommand open;
        private ICommand run;
        private ICommand pause;

        public ICommand Open { get => open ?? (open = new RelayCommand(x => OpenFile())); }
        public ICommand Run { get => run ?? (run = new RelayCommand(x => RunFileRead())); }
        public ICommand Pause { get => pause ?? (pause = new RelayCommand(x => PauseFileRead())); }

        private void OpenFile()
        {
            // TODO: make this a DI service for testability)
            var ofd = new OpenFileDialog();
            ofd.InitialDirectory = @"C:\Users\Public\Daybreak Game Company\Installed Games\EverQuest\Logs";
            if (!ofd.ShowDialog().Value)
                return;

            var fileName = ofd.FileName;

            // Raise Message ....
        }

        private void RunFileRead()
        {
        }

        private void PauseFileRead()
        {
        }
    }
}
