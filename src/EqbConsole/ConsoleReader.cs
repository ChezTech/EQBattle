using System;
using System.Threading;
using System.Threading.Tasks;

namespace EqbConsole
{
    public class ConsoleReader
    {
        public int KeyDelayMilliSeconds { get; set; } = 50;
        public async Task<ConsoleKeyInfo> ReadKeyAsync(bool intercept = false, CancellationToken cancellationToken = default)
        {
            // Async loop while we wait for the user to press a key
            while (!Console.KeyAvailable && !cancellationToken.IsCancellationRequested)
                await Task.Delay(KeyDelayMilliSeconds, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            // On MacOs & Linux, the terminal echoes the key before dotnet ReadKey() has a chance to swallow it
            // when using this method of checking if a key is avail, before reading it.
            // https://github.com/dotnet/corefx/issues/30610, et al
            return Console.ReadKey(intercept);
        }
    }
}
