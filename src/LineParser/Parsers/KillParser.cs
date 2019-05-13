using BizObjects;
using LogObjects;

namespace LineParser.Parsers
{
    public class KillParser : IParser
    {
        private const string OtherDeath = " has been slain by ";
        private const string YourKill = "You have slain ";
        private const string YourDeath = "You have been slain by ";
        private const string SomeoneDied = " died.";
        private readonly YouResolver YouAre;

        public KillParser(YouResolver youAre)
        {
            YouAre = youAre;
        }

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
                defender = logDatum.LogMessage.Substring(0, i);
                attacker = logDatum.LogMessage.Substring(i + OtherDeath.Length, logDatum.LogMessage.Length - i - OtherDeath.Length - 1);
                lineEntry = new Kill(logDatum, YouAre.WhoAreYou(attacker), YouAre.WhoAreYou(defender), "slain");
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
                defender = logDatum.LogMessage.Substring(i + YourKill.Length, logDatum.LogMessage.Length - i - YourKill.Length - 1);
                attacker = YouAre.Name;
                lineEntry = new Kill(logDatum, attacker, YouAre.WhoAreYou(defender), "slain");
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
                defender = YouAre.Name;
                attacker = logDatum.LogMessage.Substring(i + YourDeath.Length, logDatum.LogMessage.Length - i - YourDeath.Length - 1);
                lineEntry = new Kill(logDatum, YouAre.WhoAreYou(attacker), defender, "slain");
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
                defender = logDatum.LogMessage.Substring(0, i);
                attacker = Attack.Unknown;
                lineEntry = new Kill(logDatum, YouAre.WhoAreYou(attacker), YouAre.WhoAreYou(defender), "died");
                return true;
            }

            return false;
        }
    }
}
