
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace EqbConsole
{
    public class ChannelProcessor
    {
        private CancellationToken _token;

        public event Action<string> Message;
        public ChannelProcessor(CancellationToken token)
        {
            _token = token;
        }

        public async Task Process<T>(ChannelReader<T> reader, Action<T> transformAction, string title = "")
        {
            RaiseMessage($"Reading channel: {title}");
            try
            {
                while (await reader.WaitToReadAsync(_token))
                {
                    while (!_token.IsCancellationRequested && reader.TryRead(out var item))
                    {
                        transformAction(item);
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                RaiseMessage($"ChannelProcessor: {title} Operation Cancelled: {ex.GetType().Name} - {ex.Message}");
                throw;
            }
            finally
            {
                RaiseMessage($"Done reading channel: {title}");
            }
        }

        public async Task Process<T, U>(ChannelReader<T> reader, ChannelWriter<U> writer, Func<T, U> transformFunc, string title = "")
        {
            RaiseMessage($"Transforming channel: {title}");
            await Process(reader, item =>
            {
                var uItem = transformFunc(item);
                if (uItem != null)
                    writer.TryWrite(uItem);
            }, title);

            RaiseMessage($"Done transforming channel: {title}");
        }

        public async Task Process<T>(ChannelWriter<T> writer, Func<CancellationToken, IEnumerable<T>> transformFunc, int delayTimeMs = 250)
        {
            RaiseMessage("Writing channel");

            // Yield to get off the main thread context right away. This will continue on a thread pool.
            await Task.Yield();
            RaiseMessage("Writing channel - after yield");

            while (!_token.IsCancellationRequested)
            {
                // Get all the items currently available and add them to our channel
                foreach (var item in transformFunc(_token))
                    writer.TryWrite(item);

                delayTimeMs = 15000;
                RaiseMessage($"Waiting to read next source chunk: {delayTimeMs}ms");
                // Wait a bit of time before looking for the next chunk of items
                await Task.Delay(delayTimeMs, _token);
            }
        }

        private void RaiseMessage(string message = "")
        {
            Message?.Invoke(message);
        }
    }
}
