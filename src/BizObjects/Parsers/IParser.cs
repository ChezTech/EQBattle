using LogObjects;

namespace BizObjects.Parsers
{
    public interface IParser
    {
        bool TryParse(LogDatum logDatum, out ILine lineEntry);
    }
}
