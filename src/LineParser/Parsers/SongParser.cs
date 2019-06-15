using System;
using System.Text.RegularExpressions;
using BizObjects.Converters;
using BizObjects.Lines;
using LogObjects;

namespace LineParser.Parsers
{
    public class SongParser : IParser
    {
        private readonly Regex RxSong = new Regex(@"(.+) begins to sing a song. <?(.+)>", RegexOptions.Compiled); // https://regex101.com/r/u6rBj3/2/

        private readonly YouResolver YouAre;

        public SongParser(YouResolver youAre)
        {
            YouAre = youAre;
        }

        public bool TryParse(LogDatum logDatum, out ILine lineEntry)
        {
            if (TryParseSong(logDatum, out lineEntry))
                return true;

            return false;
        }

        private bool TryParseSong(LogDatum logDatum, out ILine lineEntry)
        {
            var match = RxSong.Match(logDatum.LogMessage);

            if (!match.Success)
            {
                lineEntry = null;
                return false;
            }

            var name = match.Groups[1].Value;
            var songName = match.Groups[2].Value;

            lineEntry = new Song(logDatum, YouAre.WhoAreYou(name), songName);

            return true;
        }
    }
}
