﻿using BizObjects.Battle;
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
        private FightListItem selectedFight;
        private ISkirmish latestSkirmish;
        private IFight latestFight;
        private FightListItem latestFightListItem;

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
            ClearFightEvents(latestFight);

            latestFight = fight;
            latestFightListItem = NewFLIFromFight(fight);
            fightList.Add(latestFightListItem);
            latestFight.PropertyChanged += Fight_PropertyChanged;
        }

        private IEnumerable<FightListItem> ConvertFightsIntoListItems(IEnumerable<ISkirmish> skirmishes)
        {
            //IEnumerable<FightListItem> newFightList = skirmishes.SelectMany(x => x.Fights).Select(x => NewFLIFromFight(x));

            ObservableCollection<FightListItem> newFightList = new ObservableCollection<FightListItem>();
            foreach (var skirmish in skirmishes)
            {
                foreach (var fight in skirmish.Fights)
                    AddNewFight(newFightList, fight);
            }
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

        private void Fight_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Fight.PrimaryMob))
                latestFightListItem.Name = latestFight.PrimaryMob.Name;
            if (e.PropertyName == nameof(Fight.Statistics.Duration.FightDuration))
                latestFightListItem.Duration = latestFight.Statistics.Duration.FightDuration;
            if (e.PropertyName == nameof(Fight.PrimaryMobFighter.DefensiveStatistics.Hit.Total))
                latestFightListItem.MobDefensiveDamage = latestFight.PrimaryMobFighter.DefensiveStatistics.Hit.Total;
            if (e.PropertyName == nameof(Fight.PrimaryMobFighter.OffensiveStatistics.PerTime.FightDPS))
                latestFightListItem.MobOffensiveDps = latestFight.PrimaryMobFighter.OffensiveStatistics.PerTime.FightDPS;
            //if (e.PropertyName == nameof(Fight.Zone))
            //    latestFightListItem.Zone = latestFight.PrimaryMob.Name;
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

        private void ClearFightEvents(IFight latestFight)
        {
            if (latestFight != null)
                latestFight.PropertyChanged -= Fight_PropertyChanged;
        }

        public ObservableCollection<FightListItem> FightList { get => fightList; set => SetProperty(ref fightList, value); }

        public FightListItem SelectedFight
        {
            get => selectedFight;
            set
            {
                if (SetProperty(ref selectedFight, value))
                    Messenger.Instance.Publish("OnSelectedFightChanged", SelectedFight?.Fight);
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
