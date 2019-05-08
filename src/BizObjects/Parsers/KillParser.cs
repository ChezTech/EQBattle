﻿using LogObjects;

namespace BizObjects.Parsers
{
    public class KillParser : IParser
    {
        private const string OtherDeath = "has been slain by";
        private const string YourKill = "You have slain";
        private const string YourDeath = "You have been slain by";
        private const string SomeoneDied = "died";

        public bool TryParse(LogDatum logDatum, out ILine lineEntry)
        {
            if (TryParseOtherDeath(logDatum, out lineEntry))
                return true;
            if (TryParseYourKill(logDatum, out lineEntry))
                return true;
            if (TryParseYourDeath(logDatum, out lineEntry))
                return true;
            if (TryParseSomeoneDied(logDatum, out lineEntry))
                return true;
            return false;
        }

        private bool TryParseOtherDeath(LogDatum logDatum, out ILine lineEntry)
        {
            lineEntry = null;
            string attacker;
            string defender;

            int i = logDatum.LogMessage.IndexOf(OtherDeath);
            if (i >= 0)
            {
                defender = logDatum.LogMessage.Substring(0, i - 1).Trim(' ', '!', '.');
                attacker = logDatum.LogMessage.Substring(i + OtherDeath.Length + 1).Trim(' ', '!', '.');
                lineEntry = new Kill(logDatum, attacker, defender, "slain");
                return true;
            }

            return false;
        }

        private bool TryParseYourKill(LogDatum logDatum, out ILine lineEntry)
        {
            lineEntry = null;
            string attacker;
            string defender;

            int i = logDatum.LogMessage.IndexOf(YourKill);
            if (i >= 0)
            {
                defender = logDatum.LogMessage.Substring(i + YourKill.Length + 1).Trim(' ', '!', '.');
                attacker = Attack.You;
                lineEntry = new Kill(logDatum, attacker, defender, "slain");
                return true;
            }

            return false;
        }

        private bool TryParseYourDeath(LogDatum logDatum, out ILine lineEntry)
        {
            lineEntry = null;
            string attacker;
            string defender;

            int i = logDatum.LogMessage.IndexOf(YourDeath);
            if (i >= 0)
            {
                defender = Attack.You;
                attacker = logDatum.LogMessage.Substring(i + YourDeath.Length + 1).Trim(' ', '!', '.');
                lineEntry = new Kill(logDatum, attacker, defender, "slain");
                return true;
            }

            return false;
        }

        private bool TryParseSomeoneDied(LogDatum logDatum, out ILine lineEntry)
        {
            lineEntry = null;
            string attacker;
            string defender;

            int i = logDatum.LogMessage.IndexOf(SomeoneDied);
            if (i >= 0)
            {
                defender = logDatum.LogMessage.Substring(i + SomeoneDied.Length + 1).Trim(' ', '!', '.');
                attacker = Attack.Unknown;
                lineEntry = new Kill(logDatum, attacker, defender, "died");
                return true;
            }

            return false;
        }
    }
}
