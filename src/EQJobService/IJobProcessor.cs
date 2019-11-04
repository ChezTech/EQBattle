using System;
using System.Threading;
using System.Threading.Tasks;
using BizObjects.Battle;

namespace EQJobService
{
    public interface IJobProcessor
    {
        Task StartProcessingJobAsync(string logFilePath, Battle eqBattle);

        CancellationTokenSource CancelSource { get; set; }

        void ShowStatus();

        /// <summary>
        /// Start reading from the file. If this is an active file, this event will be raised againt after EoFReached once there are more lines.
        /// </summary>
        event Action StartReading;

        /// <summary>
        /// All lines have been read from the file. If this is an active file, more lines will be read periodically and this event raised again.
        /// </summary>
        event Action EoFReached;

        /// <summary>
        /// All lines have been processed and added to the Battle. This signifies that reading and parsing of the log file is complete.
        /// If this is an active file, more lines will be read, parsed, and added to the Battle and this event raised again.
        /// </summary>
        event Action EoFBattle;
    }
}
