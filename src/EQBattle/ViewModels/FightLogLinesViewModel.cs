using BizObjects.Battle;
using BizObjects.Lines;
using LogObjects;
using System.Linq;

namespace EQBattle.ViewModels
{
    class FightLogLinesViewModel : ModelListViewModel<ILine, FightLogLinesListItems>
    {
        private Fight fight;
        public Fight Fight { get => fight; set => SetProperty(ref fight, value); }

        public FightLogLinesViewModel()
        {
            Messenger.Instance.Subscribe("OnSelectedFightChanged", x =>
            {
                ClearFightEvents();

                Fight = x as Fight;
                if (Fight == null) // This can happen when loading a new file and the Battle resets to empty
                    return;

                NewModelListItems(Fight.Lines.Select(x => new FightLogLinesListItems(x)));

                Fight.Lines.CollectionChanged += Lines_CollectionChanged;
            });
        }

        private void ClearFightEvents()
        {
            if (Fight != null)
                Fight.Lines.CollectionChanged -= Lines_CollectionChanged;
        }

        private void Lines_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // I'm not sure why I have to run this on UI thread. I do EnableCollectionSynchronization in NewModelListItems() which should take care of that (I thought)
            foreach (var item in e.NewItems)
                RunOnUIThread(() => ModelListItems.Add(new FightLogLinesListItems(item as ILine)));
        }
    }

    class FightLogLinesListItems : ModelListItem<ILine>
    {
        private LogDatum logLine;
        public LogDatum LogLine { get => logLine; set => SetProperty(ref logLine, value); }

        public FightLogLinesListItems(ILine model) : base(model)
        {
            Refresh();
        }

        public override void Refresh()
        {
            using (new Battle.Freeze())
            {
                LogLine = Model.LogLine;
            }
        }
    }
}
