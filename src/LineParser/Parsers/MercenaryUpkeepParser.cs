using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using BizObjects;
using LogObjects;

namespace LineParser.Parsers
{
    public class MercenaryUpkeepParser : IParser
    {
        private readonly Regex RxMercUpkeep = new Regex(@".*(waived|charged).* upkeep cost of (\d+) plat, and (\d+) gold(?: or (\d+) Bayle Mark)?", RegexOptions.Compiled); // https://regex101.com/r/ICtIMR/2

        private readonly YouResolver YouAre;

        public MercenaryUpkeepParser(YouResolver youAre)
        {
            YouAre = youAre;
        }

        public bool TryParse(LogDatum logDatum, out ILine lineEntry)
        {
            if (TryParseMercUpkeep(logDatum, out lineEntry))
                return true;

            return false;
        }

        private bool TryParseMercUpkeep(LogDatum logDatum, out ILine lineEntry)
        {
            var match = RxMercUpkeep.Match(logDatum.LogMessage);

            if (!match.Success)
            {
                lineEntry = null;
                return false;
            }

            var chargeOrWaive = match.Groups[1].Value;
            var plat = int.Parse(match.Groups[2].Value);
            var gold = int.Parse(match.Groups[3].Value);
            var bayleMark = match.Groups[4].Success ? int.Parse(match.Groups[4].Value) : 0;

            decimal cost = plat + gold / 10m;
            var waivedCost = 0m;
            if (chargeOrWaive == "waived")
            {
                waivedCost = cost;
                cost = 0;
            }

            lineEntry = new MercenaryUpkeep(logDatum, cost, waivedCost, bayleMark);

            return true;
        }
    }
}
