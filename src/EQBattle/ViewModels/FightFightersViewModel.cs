using Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace EQBattle.ViewModels
{
    public class FightFightersViewModel : PropertyChangeBase
    {
        private Fight fight;

        public FightFightersViewModel()
        {
            Messenger.Instance.Subscribe("OnSelectedFightChanged", x => Fight = x as Fight);
        }

        public Fight Fight { get => fight; set => SetProperty(ref fight, value); }
    }

    class FighterListItem : PropertyChangeBase
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

    class FighterStats
    {
        private string duration;
        public string Duration { get => duration; set => SetProperty(ref duration, value); }

        private string dps;
        public string DPS { get => dps; set => SetProperty(ref dps, value); }

        private string dps6;
        public string DPS6 { get => dps6; set => SetProperty(ref dps6, value); }

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
