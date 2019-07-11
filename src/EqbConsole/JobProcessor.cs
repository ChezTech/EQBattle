using System.Threading.Tasks;
using BizObjects.Battle;
using LineParser;

namespace EqbConsole
{
    public abstract class JobProcessor : IJobProcessor
    {
        public abstract void StartProcessingJob(string logFilePath, Battle eqBattle);
        public abstract Task StartProcessingJobAsync(string logFilePath, Battle eqBattle);

        protected readonly LineParserFactory _parser;
        protected readonly int _parserCount;

        public JobProcessor(LineParserFactory parser, int parserCount = 1)
        {
            _parser = parser;
            _parserCount = parserCount;
        }
    }
}
