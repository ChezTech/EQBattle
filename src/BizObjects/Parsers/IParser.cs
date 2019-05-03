using LogObjects;

namespace BizObjects.Parsers
{
    public interface IParser
    {
        bool TryParse(LogDatum logDatum, out Line lineEntry);
    }
}