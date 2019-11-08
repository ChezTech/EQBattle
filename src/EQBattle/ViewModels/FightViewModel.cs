using BizObjects.Battle;
using Core;

namespace EQBattle.ViewModels
{
    public class FightViewModel : PropertyChangeBase
    {
        private PropertyChangeBase currentFightVM;

        private Skirmish skirmish;

        public FightViewModel()
        {
            Messenger.Instance.Subscribe("OnSelectedSkirmishChanged", x => Skirmish = x as Skirmish);

            CurrentFightVM = new FightOverviewViewModel();
        }

        public Skirmish Skirmish { get => skirmish; set => SetProperty(ref skirmish, value); }

        public PropertyChangeBase CurrentFightVM { get => currentFightVM; set => SetProperty(ref currentFightVM, value); }
    }
}
