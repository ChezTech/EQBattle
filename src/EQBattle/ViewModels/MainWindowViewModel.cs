using Core;
using EQJobService;
using System.Threading.Tasks;
using System.Windows.Data;

namespace EQBattle.ViewModels
{
    class MainWindowViewModel : PropertyChangeBase
    {
        private PropertyChangeBase _headerVM;
        private PropertyChangeBase _listVM;
        private PropertyChangeBase _detailVM;
        private PropertyChangeBase _footerVM;

        private EQJob _eqJob;
        private readonly object _lock = new object();

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
            _eqJob = new EQJob(fileName as string);
            BindingOperations.EnableCollectionSynchronization(_eqJob.Battle.Skirmishes, _lock);

            Messenger.Instance.Publish("NewEQJob", _eqJob);
            Messenger.Instance.Publish("NewBattle", _eqJob.Battle);

            // NOTE: probably more robustness needed here to really make this async and handle errors etc...
            Task.Run(() => _eqJob.ReadFileIntoBattleAsync());
        }

        public PropertyChangeBase HeaderVM
        {
            get => _headerVM;
            set => SetProperty(ref _headerVM, value);
        }

        public PropertyChangeBase ListVM
        {
            get => _listVM;
            set => SetProperty(ref _listVM, value);
        }

        public PropertyChangeBase DetailVM
        {
            get => _detailVM;
            set => SetProperty(ref _detailVM, value);
        }

        public PropertyChangeBase FooterVM
        {
            get => _footerVM;
            set => SetProperty(ref _footerVM, value);
        }
    }
}
