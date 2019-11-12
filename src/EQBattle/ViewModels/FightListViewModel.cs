using BizObjects.Battle;
using Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;

namespace EQBattle.ViewModels
{
    class FightListViewModel : PropertyChangeBase
    {
        private ObservableCollection<FightListItem> fightList;
        private readonly object _lock = new object();

        private Battle battle;
        private Skirmish selectedSkirmish;
        private ISkirmish latestSkirmish;

        public FightListViewModel()
        {
            Messenger.Instance.Subscribe("NewBattle", x => NewBattle(x as Battle));
            Messenger.Instance.Subscribe("RefreshBattle", x => NewBattle(battle));
        }

        private void NewBattle(Battle battle)
        {
            ClearBattleEvents(this.battle);

            this.battle = battle;
            FightList = new ObservableCollection<FightListItem>(ConvertFightsIntoListItems(battle.Skirmishes));
            BindingOperations.EnableCollectionSynchronization(FightList, _lock);

            battle.Skirmishes.CollectionChanged += Skirmishes_CollectionChanged;
            AddNewSkirmish(null, battle.Skirmishes.LastOrDefault());
        }

        private void Skirmishes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (var item in e.NewItems)
                AddNewSkirmish(FightList, item as ISkirmish);
        }

        private void AddNewSkirmish(ObservableCollection<FightListItem> fightList, ISkirmish skirmish)
        {
            ClearSkirmishEvents(latestSkirmish);

            if (skirmish == null)
                return;

            latestSkirmish = skirmish;
            latestSkirmish.Fights.CollectionChanged += Fights_CollectionChanged;
        }

        private void Fights_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (var item in e.NewItems)
                AddNewFight(FightList, item as IFight);
        }

        private void AddNewFight(ObservableCollection<FightListItem> fightList, IFight fight)
        {
            fightList.Add(NewFLIFromFight(fight));
        }

        private IEnumerable<FightListItem> ConvertFightsIntoListItems(IEnumerable<ISkirmish> skirmishes)
        {
            IEnumerable<FightListItem> newFightList = skirmishes.SelectMany(x => x.Fights).Select(x => NewFLIFromFight(x));

            //ObservableCollection<FightListItem> newFightList = new ObservableCollection<FightListItem>();
            //foreach (var skirmish in skirmishes)
            //{
            //    AddNewSkirmish(newFightList, skirmish);
            //    foreach (var fight in skirmish.Fights)
            //        AddNewFight(newFightList, fight);
            //}
            return newFightList;
        }

        private static FightListItem NewFLIFromFight(IFight fight)
        {
            return new FightListItem()
            {
                Name = fight.PrimaryMob.Name,
                Duration = fight.Statistics.Duration.FightDuration,
                MobDefensiveDamage = fight.PrimaryMobFighter.DefensiveStatistics.Hit.Total,
                MobOffensiveDps = fight.PrimaryMobFighter.OffensiveStatistics.PerTime.FightDPS,
                Zone = "TBD",
                Fight = fight
            };
        }

        private void ClearBattleEvents(Battle battle)
        {
            if (battle != null)
                battle.Skirmishes.CollectionChanged -= Skirmishes_CollectionChanged;
            ClearSkirmishEvents(latestSkirmish);
        }

        private void ClearSkirmishEvents(ISkirmish skirmish)
        {
            if (skirmish != null)
                skirmish.Fights.CollectionChanged -= Fights_CollectionChanged;
        }

        public Battle Battle { get => battle; set => SetProperty(ref battle, value); }
        public ObservableCollection<FightListItem> FightList { get => fightList; set => SetProperty(ref fightList, value); }

        public Skirmish SelectedSkirmish
        {
            get => selectedSkirmish;
            set
            {
                if (SetProperty(ref selectedSkirmish, value))
                    Messenger.Instance.Publish("OnSelectedSkirmishChanged", SelectedSkirmish);
            }
        }
    }

    class FightListItem : PropertyChangeBase
    {
        private string name;
        private int mobDefensiveDamage;
        private double mobOffensiveDps;
        private TimeSpan duration;
        private string zone;
        private IFight fight;

        public string Name { get => name; set => SetProperty(ref name, value); }
        public int MobDefensiveDamage { get => mobDefensiveDamage; set => SetProperty(ref mobDefensiveDamage, value); }
        public double MobOffensiveDps { get => mobOffensiveDps; set => SetProperty(ref mobOffensiveDps, value); }
        public TimeSpan Duration { get => duration; set => SetProperty(ref duration, value); }
        public string Zone { get => zone; set => SetProperty(ref zone, value); }
        public IFight Fight { get => fight; set => SetProperty(ref fight, value); }
    }
}
