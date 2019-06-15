using BizObjects.Converters;
using LineParser;
using LineParser.Parsers;

namespace BizObjectsTests
{
    public abstract class ParserTestBase
    {
        protected static readonly YouResolver YouAre = new YouResolver("Khadaji");
        protected static readonly LineParserFactory _parser = new LineParserFactory();
        private readonly IParser _hitParser = new HitParser(YouAre);
        private readonly IParser _missParser = new MissParser(YouAre);
        private readonly IParser _healParser = new HealParser(YouAre);
        private readonly IParser _killParser = new KillParser(YouAre);
        private readonly IParser _whoParser = new WhoParser(YouAre);

        public ParserTestBase()
        {
            _parser.AddParser(_hitParser, null);
            _parser.AddParser(_missParser, null);
            _parser.AddParser(_healParser, null);
            _parser.AddParser(_killParser, null);
            _parser.AddParser(_whoParser, null);
        }
    }
}
