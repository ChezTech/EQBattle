using Core;
using System.Collections.ObjectModel;

namespace EQBattle.ViewModels
{
    public class FightViewModel : PropertyChangeBase
    {
        private ObservableCollection<FightTabItem> fightTabs = new ObservableCollection<FightTabItem>();
        private FightTabItem selectedTab;

        public class FightTabItem
        {
            public string Header { get; set; }
            public PropertyChangeBase Content { get; set; }
        }

        public FightViewModel()
        {
            fightTabs.Add(new FightTabItem() { Header = "Overview", Content = new FightOverviewViewModel() });
            fightTabs.Add(new FightTabItem() { Header = "Log Lines", Content = new FightLogLinesViewModel() });
            fightTabs.Add(new FightTabItem() { Header = "Fighters", Content = new FightFightersViewModel() });

            SelectedTab = fightTabs[0];
        }

        public ObservableCollection<FightTabItem> FightTabs { get => fightTabs; set => SetProperty(ref fightTabs, value); }

        public FightTabItem SelectedTab { get => selectedTab; set => SetProperty(ref selectedTab, value); }
    }
}
