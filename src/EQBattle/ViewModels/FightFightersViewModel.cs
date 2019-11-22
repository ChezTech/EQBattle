using BizObjects.Battle;
using Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Windows.Data;
using System.Collections.ObjectModel;

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

            FighterList = new ObservableCollection<FighterListItem>(ConvertFightersIntoListItems(fight.Fighters));
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

            var fli = NewFLIFromFighter(fighter);

            RunOnUIThread(() =>
            {
                fighterList.Add(fli);
            });
        }

        private IEnumerable<FighterListItem> ConvertFightersIntoListItems(IEnumerable<Fighter> fighters)
        {
            var fighterItemList = fighters.Select(x => NewFLIFromFighter(x));
            return fighterItemList;
        }

        private FighterListItem NewFLIFromFighter(Fighter fighter)
        {
            var fighterItem = new FighterListItem()
            {
                Name = fighter.Character.Name,
                Class = "aClass", // fighter.Character.Class.Name,
                Offense = new FighterStats()
                {
                    Duration = fighter.OffensiveStatistics.Duration.FighterDuration,
                    DPS = fighter.OffensiveStatistics.PerTime.FighterDPS,
                    DPS6 = fighter.OffensiveStatistics.PerTime.FighterDPSLastSixSeconds,
                    HitTotal = fighter.OffensiveStatistics.Hit.Total,
                    HitCount = fighter.OffensiveStatistics.Hit.Count,
                    Max = fighter.OffensiveStatistics.Hit.Max,
                    MissCount = fighter.OffensiveStatistics.Miss.Count,
                    HealTotal = fighter.OffensiveStatistics.Heal.Total,
                    HealCount = fighter.OffensiveStatistics.Heal.Count,
                },
                Defense = new FighterStats()
                {
                    Duration = fighter.DefensiveStatistics.Duration.FighterDuration,
                    DPS = fighter.DefensiveStatistics.PerTime.FighterDPS,
                    DPS6 = fighter.DefensiveStatistics.PerTime.FighterDPSLastSixSeconds,
                    HitTotal = fighter.DefensiveStatistics.Hit.Total,
                    HitCount = fighter.DefensiveStatistics.Hit.Count,
                    Max = fighter.DefensiveStatistics.Hit.Max,
                    MissCount = fighter.DefensiveStatistics.Miss.Count,
                    HealTotal = fighter.DefensiveStatistics.Heal.Total,
                    HealCount = fighter.DefensiveStatistics.Heal.Count,
                },
            };

            //fighter.PropertyChanged +=

            return fighterItem;
        }

    }

    public class FighterListItem : PropertyChangeBase
    {
        private string name;
        public string Name { get => name; set => SetProperty(ref name, value); }

        private string charClass;
        public string Class { get => charClass; set => SetProperty(ref charClass, value); }

        private FighterStats offense;
        public FighterStats Offense { get => offense; set => SetProperty(ref offense, value); }

        private FighterStats defense;
        public FighterStats Defense { get => defense; set => SetProperty(ref defense, value); }
    }

    public class FighterStats : PropertyChangeBase
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
    }
}
