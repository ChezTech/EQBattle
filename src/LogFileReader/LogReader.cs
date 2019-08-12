using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LogFileReader
{
    public class LogReader
    {
        private readonly CancellationToken _token;

        private readonly FileInfo _logFile;

        public LogReader(string logFilePathName, CancellationToken token)
        {
            _logFile = new FileInfo(logFilePathName);
            if (!_logFile.Exists)
                throw new FileNotFoundException($"File not found: {logFilePathName}", logFilePathName);

            _token = token;
        }

        public event Action<string> LineRead;
        public event Func<Task> EoFReached;

        public async Task StartReadingAsync()
        {
            // Force an early async context switch, otherwise our normal context switch path is only on EOF.
            await Task.Yield();

            await OpenFileToReadAsync();
        }

        private async Task OpenFileToReadAsync()
        {
            _token.ThrowIfCancellationRequested();
            using (var fs = _logFile.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            using (var sr = new StreamReader(fs))
                await ReadUntillCancelledAsync(sr);
        }

        private async Task ReadUntillCancelledAsync(StreamReader sr)
        {
            while (!_token.IsCancellationRequested)
            {
                ReadCurrentLinesUntilEof(sr);
                _token.ThrowIfCancellationRequested();
                await RaiseEof();
            }
        }

        private void ReadCurrentLinesUntilEof(StreamReader sr)
        {
            string logLine;
            while (!_token.IsCancellationRequested && (logLine = sr.ReadLine()) != null)
                RaiseReadLine(logLine);
        }

        private void RaiseReadLine(string logLine)
        {
            LineRead?.Invoke(logLine);
        }

        private async Task RaiseEof()
        {
            // Because this is an async event handler, we need to explicitly grab all the currently registered handlers,
            // then invoke them all and await them all.
            var taskList = EoFReached?.GetInvocationList().Select(h => ((Func<Task>)h)?.Invoke());
            await Task.WhenAll(taskList ?? new Task[0]);
        }
    }
}
