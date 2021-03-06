﻿using System;
using System.Collections.Generic;

namespace BizObjects.Converters
{
    public class AttackTypeConverter
    {
        private readonly IDictionary<string, AttackType> _nameToTypeMap = new Dictionary<string, AttackType>()
        {
            {"backstab", AttackType.Backstab },
            {"backstabs", AttackType.Backstab },
            {"bash", AttackType.Bash },
            {"bashes", AttackType.Bash },
            {"bite", AttackType.Bite },
            {"bites", AttackType.Bite },
            {"burn", AttackType.Burn },
            {"burns", AttackType.Burn },
            {"burned", AttackType.Burn },
            {"claw", AttackType.Claw },
            {"claws", AttackType.Claw },
            {"crush", AttackType.Crush },
            {"crushes", AttackType.Crush },
            {"died", AttackType.Kill },
            {"frenzies on", AttackType.Frenzy },
            {"frenzy", AttackType.Frenzy },
            {"gore", AttackType.Gore },
            {"gores", AttackType.Gore },
            {"hit", AttackType.Hit },
            {"hits", AttackType.Hit },
            {"kick", AttackType.Kick },
            {"kicks", AttackType.Kick },
            {"kill", AttackType.Kill },
            {"maul", AttackType.Maul },
            {"mauls", AttackType.Maul },
            {"pierce", AttackType.Pierce },
            {"pierced", AttackType.Pierce },
            {"pierces", AttackType.Pierce },
            {"punch", AttackType.Punch },
            {"punches", AttackType.Punch },
            {"slain", AttackType.Kill },
            {"slash", AttackType.Slash },
            {"slashes", AttackType.Slash },
            {"slice", AttackType.Slice },
            {"slices", AttackType.Slice },
            {"smash", AttackType.Smash },
            {"smashes", AttackType.Smash },
            {"sting", AttackType.Sting },
            {"stings", AttackType.Sting },
            {"strike", AttackType.Strike },
            {"strikes", AttackType.Strike },
            {"torment", AttackType.Torment },
            {"tormented", AttackType.Torment },
            {"torments", AttackType.Torment },
            //{"xxxx", AttackType.Unknown },
        };

        public AttackType Convert(string hitTypeName)
        {
            if (hitTypeName == null) return AttackType.Unknown;

            return _nameToTypeMap.TryGetValue(hitTypeName, out AttackType hitType)
                ? hitType
                : Enum.TryParse(hitTypeName, out hitType)
                    ? hitType
                    : AttackType.Unknown;
        }

        public ICollection<string> Names { get { return _nameToTypeMap.Keys; } }
    }
}
