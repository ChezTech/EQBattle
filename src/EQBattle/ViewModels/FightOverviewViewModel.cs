using BizObjects.Battle;
using Core;

namespace EQBattle.ViewModels
{
    class FightOverviewViewModel : PropertyChangeBase
    {
        private Battle battle;
        private Skirmish skirmish;

        public FightOverviewViewModel()
        {
            Messenger.Instance.Subscribe("NewBattle", x => NewBattle(x));
            Messenger.Instance.Subscribe("OnSelectedSkirmishChanged", x => Skirmish = x as Skirmish);
        }

        private void NewBattle(object x)
        {
            Battle = x as Battle;
        }

        public Battle Battle { get => battle; set => SetProperty(ref battle, value); }
        public Skirmish Skirmish { get => skirmish; set => SetProperty(ref skirmish, value); }
    }
}
