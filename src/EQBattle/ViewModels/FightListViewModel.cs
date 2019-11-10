using BizObjects.Battle;
using Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace EQBattle.ViewModels
{
    class FightListViewModel : PropertyChangeBase
    {
        private ObservableCollection<FightListItem> fightList;

        private Battle battle;
        private Skirmish selectedSkirmish;

        public FightListViewModel()
        {
            Messenger.Instance.Subscribe("NewBattle", x => NewBattle(x as Battle));
        }

        private void NewBattle(Battle battle)
        {
            FightList = new ObservableCollection<FightListItem>(ConvertFightsIntoListItems(battle.Skirmishes));
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
