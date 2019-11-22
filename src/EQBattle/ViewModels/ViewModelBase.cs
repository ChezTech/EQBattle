using Core;
using System;
using System.Threading;
using System.Windows;

namespace EQBattle.ViewModels
{
    public class ViewModelBase : PropertyChangeBase
    {
        protected void RunOnUIThread(Action action)
        {
            // If we're on the UI already, just run the thing
            if (Application.Current.Dispatcher.Thread == Thread.CurrentThread)
                action();
            else
                Application.Current.Dispatcher.BeginInvoke(action);
        }
    }
}
