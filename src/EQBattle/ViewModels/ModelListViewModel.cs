using Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace EQBattle.ViewModels
{
    public abstract class ModelListViewModel<T, K> : PropertyChangeBase
        where T : class
        where K : ModelListItem<T>
    {
        private readonly object _lock = new object();

        private ObservableCollection<K> modelListItems;
        public ObservableCollection<K> ModelListItems { get => modelListItems; set => SetProperty(ref modelListItems, value); }

        protected void NewModelListItems(IEnumerable<K> sourceModelListItems)
        {
            ModelListItems = new ObservableCollection<K>(sourceModelListItems);
            BindingOperations.EnableCollectionSynchronization(ModelListItems, _lock);
        }
    }
}
