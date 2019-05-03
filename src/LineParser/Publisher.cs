﻿using BizObjects;
using System;

namespace LineParser
{
    public class Publisher
    {
        public event Action<Line> LineCreated;
        public event Action<Unknown> UnknownCreated;
        public event Action<Attack> AttackCreated;
        public event Action<Kill> KillCreated;

        public void RaiseCreated(Line o)
        {
            LineCreated?.Invoke(o);
        }

        public void RaiseCreated(Unknown o)
        {
            UnknownCreated?.Invoke(o);
        }

        public void RaiseCreated(Attack o)
        {
            AttackCreated?.Invoke(o);
        }

        public void RaiseCreated(Kill o)
        {
            KillCreated?.Invoke(o);
        }
    }
}
