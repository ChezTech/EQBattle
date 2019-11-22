using Core;

namespace EQBattle.ViewModels
{
    public interface IModelListItem<T>
    {
        public T Model { get; }
        void Refresh();
    }

    public abstract class ModelListItem<T> : PropertyChangeBase, IModelListItem<T>
    {
        public ModelListItem(T model)
        {
            Model = model;
        }

        public T Model { get; private set; }

        public abstract void Refresh();
    }
}
