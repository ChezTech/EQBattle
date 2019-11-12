using BizObjects.Battle;
using Core;

namespace EQBattle.ViewModels
{
    class FightOverviewViewModel : PropertyChangeBase
    {
        private Battle battle;
        private Fight fight;

        public FightOverviewViewModel()
        {
            Messenger.Instance.Subscribe("NewBattle", x => NewBattle(x));
            Messenger.Instance.Subscribe("OnSelectedFightChanged", x => Fight = x as Fight);
        }

        private void NewBattle(object x)
        {
            Battle = x as Battle;
        }

        public Battle Battle { get => battle; set => SetProperty(ref battle, value); }
        public Fight Fight { get => fight; set => SetProperty(ref fight, value); }
    }
}
