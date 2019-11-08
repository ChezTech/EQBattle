using BizObjects.Battle;
using Core;
using System.Collections.ObjectModel;

namespace EQBattle.ViewModels
{
    public class FightViewModel : PropertyChangeBase
    {
        private ObservableCollection<FightTabItem> fightTabs = new ObservableCollection<FightTabItem>();
        private FightTabItem selectedTab;

        private Skirmish skirmish;

        public class FightTabItem
        {
            public string Header { get; set; }
            public PropertyChangeBase Content { get; set; }
        }

        public FightViewModel()
        {
            Messenger.Instance.Subscribe("OnSelectedSkirmishChanged", x => Skirmish = x as Skirmish);

            fightTabs.Add(new FightTabItem() { Header = "Overview", Content = new FightOverviewViewModel() });
            fightTabs.Add(new FightTabItem() { Header = "Log Lines", Content = new FightLogLinesViewModel() });

            SelectedTab = fightTabs[0];
        }

        public Skirmish Skirmish { get => skirmish; set => SetProperty(ref skirmish, value); }

        public ObservableCollection<FightTabItem> FightTabs { get => fightTabs; set => SetProperty(ref fightTabs, value); }

        public FightTabItem SelectedTab { get => selectedTab; set => SetProperty(ref selectedTab, value); }
    }
}
