using BizObjects.Battle;

namespace EqbConsole
{
    public interface IJobProcessor
    {
        void StartProcessingJob(string logFilePath, Battle eqBattle);
    }
}
