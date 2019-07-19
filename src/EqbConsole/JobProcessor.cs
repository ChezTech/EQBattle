using System.Threading;
using System.Threading.Tasks;
using BizObjects.Battle;
using LineParser;

namespace EqbConsole
{
    public abstract class JobProcessor : IJobProcessor
    {
        public abstract Task StartProcessingJobAsync(string logFilePath, Battle eqBattle);

        protected readonly LineParserFactory _parser;
        protected readonly int _parserCount;

        public CancellationTokenSource CancelSource { get; set; }

        public JobProcessor(LineParserFactory parser, int parserCount = 1)
        {
            _parser = parser;
            _parserCount = parserCount;
        }
    }
}
