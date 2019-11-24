using Core;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace EQBattle.ViewModels
{
    public class ViewModelBase : PropertyChangeBase
    {
        private DispatcherTimer refreshTimer;

        protected void RunOnUIThread(Action action)
        {
            // If we're on the UI already, just run the thing
            if (Application.Current.Dispatcher.Thread == Thread.CurrentThread)
                action();
            else
                Application.Current.Dispatcher.BeginInvoke(action);
        }

        protected void StartDispatchTimer(int msInterval, Action refreshAction)
        {
            StartDispatchTimer(new TimeSpan(0, 0, 0, 0, msInterval), refreshAction);
        }

        protected void StartDispatchTimer(TimeSpan interval, Action refreshAction)
        {
            if (refreshTimer != null)
                refreshTimer.Stop();

            refreshTimer = new DispatcherTimer();
            refreshTimer.Interval = interval;
            refreshTimer.Tick += (s, e) => refreshAction();
            refreshTimer.Start();
        }
    }
}
