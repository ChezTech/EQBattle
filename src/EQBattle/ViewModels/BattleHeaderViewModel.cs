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
        }

        private void RunFileRead()
        {
        }

        private void PauseFileRead()
        {
        }
    }
}
