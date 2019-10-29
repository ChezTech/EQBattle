using System;
using System.Collections.Generic;

namespace EQBattle
{
    public sealed class Messenger
    {
        private Dictionary<string, HashSet<Action<object>>> events = new Dictionary<string, HashSet<Action<object>>>();

        public void Subscribe(string eventName, Action<object> callback)
        {
            if (!events.ContainsKey(eventName))
                events.Add(eventName, new HashSet<Action<object>>());

            events[eventName].Add(callback);
        }

        public void Unsubscribe(string eventName, Action<object> callback)
        {
            if (events.ContainsKey(eventName))
                events[eventName].Remove(callback);
        }

        public void Publish(string eventName, object arg = default)
        {
            if (events.ContainsKey(eventName))
                foreach (var callback in events[eventName])
                    callback(arg);
        }

        #region Singleton

        // Jon Skeet: https://csharpindepth.com/articles/singleton

        // Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
        static Messenger() { }

        private Messenger() { }

        public static Messenger Instance { get; } = new Messenger();

        #endregion
    }
}
