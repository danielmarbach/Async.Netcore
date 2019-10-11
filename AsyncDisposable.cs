using System;
using System.IO;
using System.Threading.Tasks;

class AsyncDisposable : IRunnable
{
    public async Task Run()
    {
        using (var wrong = new WrongDisposable())
        {
        }

        await using (var correct = new CorrectDisposable()
            .ConfigureAwait(false))
        {
        }

        // using (var correct = new CorrectDisposable())
        // {
        // }
    }

    class WrongDisposable : DisposableBase, IDisposable
    {
        public void Dispose()
        {
            Log();

            // sync over async anti-pattern
            stream.FlushAsync().GetAwaiter().GetResult();
            stream.Dispose();
        }
    }

    class CorrectDisposable : DisposableBase, IAsyncDisposable, IDisposable
    {
        public async ValueTask DisposeAsync()
        {
            Log();

            await stream.FlushAsync().ConfigureAwait(false);
            stream.Dispose();
            // or
            await stream.DisposeAsync().ConfigureAwait(false);
        }

        public void Dispose()
        {
            Log();

            stream.Flush();
            stream.Dispose();
        }
    }
}
