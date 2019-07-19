using System.Threading;
using System.Threading.Tasks;
using BizObjects.Battle;

namespace EqbConsole
{
    public interface IJobProcessor
    {
        Task StartProcessingJobAsync(string logFilePath, Battle eqBattle);
        CancellationTokenSource CancelSource { get; set; }
    }
}
