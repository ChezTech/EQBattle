using System;
using System.Threading;
using System.Threading.Tasks;
using BizObjects.Battle;
using LineParser;

namespace EQJobService
{
    public abstract class JobProcessor : IJobProcessor
    {
        public abstract Task StartProcessingJobAsync(string logFilePath, Battle eqBattle);

        protected readonly LineParserFactory _parser;
        protected readonly int _parserCount;

        public event Action StartReading;
        public event Action EoFReached;
        public event Action EoFBattle;

        protected virtual void OnStartReading() { StartReading?.Invoke(); }
        protected virtual void OnEoFReached() { EoFReached?.Invoke(); }
        protected virtual void OnEoFBattle() { EoFBattle?.Invoke(); }

        public CancellationTokenSource CancelSource { get; set; }

        public virtual void ShowStatus() { }

        public JobProcessor(LineParserFactory parser, int parserCount = 1)
        {
            _parser = parser;
            _parserCount = parserCount;
        }
    }
}
