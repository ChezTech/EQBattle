using BizObjects.Battle;
using Core;

namespace EQBattle.ViewModels
{
    class FightOverviewViewModel : PropertyChangeBase
    {
        private Fight fight;

        public FightOverviewViewModel()
        {
            Messenger.Instance.Subscribe("OnSelectedFightChanged", x => Fight = x as Fight);
        }

        public Fight Fight { get => fight; set => SetProperty(ref fight, value); }
    }
}
