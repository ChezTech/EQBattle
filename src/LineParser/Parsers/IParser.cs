using BizObjects.Lines;
using LogObjects;

namespace LineParser.Parsers
{
    public interface IParser
    {
        bool TryParse(LogDatum logDatum, out ILine lineEntry);
    }
}
