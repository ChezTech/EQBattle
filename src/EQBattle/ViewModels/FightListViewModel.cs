﻿using BizObjects.Battle;
using Core;
using EQJobService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;

namespace EQBattle.ViewModels
{
    class FightListViewModel : ModelListViewModel<IFight, FightListItem>
    {
        private EQJob latestEQJob;
        private Battle latestBattle;
        private FightListItem selectedFight;
        private ISkirmish latestSkirmish;
        private IFight latestFight;
        private FightListItem latestFightListItem;

        private bool isFileLoading = false;
        private bool newBattleSelectItemYet = false;

        public FightListViewModel()
        {
            Messenger.Instance.Subscribe("NewEQJob", x => NewEQJob(x as EQJob));
            Messenger.Instance.Subscribe("NewBattle", x => NewBattle(x as Battle));
            Messenger.Instance.Subscribe("RefreshBattle", x => NewBattle(latestBattle));
        }

        private void NewEQJob(EQJob eqJob)
        {
            ClearEQJobEvents(latestEQJob);

            latestEQJob = eqJob;

            latestEQJob.StartReading += LatestEQJob_StartReading;
            latestEQJob.EoFBattle += LatestEQJob_EoFBattle;
        }

        private void LatestEQJob_StartReading()
        {
            isFileLoading = true;
        }

        private void LatestEQJob_EoFBattle()
        {
            isFileLoading = false;

            // We've finished reading the EQ Log file (for now), give an update to the GUI
            latestFightListItem?.Refresh();

            TrackFight();
        }

        private void ClearEQJobEvents(EQJob eqJob)
        {
            if (eqJob == null)
                return;

            latestEQJob.StartReading -= LatestEQJob_StartReading;
            latestEQJob.EoFBattle -= LatestEQJob_EoFBattle;
        }

        private void NewBattle(Battle battle)
        {
            ClearBattleEvents(latestBattle);
            latestSkirmish = null;
            latestFight = null;
            latestFightListItem = null;
            newBattleSelectItemYet = false;

            latestBattle = battle;
            NewModelListItems(ConvertFightsIntoListItems(battle.Skirmishes));

            battle.Skirmishes.CollectionChanged += Skirmishes_CollectionChanged;
            AddNewSkirmish(null, battle.Skirmishes.LastOrDefault());
        }

        private void Skirmishes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (var item in e.NewItems)
                AddNewSkirmish(ModelListItems, item as ISkirmish);
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
                AddNewFight(ModelListItems, item as IFight);
        }

        private void AddNewFight(ObservableCollection<FightListItem> fightList, IFight fight)
        {
            // Before we move on to the next fight, give one last update to the current fight so the GUI can update itself
            latestFightListItem?.Refresh();

            latestFightListItem = new FightListItem(fight);
            fightList.Add(latestFightListItem);

            TrackFight();

            latestFight = fight;
        }

        private void TrackFight()
        {
            // We want to select the first item if we're starting to load a file
            if (SelectedFight == null)
                SelectedFight = latestFightListItem;

            // If we're still loading the file, don't track it (it slows down the load by updating all the UI with each selection)
            if (isFileLoading)
                return;

            // If we're on the most recent fight (or we haven't selected an item yet), move our "cursor" to this new fight to track the latest
            else if (latestFight == SelectedFight.Model || !newBattleSelectItemYet)
                SelectedFight = latestFightListItem;

            newBattleSelectItemYet = true;
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

        public FightListItem SelectedFight
        {
            get => selectedFight;
            set
            {
                if (SetProperty(ref selectedFight, value))
                    Messenger.Instance.Publish("OnSelectedFightChanged", SelectedFight?.Model);
            }
        }
    }

    class FightListItem : ModelListItem<IFight>
    {
        private string name;
        private int mobDefensiveDamage;
        private double mobOffensiveDps;
        private TimeSpan duration;
        private string zone;

        public string Name { get => name; set => SetProperty(ref name, value); }
        public int MobDefensiveDamage { get => mobDefensiveDamage; set => SetProperty(ref mobDefensiveDamage, value); }
        public double MobOffensiveDps { get => mobOffensiveDps; set => SetProperty(ref mobOffensiveDps, value); }
        public TimeSpan Duration { get => duration; set => SetProperty(ref duration, value); }
        public string Zone { get => zone; set => SetProperty(ref zone, value); }

        public FightListItem(IFight model) : base(model)
        {
            Refresh();
        }

        public override void Refresh()
        {
            using (new Battle.Freeze())
            {
                Name = Model.PrimaryMob.Name;
                Duration = Model.Statistics.Duration.FightDuration;
                MobDefensiveDamage = Model.PrimaryMobFighter.DefensiveStatistics.Hit.Total;
                MobOffensiveDps = Model.PrimaryMobFighter.OffensiveStatistics.PerTime.FightDPS;
                Zone = "TBD";
            }

        }
    }
}
