using BizObjects.Battle;
using System;
using System.Linq;
using System.Windows.Data;
using System.Collections.ObjectModel;
using BizObjects.Statistics;

namespace EQBattle.ViewModels
{
    public class FightFightersViewModel : ViewModelBase
    {
        private ObservableCollection<FighterListItem> fighterList;
        public ObservableCollection<FighterListItem> FighterList { get => fighterList; set => SetProperty(ref fighterList, value); }
        private readonly object _lock = new object();

        private Fight fight;

        public FightFightersViewModel()
        {
            FighterList = new ObservableCollection<FighterListItem>();
            StartDispatchTimer(250, () =>
            {
                foreach (var fli in FighterList)
                    fli.Refresh();
            });

            Messenger.Instance.Subscribe("OnSelectedFightChanged", x => NewFight(x as Fight));
        }

        private void NewFight(Fight newFight)
        {
            if (fight != null)
                fight.Fighters.CollectionChanged -= Fighters_CollectionChanged;

            fight = newFight;

            if (fight == null)
            {
                FighterList = new ObservableCollection<FighterListItem>();
                return;
            }

            FighterList = new ObservableCollection<FighterListItem>(fight.Fighters.Select(x => new FighterListItem(x)));
            BindingOperations.EnableCollectionSynchronization(FighterList, _lock);

            fight.Fighters.CollectionChanged += Fighters_CollectionChanged;
        }

        private void Fighters_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (var item in e.NewItems)
                AddNewFighter(FighterList, item as Fighter);
        }

        private void AddNewFighter(ObservableCollection<FighterListItem> fighterList, Fighter fighter)
        {
            // Not sure why the EnableCollectionSynchronization() doesn't help when this collection is created.
            // I thought that perhaps the collection wasn't created on the UI thread and wrapped it in the RunOnUIThread() call, but that didn't help either.
            // So, not ideal, but this does the trick.

            var fli = new FighterListItem(fighter);

            RunOnUIThread(() =>
            {
                fighterList.Add(fli);
            });
        }
    }

    public class FighterListItem : ModelListItem<Fighter>
    {
        private string name;
        public string Name { get => name; set => SetProperty(ref name, value); }

        private string charClass;
        public string Class { get => charClass; set => SetProperty(ref charClass, value); }

        private FighterStats offense;
        public FighterStats Offense { get => offense; set => SetProperty(ref offense, value); }

        private FighterStats defense;
        public FighterStats Defense { get => defense; set => SetProperty(ref defense, value); }

        public FighterListItem(Fighter fighter) : base(fighter)
        {
            Offense = new FighterStats(Model.OffensiveStatistics);
            Defense = new FighterStats(Model.DefensiveStatistics);
            Refresh();
        }

        public override void Refresh()
        {
            Name = Model.Character.Name;
            Class = "aClass"; // Model.Character.Class.Name;
            Offense.Refresh();
            Defense.Refresh();
        }
    }

    public class FighterStats : ModelListItem<FightStatistics>
    {
        private TimeSpan duration;
        public TimeSpan Duration { get => duration; set => SetProperty(ref duration, value); }

        private double dps;
        public double DPS { get => dps; set => SetProperty(ref dps, value); }

        private double dps6;
        public double DPS6 { get => dps6; set => SetProperty(ref dps6, value); }

        private int hitTotal;
        public int HitTotal { get => hitTotal; set => SetProperty(ref hitTotal, value); }

        private int hitCount;
        public int HitCount { get => hitCount; set => SetProperty(ref hitCount, value); }

        private int max;
        public int Max { get => max; set => SetProperty(ref max, value); }

        private int missCount;
        public int MissCount { get => missCount; set => SetProperty(ref missCount, value); }

        private int healTotal;
        public int HealTotal { get => healTotal; set => SetProperty(ref healTotal, value); }

        private int healCount;
        public int HealCount { get => healCount; set => SetProperty(ref healCount, value); }

        public FighterStats(FightStatistics stats) : base(stats)
        {
            Refresh();
        }

        public override void Refresh()
        {
            Duration = Model.Duration.FighterDuration;
            DPS = Model.PerTime.FighterDPS;
            DPS6 = Model.PerTime.FighterDPSLastSixSeconds;
            HitTotal = Model.Hit.Total;
            HitCount = Model.Hit.Count;
            Max = Model.Hit.Max;
            MissCount = Model.Miss.Count;
            HealTotal = Model.Heal.Total;
            HealCount = Model.Heal.Count;
        }

    }
}
